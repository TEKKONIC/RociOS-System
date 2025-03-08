using System;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using RociOS.Hud;
using NLog.Fluent;
using NLog;
using VRage.Game;

namespace RociOS
{
    public sealed partial class ROSMain
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private TextPage helpMain;
        private RociConfig RociConfig;

        public ROSMain(RociConfig RociConfig)
        {
            this.RociConfig = RociConfig;
        }

        public void InitSettingsMenu()
        {
            try
            {
                Log.Debug("InitSettingsMenu started.");

                RichHudTerminal.Root.Enabled = true;
                
                helpMain = new TextPage()
                {
                    Name = "Help",
                    HeaderText = "RociOS Help",
                    SubHeaderText = "",
                    Text = HelpText.GetHelpMessage(),
                };
            
                RichHudTerminal.Root.AddRange(new IModRootMember[] 
                { 
                    new ControlPage()
                    {
                        Name = "Settings",
                        CategoryContainer = 
                        {
                            GetRociOSHUD(),
                            GetSuitAntennaSettings(),
                            AutoFactionChat(),
                            GetHelpSettings(),
                        },
                    },
                    helpMain,
                });

                Log.Debug("InitSettingsMenu completed.");
            }
            catch (Exception ex)
            {
                Log.Error($"Exception in InitSettingsMenu: {ex.Message}");
                throw;
            }
        }

        private ControlCategory GetRociOSHUD()
        {
            Log.Debug("GetRociOSSettings started.");

            TerminalOnOffButton RociOSHud = new TerminalOnOffButton
            {
                Name = "RociOSystem Hud",
                Value = RociConfig.RociOSEnabled,
                CustomValueGetter = () => RociConfig.RociOSEnabled,
                ControlChangedHandler = (sender, args) => 
                {
                    RociConfig.RociOSEnabled = (sender as TerminalOnOffButton).Value;
                    Log.Debug($"RociOS toggle changed to: {RociConfig.RociOSEnabled}");
                },
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Enables/disables RociOSystem Hud- Features Coming Soon",
                },
            };

            Log.Debug("GetRociOSSettings completed.");

            var HudpositionX = new TerminalSlider()
            {
                Name = "HudpositionX",
                Min = -1f,
                Max = 1f,
                Value = RociConfig.HudX,
                CustomValueGetter = () => (float)RociConfig.HudX,
                ControlChangedHandler = (sender, args) =>
                {
                var slider = sender as TerminalSlider;
                RociConfig.HudX = (float)slider.Value;
                slider.ValueText = RociConfig.HudX.ToString();
                },
                ToolTip = new RichText(ToolTip.DefaultText)
                { 
                     "x axis of the RociOS Hud Terminal"
                },
            };

            var HudpositionY = new TerminalSlider()
            {
                Name = "HudpositionY",
                Min = -1f,
                Max = 1f,
                Value = RociConfig.HudY,
                CustomValueGetter = () => (float)RociConfig.HudY,
                ControlChangedHandler = (sender, args) =>
                {
                    var slider = sender as TerminalSlider;
                    RociConfig.HudY = (float)slider.Value;
                    slider.ValueText = RociConfig.HudY.ToString();
                },
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "y axis of the RociOS Hud Terminal"
                }
            };

            var HudpositionZ = new TerminalSlider()
            {
                Name = "HudpositionZ",
                Min = -1f,
                Max = 1f,
                Value = RociConfig.HudZ,
                CustomValueGetter = () => (float)RociConfig.HudZ,
                ControlChangedHandler = (sender, args) =>
                {
                    var slider = sender as TerminalSlider;
                    RociConfig.HudZ = (float)slider.Value;
                    slider.ValueText = RociConfig.HudZ.ToString();
                },
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Z axis of the RociOS Hud Terminal"
                }
            };

            return new ControlCategory
            {
                HeaderText = "RociOSystem Hud Interface",
                SubheaderText = "Configure RociOSystem Hud -  Features Coming Soon",
                TileContainer =
                {
                    new ControlTile { RociOSHud },
                    new ControlTile() { HudpositionX, HudpositionY, HudpositionZ },
                },
            };
        }

        private ControlCategory GetSuitAntennaSettings()
        {
            var SuitAntennaToggleBox = new TerminalOnOffButton()
            {
                Name = "Auto Suit Antenna Disabler",
                Value = RociConfig.DisableSuitBroadcasting,
                CustomValueGetter = () => RociConfig.DisableSuitBroadcasting,
                ControlChangedHandler = (sender, args) =>
                {
                    RociConfig.DisableSuitBroadcasting = (sender as TerminalOnOffButton).Value;
                    Log.Debug($"Suit antenna disabler changed to: {RociConfig.DisableSuitBroadcasting}");
                    
                    OnDisableSuitBroadcastingChanged(RociConfig.DisableSuitBroadcasting);
                },
                ToolTip = new RichText("Disables suits broadcasting Disabler")
            };

            return new ControlCategory()
            {
                HeaderText = "Auto Disable Suit Antenna",
                SubheaderText = "Configures the Auto Disable Suit Antenna Function",
                TileContainer =
                {
                    new ControlTile() { SuitAntennaToggleBox },
                },
            };
        }

        private void OnDisableSuitBroadcastingChanged(bool isDisabled)
        {
            
            if (isDisabled)
            {
                RociOS.utilities.RociSession.SetAntennaDisabledFlag(true);
                Log.Info("Suit broadcasting is now disabled.");
            }
            else
            {
                RociOS.utilities.RociSession.SetAntennaDisabledFlag(false);
                Log.Info("Suit broadcasting is now enabled");
            }
        }

        private ControlCategory AutoFactionChat()
        {
            var AutoFactionChatToggleBox = new TerminalOnOffButton()
            {
                Name = "Auto Faction Chat",
                Value = RociConfig.EnableAutoFactionChat,
                CustomValueGetter = () => RociConfig.EnableAutoFactionChat,
                ControlChangedHandler = ((sender, args) =>
                {
                    RociConfig.EnableAutoFactionChat = (sender as TerminalOnOffButton).Value;
                    Log.Debug($"Auto Faction Chat changed to: {RociConfig.EnableAutoFactionChat}");
                    
                    OnDisableAutoFactionChatChanged(RociConfig.EnableAutoFactionChat);
                }),
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Enables auto faction chat System"
                }
            };

            return new ControlCategory()
            {
                HeaderText = "Auto Faction Chat",
                SubheaderText = "Configures the Auto Faction Function",
                TileContainer =
                {
                    new ControlTile() { AutoFactionChatToggleBox }
                },
            };
        }

        private void OnDisableAutoFactionChatChanged(bool isAFactionChatDisabled)
        {
            if (isAFactionChatDisabled)
            {
                RociOS.utilities.MyChatSystemPatch.SetAutoFactionChatEnabled(true);
                Log.Info("Auto Faction Chat is now enabled");
            }
            else
            {
                RociOS.utilities.MyChatSystemPatch.SetAutoFactionChatEnabled(false);
                Log.Info("Auto Faction Chat is now disabled.");
            }
        }
        
        private ControlCategory GetHelpSettings()
        {
            var openHelp = new TerminalButton()
            {
                Name = "Open Help",
                ControlChangedHandler = (sender, args) => RichHudTerminal.OpenToPage(helpMain)
            };

            var title1 = new ControlTile()
            {
                openHelp,
            };
            

            var loadCfg = new TerminalButton()
            {
                Name = "Load config",
                ControlChangedHandler = (sender, args) => RociConfig.Load()
            };

            var saveCfg = new TerminalButton()
            {
                Name = "Save config",
                ControlChangedHandler = (sender, args) => RociConfig.Save()
            };

            var resetCfg = new TerminalButton()
            {
                Name = "Reset config",
                ControlChangedHandler = (sender, args) => 
                    {}
            };

            var tile2 = new ControlTile()
            {
                loadCfg,
                saveCfg,
                resetCfg
            };

            return new ControlCategory()
            {
                HeaderText = "Help",
                SubheaderText = "Help text and Config Control",
                TileContainer = { title1, tile2 },
            };
        }
    }  
}


