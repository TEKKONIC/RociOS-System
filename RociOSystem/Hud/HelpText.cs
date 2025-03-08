using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RociOS.Hud
{
    public static partial class HelpText
    {
        public static string controlList;
        private static readonly Color accentGrey = new Color(200, 200, 210);

        private static GlyphFormat
            highlight = new GlyphFormat(accentGrey),
            subheader = new GlyphFormat(accentGrey, TextAlignment.Center, 1.1f, FontStyles.Underline),
            subsection = new GlyphFormat(color:accentGrey, style: FontStyles.Underline);
        
        public static RichText GetHelpMessage()
        {
            return new RichText(GlyphFormat.White)
            {
                { " Update 1.1:\n\n", subheader },
                
                { "Settings Menu:\n\n", subheader},

                $"By default, this settings menu can be toggled by pressing ", { "F2.", highlight }, " From here, you can configure block ",
                
                { "Chat Commands:\n\t", subheader},

                $"The chat commands in this mod are largely redundant; the functionality they provide is also provided by the settings menu. ",
                $"If you prefer to use the chat commands for whatever reason, here they are:\n\n",

                $"All chat commands must begin with ", { "“/Roci”", highlight }, "and are not case-sensitive. The arguments following ", { "“/Roci”", highlight }, 
                " can be separated either by whitespace, a comma, semicolon or pipe character. Whatever floats your boat; just make sure there’s something between them.\n\n",

                $"• help:\t\t\tYou are here.\n",
                $"• save:\t\t\tSaves the current configuration\n",
                $"• load:\t\t\tLoads configuration from the config file\n\n\n",

                $"Example: \"/Roci Disable RociOS \"\n\n",
            };
        }
    }
}