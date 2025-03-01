using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using RichHudFramework.Internal;
using RichHudFramework.UI.Client;

namespace RichHudFramework.IO
{
    public abstract class Config<ConfigT> where ConfigT : Config<ConfigT>, new()
    {
        public static ConfigT Defaults
        {
            get
            {
                if (defaultInstance == null)
                    defaultInstance = new ConfigT();

                return defaultInstance.GetDefaults();
            }
        }
        private static ConfigT defaultInstance;
        public virtual void Validate() { }
        protected abstract ConfigT GetDefaults();
    }

    public abstract class ConfigRoot<ConfigT> : Config<ConfigT> where ConfigT : ConfigRoot<ConfigT>, new()
    {
        
        [XmlAttribute("ConfigVersionID")]
        public virtual int VersionID { get; set; }
        
        public static event Action OnConfigSave, OnConfigLoad;
        
        public static string FileName { get { return ConfigIO.FileName; } set { ConfigIO.FileName = value; } }
        public static ConfigT Current
        {
            get { if (current == null) Load(); return current; }
            private set { current = value; }
        }
        private static ConfigT current;
        
        public static void Load(bool silent = true)
        {
            Current = ConfigIO.Instance.Load(silent);
            OnConfigLoad?.Invoke();
        }
        
        public static void LoadStart(bool silent = false) =>
            LoadStart(null, silent);
        
        public static void LoadStart(Action Callback, bool silent = false)
        {
            ConfigIO.Instance.LoadStart((ConfigT value) =>
            {
                Current = value;
                OnConfigLoad?.Invoke();
                Callback?.Invoke();
            }, silent);
        }
        
        public static void Save()
        {
            OnConfigSave?.Invoke();
            ConfigIO.Instance.Save(Current);
        }
        
        public static void SaveStart(bool silent = false)
        {
            OnConfigSave?.Invoke();
            ConfigIO.Instance.SaveStart(Current, silent);
        }
        
        public static void ResetConfig(bool silent = false)
        {
            ConfigIO.Instance.SaveStart(Defaults, silent);
            Current = Defaults;
            OnConfigLoad?.Invoke();
        }
        
        public static void ClearSubscribers()
        {
            OnConfigLoad = null;
            OnConfigSave = null;
        }

        internal sealed class ConfigIO : RichHudParallelComponentBase
        {
            public bool SaveInProgress { get; private set; }

            public static ConfigIO Instance
            {
                get
                {
                    if (_instance == null)
                        _instance = new ConfigIO();
                    else if (_instance.Parent == null && RichHudCore.Instance != null)
                        _instance.RegisterComponent(RichHudCore.Instance);

                    return _instance;
                }
                private set { _instance = value; }
            }

            private static ConfigIO _instance;

            public static string FileName
            {
                get { return fileName; }
                set
                {
                    if (value != null && value.Length > 0) fileName = value;
                }
            }

            private static string fileName = $"{typeof(ConfigT).Name}.xml";

            private readonly LocalFileIO cfgFile;

            private ConfigIO() : base(true, true)
            {
                cfgFile = new LocalFileIO(FileName);
                SaveInProgress = false;
            }

            protected override void ErrorCallback(List<KnownException> known, AggregateException unknown)
            {
                if (known != null && known.Count > 0)
                {
                    SaveInProgress = false;
                    string exceptions = "";

                    foreach (Exception e in known)
                    {
                        ExceptionHandler.SendChatMessage(e.Message);
                        exceptions += e.ToString();
                    }

                    ExceptionHandler.WriteToLog(exceptions);
                }

                if (unknown != null)
                {
                    ExceptionHandler.WriteToLog("\nConfig operation failed.\n" + unknown.ToString());
                    ExceptionHandler.SendChatMessage("Config operation failed.");
                    SaveInProgress = false;

                    throw unknown;
                }
            }

            public ConfigT Load(bool silent = false)
            {
                ConfigT cfg = null;
                KnownException loadException = null;

                if (!SaveInProgress)
                {
                    SaveInProgress = true;

                    if (!silent)
                        ExceptionHandler.SendChatMessage("Loading configuration...");

                    if (cfgFile.FileExists)
                    {
                        loadException = TryLoad(out cfg);
                        cfg = ValidateConfig(cfg);
                    }
                    else
                        cfg = Defaults;

                    TrySave(cfg);

                    if (loadException != null)
                        ExceptionHandler.WriteToLog(loadException.ToString());
                    else if (!silent)
                        ExceptionHandler.SendChatMessage("Configuration loaded.");
                }
                else
                    ExceptionHandler.SendChatMessage("Save operation already in progress.");

                SaveInProgress = false;
                return cfg;
            }

            public void LoadStart(Action<ConfigT> UpdateConfig, bool silent = false)
            {
                if (!SaveInProgress)
                {
                    SaveInProgress = true;
                    if (!silent) ExceptionHandler.SendChatMessage("Loading configuration...");

                    EnqueueTask(() =>
                    {
                        ConfigT cfg = null;
                        KnownException loadException = null;

                        // Load and validate
                        if (cfgFile.FileExists)
                        {
                            loadException = TryLoad(out cfg);
                            cfg = ValidateConfig(cfg);
                        }
                        else
                            cfg = Defaults;

                        // Enqueue callback
                        EnqueueAction(() =>
                            UpdateConfig(cfg));

                        // Write validated config back to the file
                        TrySave(cfg);

                        if (loadException != null)
                        {
                            EnqueueAction(() =>
                                LoadFinish(false, silent));

                            throw loadException;
                        }
                        else
                            EnqueueAction(() =>
                                LoadFinish(true, silent));
                    });
                }
                else
                    ExceptionHandler.SendChatMessage("Save operation already in progress.");
            }
            
            private ConfigT ValidateConfig(ConfigT cfg)
            {
                if (cfg != null)
                {
                    if (cfg.VersionID != Defaults.VersionID)
                    {
                        EnqueueAction(() => ExceptionHandler.SendChatMessage("Config version mismatch. Some settings may have " +
                                                                             "been reset. A backup of the original config file will be made."));

                        Backup();
                    }

                    cfg.Validate();
                    return cfg;
                }
                else
                {
                    EnqueueAction(() => ExceptionHandler.SendChatMessage("Unable to load configuration."));
                    return Defaults;
                }
            }

            private void LoadFinish(bool success, bool silent = false)
            {
                if (SaveInProgress)
                {
                    if (!silent)
                    {
                        if (success)
                            ExceptionHandler.SendChatMessage("Configuration loaded.");
                    }

                    SaveInProgress = false;
                }
            }
            
            public void SaveStart(ConfigT cfg, bool silent = false)
            {
                if (!SaveInProgress)
                {
                    if (!silent) ExceptionHandler.SendChatMessage("Saving configuration...");
                    SaveInProgress = true;

                    EnqueueTask(() =>
                    {
                        cfg.Validate();
                        KnownException exception = TrySave(cfg);

                        if (exception != null)
                        {
                            EnqueueAction(() =>
                                SaveFinish(false, silent));

                            throw exception;
                        }
                        else
                            EnqueueAction(() =>
                                SaveFinish(true, silent));
                    });
                }
                else
                    ExceptionHandler.SendChatMessage("Save operation already in progress.");
            }

            private void SaveFinish(bool success, bool silent = false)
            {
                if (SaveInProgress)
                {
                    if (!silent)
                    {
                        if (success)
                            ExceptionHandler.SendChatMessage("Configuration saved.");
                        else
                            ExceptionHandler.SendChatMessage("Unable to save configuration.");
                    }

                    SaveInProgress = false;
                }
            } 
            
            public void Save(ConfigT cfg)
            {
                if (!SaveInProgress)
                {
                    cfg.Validate();
                    KnownException exception = TrySave(cfg);

                    if (exception != null)
                        throw exception;
                }
            }
            
            private void Backup()
            {
                if (cfgFile.FileExists)
                {
                    KnownException exception = cfgFile.TryDuplicate($"old_" + cfgFile.file);

                    if (exception != null)
                        throw exception;
                }
            }
            
            private KnownException TryLoad(out ConfigT cfg)
            {
                string data;
                KnownException exception = cfgFile.TryRead(out data);
                cfg = null;

                if (exception != null || data == null)
                    return exception;
                else
                    exception = Utils.Xml.TryDeserialize(data, out cfg);

                return exception;
            }
            
            private KnownException TrySave(ConfigT cfg)
            {
                string xmlOut;
                KnownException exception = Utils.Xml.TrySerialize(cfg, out xmlOut);

                if (exception == null && xmlOut != null)
                    exception = cfgFile.TryWrite(xmlOut);

                return exception;
            }
        }
    }
}