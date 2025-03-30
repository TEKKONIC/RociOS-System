using System;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using NLog;
using ProtoBuf;

namespace RociOS
{
    [ProtoContract] [Serializable]
    public class RociConfig
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const string ConfigFileName = "RociOSConfig.bin";

        [ProtoMember(1)]
        [DefaultValue(true)]
        public bool RociOSEnabled { get; set; } = true;
        
        [ProtoMember(2)]
        [DefaultValue(true)]
        public bool EnableAutoFactionChat { get; set; } = true;
        [ProtoMember(3)]
        [DefaultValue(true)]
        public bool DisableSuitBroadcasting { get; set; } = true;
        [ProtoMember(4)]
        [DefaultValue(true)]
        public bool GrabSingleItem { get; set; } = true;

        public float HudX { get; set; } = -0.4f;
        public float HudY { get; set; } = -0.525f;
        public float HudZ { get; set; } = -0.1f;
        public bool HudVisible { get; set; } = true;

        public static RociConfig Load()
        {
            Log.Info("Attempting to load configuration.");
            try
            {
                if (File.Exists(ConfigFileName))
                {
                    Log.Info($"Config file {ConfigFileName} found. Reading file.");
                    using (var file = File.OpenRead(ConfigFileName))
                    {
                        Log.Info("Config file read successfully. Deserializing Protobuf.");
                        return Serializer.Deserialize<RociConfig>(file);
                    }
                }
                else
                {
                    Log.Warn($"Config file {ConfigFileName} not found. Using default settings.");
                    return new RociConfig();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load config file. Using default settings.");
                return new RociConfig();
            }
        }

        public void Save()
        {
            Log.Info("Attempting to save configuration.");
            try
            {
                using (var file = File.Create(ConfigFileName))
                {
                    Log.Info("Serializing configuration to Protobuf.");
                    Serializer.Serialize(file, this);
                    Log.Info("Config file saved successfully.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save config file.");
            }
        }
    }
}