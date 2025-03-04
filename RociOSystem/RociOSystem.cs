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
    public class RociOSystem : IPlugin, IDisposable 
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        private readonly RociConfig RociConfig;
        
        public static string ChatName => "RociOSystem";
        
        private static RociOSystem Instance { get; set; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public RociOSystem()
        {
            try
            {
                // Configure NLog programmatically
                var loggingConfig = new LoggingConfiguration();

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

                loggingConfig.AddTarget(consoleTarget);
                loggingConfig.AddTarget(fileTarget);

                loggingConfig.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);
                loggingConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);

                LogManager.Configuration = loggingConfig;

                Log.Info("RociOS constructor called.");
                this.RociConfig = RociConfig.Load(); 
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
            RociOSystem.Instance = this;

            if (RociConfig == null)
            {
                Log.Error("Configuration is null. Initialization cannot proceed.");
                throw new InvalidOperationException("Configuration is null.");
            }

            if (RociConfig.RociOSEnabled)
            {
                new Harmony("RociOSHud").PatchAll(Assembly.GetExecutingAssembly());
                Log.Info("RociOS Hud enabled.");
            }

            if (RociConfig.EnableAutoFactionChat)
            {
                new Harmony("RociOSession").PatchAll(Assembly.GetExecutingAssembly());
                Log.Info("AutoFactionChat loaded");
            }

            if (RociConfig.GrabSingleItem)
            {
                Log.Debug("GrabSingleItem: Patching");
                new Harmony("GrabSingleItem").PatchAll(Assembly.GetExecutingAssembly());
                Log.Info("GrabSingleItem: Patches applied");
            }
        }
        
        public void Dispose() => RociOSystem.Instance = null;

        public void Update()
        { }
    }
}