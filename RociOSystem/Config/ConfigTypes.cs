using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using System.Xml.Serialization;
using VRage.Input;
using VRageMath;

namespace RociOS.Config
{
    [XmlRoot, XmlType(TypeName = "RociOS Settings")] 
    public class RociConfig : ConfigRoot<RociConfig>
    {
        public static bool WasConfigOld { get; private set; }
        private const int vID = 11;

        protected override RociConfig GetDefaults()
        {
            return new RociConfig
            {
                VersionID = vID,
            };
        }
    }
}