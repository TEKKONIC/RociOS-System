using System;
using System.Collections.Generic;
using RichHudFramework.IO;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using RociOS.Hud;
using NLog.Fluent;

namespace RociOS
{
    public sealed partial class ROSMain
    {
        private static TextPage helpMain;

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
                Value =  RociOSConfig.RociOSEnabled,
                CustomValueGetter = () => RociOSConfig.RociOSEnabled,
                ControlChangedHandler = (sender, args) => RociOSConfig.RociOSEnabled = (sender as TerminalOnOffButton).Value,
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

        private static ControlCategory GetSuitAntennaSettings()
        {
            var SuitAntennaToggleBox = new TerminalOnOffButton()
            {
                Name = "Auto Suit Antenna Disabler ",
                Value = RociOSConfig.DisableSuitBroadcasting,
                CustomValueGetter = () => RociOSConfig.DisableSuitBroadcasting,
                ControlChangedHandler = (RichHudFramework.EventHandler) ((sender, args) => RociOSConfig.DisableSuitBroadcasting = (sender as TerminalOnOffButton).Value),
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

        private static ControlCategory AutoFactionChat()
        {
            var AutoFactionChatToggleBox = new TerminalOnOffButton()
            {
                Name = "Auto Faction Chat",
                Value = RociOSConfig.EnableAutoFactionChat,
                CustomValueGetter = () => RociOSConfig.EnableAutoFactionChat,
                ControlChangedHandler = ((sender, args) => RociOSConfig.EnableAutoFactionChat = (sender as TerminalOnOffButton).Value),
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
        
        private static ControlCategory GetHelpSettings()
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


