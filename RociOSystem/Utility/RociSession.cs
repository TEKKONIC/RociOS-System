using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Draygo.API;
using Epic.OnlineServices;
using RichHudFramework.Client;
using RichHudFramework.Internal;
using VRage;
using VRage.Game.Components;
using VRage.Game.Entity;
using RociOS;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using VRage.Utils;
using VRage.Game.ModAPI;
using NLog;
using Sandbox.Game.Entities.Character;
using NLog.Fluent;
using VRage.Game;

namespace RociOS.utilities
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public partial class RociSession : MySessionComponentBase
    {
        bool antennaDisabled = false;
        bool characterHasBeenDead = false;
        bool hasLoggedCharacterWarning = false;
        bool hasLoggedSessionWarning = false; 
        bool hasLoggedConfigWarning = false; 
        bool hasLoggedUpdateBeforeSimulation = false;
        bool hasLoggedRociOSEnabled = false;
        private ROSMain rosMain;
        private static RociConfig RociConfig;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static bool antennaDisabledFlag = false;
        private HudAPIv2 hudAPI;
        private HudAPIv2.HUDMessage hudMessage;

        public static void SetAntennaDisabledFlag(bool isDisabled)
        {
            antennaDisabledFlag = isDisabled;
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            BeforeInit();
            loadRociConfig(); 
        }

        public override void UpdateBeforeSimulation()
        {
            try
            {
                // Check if the Suit antenna disabler is set to true
                if (RociConfig == null || !RociConfig.DisableSuitBroadcasting)
                {
                    return; // Exit early if the setting is not enabled
                }

                if (!hasLoggedUpdateBeforeSimulation)
                {
                    Log.Info("UpdateBeforeSimulation called.");
                    hasLoggedUpdateBeforeSimulation = true;
                }

                if (RociConfig != null && RociConfig.RociOSEnabled)
                {
                    if (!hasLoggedRociOSEnabled)
                    {
                        Log.Info("RociOSEnabled is true.");
                        hasLoggedRociOSEnabled = true;
                    }
                    hasLoggedConfigWarning = false; 
                    var session = MyAPIGateway.Session;
                    if (session?.Player?.Controller != null && session.Player.Character != null)
                    {
                        hasLoggedSessionWarning = false; 
                        var character = session.Player.Character as MyCharacter;
                        if (character != null && !character.IsDead)
                        {
                            if (antennaDisabledFlag && !antennaDisabled)
                            {
                                character.EnableBroadcasting(false);
                                character.RequestEnableBroadcasting(false);
                                Log.Info("Disabled suit antenna for player: " + character.DisplayName);
                                MyAPIGateway.Utilities.ShowNotification("Suit antenna disabled.", 15000, VRage.Game.MyFontEnum.White);
                                antennaDisabled = true;
                            }
                            else if (!antennaDisabledFlag && antennaDisabled)
                            {
                                character.EnableBroadcasting(true);
                                character.RequestEnableBroadcasting(true);
                                Log.Info("Enabled suit antenna for player: " + character.DisplayName);
                                MyAPIGateway.Utilities.ShowNotification("Suit antenna enabled.", 15000, VRage.Game.MyFontEnum.White);
                                antennaDisabled = false;
                            }
                            hasLoggedCharacterWarning = false; 
                        }
                        else
                        {
                            if (!hasLoggedCharacterWarning)
                            {
                                Log.Warn("Character is null or dead.");
                                hasLoggedCharacterWarning = true; 
                            }
                        }
                    }
                    else
                    {
                        if (!hasLoggedSessionWarning)
                        {
                            Log.Warn("Session, Player, or Character is null.");
                            hasLoggedSessionWarning = true; 
                        }
                    }
                }
                else
                {
                    if (!hasLoggedConfigWarning)
                    {
                        Log.Warn("RociConfig is null or RociOSEnabled is false.");
                        hasLoggedConfigWarning = true; 
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in UpdateBeforeSimulation");
            }
            finally
            {
                if (MyAPIGateway.Session?.Player?.Character == null || MyAPIGateway.Session.Player.Character.IsDead)
                {
                    characterHasBeenDead = true;
                    antennaDisabled = false;
                }
            }
        }
            
        public virtual void BeforeInit()
        {
            try
            {
                RichHudClient.Init("RociOS System", new Action(this.InitCallback), new Action(this.ResetCallback));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in BeforeInit");
            }
        }
        
        private void ResetCallback()
        {}

        public void InitCallback()
        {
            try
            {
                rosMain = new ROSMain(new RociConfig());
                rosMain.InitSettingsMenu();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in InitCallback");
            }
        }

        public void loadRociConfig()
        {
            string filename = "RociConfig.cfg";

            try
            {
                if (MyAPIGateway.Utilities.FileExistsInLocalStorage(filename, typeof(RociConfig)))
                {
                    using (TextReader reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(filename, typeof(RociConfig)))
                    {
                        RociSession.RociConfig = MyAPIGateway.Utilities.SerializeFromXML<RociConfig>(reader.ReadToEnd());
                    }
                    MyLog.Default.WriteLine(RociOSystem.ChatName + "Loaded config.");
                    Log.Info("RociConfig loaded successfully.");
                }
                else
                {
                    RociSession.RociConfig = new RociConfig();
                    WriteRociConfig(RociSession.RociConfig);
                    Log.Info("RociConfig file not found. Created new config.");
                }
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLine(RociOSystem.ChatName + "Error reading config file, using defaults - " + ex);
                RociSession.RociConfig = new RociConfig();
                WriteRociConfig(RociSession.RociConfig); // Ensure a valid config is saved.
                Log.Error(ex, "Error reading config file, using defaults.");
            }

            // Log the value of RociOSEnabled
            if (RociSession.RociConfig != null)
            {
                Log.Info("RociOSEnabled: " + RociSession.RociConfig.RociOSEnabled);
            }
            else
            {
                Log.Warn("RociConfig is null after loading.");
            }
        }

        public static void WriteRociConfig(RociConfig config)
        {
            string filename = "RociConfig.cfg";

            try
            {
                using (TextWriter writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(filename, typeof(RociConfig)))
                {
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML(config));
                }
                MyLog.Default.WriteLine(RociOSystem.ChatName + "Wrote roci config.");
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLine(RociOSystem.ChatName + "Error writing roci config - " + ex);
            }
        }

        protected new virtual void UnloadData()
        {
            try
            {
                if (RichHudClient.Instance != null)
                {
                    RichHudClient.Instance.Close();
                }

                if (RichHudCore.Instance != null)
                {
                    RichHudCore.Instance.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error during UnloadData: " + ex.Message);
            }
        }
    }
}