using System;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using RociOS.Hud;
using NLog.Fluent;
using NLog;

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
                            GetRociOSSettings(),
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

        private ControlCategory GetRociOSSettings()
        {
            Log.Debug("GetRociOSSettings started.");

            TerminalOnOffButton RociOSToggleBox = new TerminalOnOffButton
            {
                Name = "RociOS toggle",
                Value = RociConfig.RociOSEnabled,
                CustomValueGetter = () => RociConfig.RociOSEnabled,
                ControlChangedHandler = (sender, args) => 
                {
                    RociConfig.RociOSEnabled = (sender as TerminalOnOffButton).Value;
                    Log.Debug($"RociOS toggle changed to: {RociConfig.RociOSEnabled}");
                },
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Enables/disables RociOSystem"
                },
            };

            Log.Debug("GetRociOSSettings completed.");

            return new ControlCategory
            {
                HeaderText = "RociOSToggle",
                SubheaderText = "Configure RociOS Plugin",
                TileContainer =
                {
                    new ControlTile { RociOSToggleBox }
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
                    
                    // Trigger an event or callback to apply the changes immediately
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
                    new ControlTile() { SuitAntennaToggleBox }
                },
            };
        }

        // Define the event or callback method
        private void OnDisableSuitBroadcastingChanged(bool isDisabled)
        {
            // Implement the logic to apply the changes immediately
            // For example, update the game state or notify other components
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
                // Implement the logic when Auto Faction Chat is disabled
                Log.Info("Auto Faction Chat is now disabled.");
            }
            else
            {
                // Implement the logic when Auto Faction Chat is enabled
                Log.Info("Auto Faction Chat is now enabled");
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
            

            var title2 = new ControlTile()
            {
                //loadConfig,
                //saveConfig,
                //resetConfig,
            };

            return new ControlCategory()
            {
                HeaderText = "Help",
                SubheaderText = "Help text and Config Control",
                TileContainer = { title1, title2 }
            };
        }
    }  
}


