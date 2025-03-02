using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using RichHudFramework.IO;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using RociOS.Hud;
using NLog.Fluent;

namespace RociOS
{
    public sealed partial class ROSMain
    {
        private TextPage helpMain;
        private Config config;

        public ROSMain(Config config)
        {
            this.config = config;
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
            TerminalOnOffButton RociOSToggleBox = new TerminalOnOffButton
            {
                Name = "RociOS toggle",
                Value = config.RociOSEnabled,
                CustomValueGetter = () => config.RociOSEnabled,
                ControlChangedHandler = (sender, args) => config.RociOSEnabled = (sender as TerminalOnOffButton).Value,
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Enables/disables RociOSystem"
                },
            };

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
                Name = "Auto Suit Antenna Disabler ",
                Value = config.DisableSuitBroadcasting,
                CustomValueGetter = () => config.DisableSuitBroadcasting,
                ControlChangedHandler = (RichHudFramework.EventHandler) ((sender, args) => config.DisableSuitBroadcasting = (sender as TerminalOnOffButton).Value),
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Disables suits broadcasting Disabler"
                }
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

        private ControlCategory AutoFactionChat()
        {
            var AutoFactionChatToggleBox = new TerminalOnOffButton()
            {
                Name = "Auto Faction Chat",
                Value = config.EnableAutoFactionChat,
                CustomValueGetter = () => config.EnableAutoFactionChat,
                ControlChangedHandler = ((sender, args) => config.EnableAutoFactionChat = (sender as TerminalOnOffButton).Value),
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


