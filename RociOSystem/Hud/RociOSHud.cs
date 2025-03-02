using System;
using NLog.Fluent;
using RichHudFramework.Client;
using RichHudFramework.Internal;
using RichHudFramework.IO;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Input;
using VRageMath;

namespace RociOS.Hud
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class RociOSHud : ModBase
    {
        public const string modName = "RociOS System";
        
        public static RociOSHud Instance { get; private set; }
        
        private ROSMain rosMain;

        public RociOSHud() : base(true, true)
        {
            if (Instance == null)
                Instance = this;

            try
            {
                Log.Debug("RociOSHud constructor started.");
                
                ExceptionHandler.ModName = modName;
                ExceptionHandler.PromptForReload = true;
                ExceptionHandler.RecoveryLimit = 3;

                Log.Debug("RociOSHud constructor completed.");
            }
            catch (Exception ex)
            {
                Log.Debug($"Exception in RociOSHud constructor: {ex.Message}");
                throw;
            }
        }

        protected override void AfterInit()
        {
            if (ExceptionHandler.IsClient)
            {
                CanUpdate = false;
                RichHudClient.Init(ExceptionHandler.ModName, HudInit, OnHudReset);
            }
        }

        private void HudInit()
        {
            if (ExceptionHandler.IsClient)
            {
                CanUpdate = true;
                
                Log.Info("Initializing Hud");

                rosMain = new ROSMain(new Config());
                rosMain.InitSettingsMenu();
            }
        }
        
        private void OnHudReset() { }
        
        public override void BeforeClose()
        {
            if (ExceptionHandler.IsClient)
            {
                
            }

            if (ExceptionHandler.Unloading)
            {
                Instance = null;
            }
        }
        
        private void ClientReset()
        {
            /* At this point, your client has been unregistered and all of
            your framework members will stop working.

            This will be called in one of three cases:
            1) The game session is unloading.
            2) An unhandled exception has been thrown and caught on either the client
            or on master.
            3) RichHudClient.Reset() has been called manually.
            */
        }
    }
}