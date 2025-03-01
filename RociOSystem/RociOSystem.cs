using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using NLog;
using NLog.Config;
using NLog.Targets;
using VRage.FileSystem;
using VRage.Plugins;


namespace RociOS
{
    public class RociOSPlugin : IPlugin, IDisposable 
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        private readonly RociOSConfig config;
        
        private static RociOSPlugin Instance { get; set; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public RociOSPlugin()
        {
            try
            {
                // Configure NLog programmatically
                var config = new LoggingConfiguration();

                var consoleTarget = new ConsoleTarget("console")
                {
                    Layout = "${longdate} ${level:uppercase=true} ${message} ${exception:format=toString,StackTrace}"
                };

                //log file path
                string logDirectory = MyFileSystem.UserDataPath;
                string logFilePath = Path.Combine(logDirectory, "RociOS.log");

                var fileTarget = new FileTarget("file")
                {
                    FileName = logFilePath,
                    Layout = "${longdate} ${level:uppercase=true} ${message} ${exception:format=toString,StackTrace}",
                    ArchiveOldFileOnStartup = true,
                    ArchiveFileName = logFilePath
                };

                config.AddTarget(consoleTarget);
                config.AddTarget(fileTarget);

                config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);
                config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);

                LogManager.Configuration = config;

                Log.Info("RociOS constructor called.");
                this.config = RociOSConfig.Load(); 
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize RociOS plugin.");
                throw;
            }
        }

        public void Init(object gameInstance)
        {
            Log.Info("==== RociOS Plugin Online ====");
            RociOSPlugin.Instance = this;

            if (config == null)
            {
                Log.Error("Configuration is null. Initialization cannot proceed.");
                throw new InvalidOperationException("Configuration is null.");
            }

            if (RociOSConfig.RociOSEnabled)
            {
                new Harmony("RociOSHud").PatchAll(Assembly.GetExecutingAssembly());
                Log.Info("RociOS Hud enabled.");
            }

            if (RociOSConfig.EnableAutoFactionChat)
            {
                new Harmony("RociOSession").PatchAll(Assembly.GetExecutingAssembly());
                Log.Info("AutoFactionChat loaded");
            }

            if (config.GrabSingleItem)
            {
                Log.Debug("GrabSingleItem: Patching");
                new Harmony("GrabSingleItem").PatchAll(Assembly.GetExecutingAssembly());
                Log.Info("GrabSingleItem: Patches applied");
            }
        }
        
        public void Dispose() => RociOSPlugin.Instance = null;

        public void Update()
        { }
    }
}