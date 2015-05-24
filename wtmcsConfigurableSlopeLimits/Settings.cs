using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Mod settings.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// The generic names.
        /// </summary>
        public static readonly HashSet<String> GenericNames;

        /// <summary>
        /// The generics.
        /// </summary>
        public static readonly List<Generic> Generics = new List<Generic>
        {
            new Generic("Highway", "high"),
            new Generic("Highway Ramp", "ramp"),
            new Generic("Large Road", "large"),
            new Generic("Medium Road", "medium"),
            new Generic("Small Road", "small"),
            new Generic("Gravel Road", "gravel"),
            new Generic("Train Track", "track"),
            new Generic("Metro Track", "track"),
            new Generic("Pedestrian Path", "pedestrian"),
            new Generic("Pedestrian Path", "pedestrian")
        };

        /// <summary>
        /// The pattern for the nets that should be ignored.
        /// </summary>
        public static readonly string ignoreNetsPattern = "(?: (?:Pipe|Transport|Connection|Line|Dock|Wire|Dam)|(?<!Pedestrian) Path)$";

        /// <summary>
        /// The nets that should be ignored.
        /// </summary>
        public static readonly Regex ignoreNetsRex = new Regex(ignoreNetsPattern, RegexOptions.IgnoreCase);

        /// <summary>
        /// The settings version.
        /// </summary>
        public readonly int Version = 1;

        /// <summary>
        /// The save count.
        /// </summary>
        public uint SaveCount = 0;

        /// <summary>
        /// The settings version in the loaded file.
        /// </summary>
        private int? loadedVersion = null;

        /// <summary>
        /// Initializes the <see cref="Settings"/> class.
        /// </summary>
        static Settings()
        {
            GenericNames = new HashSet<string>(Generics.ConvertAll<string>(g => g.Name));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        /// <param name="settings">The file settings.</param>
        public Settings(ConfigurableSlopeLimitsSettings settings = null)
        {
            SlopeLimitsGeneric = new Dictionary<string, float>();
            SlopeLimitsOriginal = new Dictionary<string, float>();
            SlopeLimitsIgnored = new Dictionary<string, float>();

            Dictionary<string, float> slopeLimits = null;

            if (settings != null)
            {
                loadedVersion = settings.Version;

                try
                {
                    slopeLimits = settings.GetSlopeLimits();
                }
                catch (Exception ex)
                {
                    Log.Error(typeof(Settings), "Constructor", ex, "config.GetSlopeLimits()");
                }
            }

            if (slopeLimits == null)
            {
                SlopeLimits = new Dictionary<string, float>();
            }
            else
            {
                SlopeLimits = slopeLimits;
                InitGenerics();
            }
        }

        /// <summary>
        /// Gets the complete path.
        /// </summary>
        /// <value>
        /// The complete path.
        /// </value>
        public static string FilePathName
        {
            get
            {
                return FileSystem.FilePathName(".xml");
            }
        }

        /// <summary>
        /// Gets the settings version in the loaded file.
        /// </summary>
        /// <value>
        /// The settings version in the loaded file.
        /// </value>
        public int LoadedVersion
        {
            get
            {
                return (loadedVersion == null || !loadedVersion.HasValue) ? 0 : loadedVersion.Value;
            }
        }

        /// <summary>
        /// The slope limits.
        /// </summary>
        public Dictionary<string, float> SlopeLimits { get; private set; }

        /// <summary>
        /// The generic slope limits.
        /// </summary>
        public Dictionary<string, float> SlopeLimitsGeneric { get; private set; }

        /// <summary>
        /// The ignored slope limits.
        /// </summary>
        public Dictionary<string, float> SlopeLimitsIgnored { get; private set; }

        /// <summary>
        /// The original slope limits.
        /// </summary>
        public Dictionary<string, float> SlopeLimitsOriginal { get; private set; }

        /// <summary>
        /// Check if net should be ignored.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>True if net should be ignored.</returns>
        public static bool IgnoreNet(string name)
        {
            return ignoreNetsRex.Match(name).Success;
        }

        /// <summary>
        /// Loads settings from the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The settings.</returns>
        public static Settings Load(string fileName = null)
        {
            Log.Debug(typeof(Settings), "Load", "Begin");

            try
            {
                if (fileName == null)
                {
                    fileName = FilePathName;
                }

                if (File.Exists(fileName))
                {
                    Log.Info(typeof(Settings), "Load", fileName);

                    using (FileStream file = File.OpenRead(fileName))
                    {
                        XmlSerializer ser = new XmlSerializer(typeof(ConfigurableSlopeLimitsSettings));
                        ConfigurableSlopeLimitsSettings cfg = ser.Deserialize(file) as ConfigurableSlopeLimitsSettings;
                        if (cfg != null)
                        {
                            Log.Debug(typeof(Settings), "Load", "Loaded");

                            Settings sets = new Settings(cfg);

                            foreach (string name in sets.SlopeLimits.Keys)
                            {
                                Log.Info(null, null, "CfgLimit", name, sets.SlopeLimits[name]);
                            }
                            foreach (string name in sets.SlopeLimitsGeneric.Keys)
                            {
                                Log.Info(null, null, "GenLimit", name, sets.SlopeLimitsGeneric[name]);
                            }

                            Log.Debug(typeof(Settings), "Load", "End");
                            return sets;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Settings), "Load", ex);
            }

            Log.Debug(typeof(Settings), "Load", "End");
            return new Settings();
        }

        /// <summary>
        /// Saves settings to the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public void Save(string fileName = null)
        {
            Log.Debug(this, "Save", "Begin");

            try
            {
                if (SlopeLimitsOriginal == null)
                {
                    SlopeLimitsOriginal = new Dictionary<string, float>();
                }

                if (fileName == null)
                {
                    fileName = FilePathName;
                }

                string filePath = Path.GetDirectoryName(Path.GetFullPath(fileName));
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                Log.Info(this, "Save", fileName);

                SaveCount++;
                using (FileStream file = File.Create(fileName))
                {
                    ConfigurableSlopeLimitsSettings cfg = new ConfigurableSlopeLimitsSettings();
                    cfg.Version = this.Version;
                    cfg.SaveCount = SaveCount;
                    cfg.IgnoreNetInfoPattern = ignoreNetsPattern;
                    cfg.GenericNetInfoNames = Generics;
                    cfg.SetSlopeLimits(SlopeLimits);
                    cfg.SetGenericSlopeLimits(SlopeLimitsGeneric);
                    cfg.SetOriginalSlopeLimits(SlopeLimitsOriginal);
                    cfg.SetIgnoredtSlopeLimits(SlopeLimitsIgnored);

                    XmlSerializer ser = new XmlSerializer(typeof(ConfigurableSlopeLimitsSettings));
                    ser.Serialize(file, cfg);
                    file.Flush();
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "Save", ex);
            }

            Log.Debug(this, "Save", "End");
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public void Update()
        {
            InitGenerics();
        }

        /// <summary>
        /// Initializes a generic limit.
        /// </summary>
        /// <param name="name">The road type name.</param>
        /// <param name="generic">The generic name.</param>
        private void InitGeneric(string name, string generic)
        {
            try
            {
                if (!SlopeLimitsGeneric.ContainsKey(generic))
                {
                    if (SlopeLimits.ContainsKey(name))
                    {
                        SlopeLimitsGeneric[generic] = SlopeLimits[name];
                    }
                    else
                    {
                        string lName = name.ToLowerInvariant();
                        if (SlopeLimitsGeneric.ContainsKey(lName))
                        {
                            SlopeLimitsGeneric[generic] = SlopeLimitsGeneric[lName];
                        }
                        else
                        {
                            foreach (string sName in SlopeLimits.Keys)
                            {
                                if (sName.ToLowerInvariant().Contains(generic))
                                {
                                    SlopeLimitsGeneric[generic] = SlopeLimits[sName];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "InitGeneric", ex);
            }
        }

        /// <summary>
        /// Initializes the generic limits.
        /// </summary>
        private void InitGenerics()
        {
            try
            {
                foreach (string name in SlopeLimits.Keys)
                {
                    string lName = name.ToLowerInvariant();
                    if (!SlopeLimitsGeneric.ContainsKey(lName))
                    {
                        SlopeLimitsGeneric[lName] = SlopeLimits[name];
                    }
                }

                foreach (Generic gen in Generics)
                {
                    InitGeneric(gen.Name, gen.Part);
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "InitGenerics", ex);
            }
        }

        /// <summary>
        /// Generic net thingy name-part pair.
        /// </summary>
        public struct Generic
        {
            /// <summary>
            /// The name.
            /// </summary>
            public string Name;

            /// <summary>
            /// The part.
            /// </summary>
            public string Part;

            /// <summary>
            /// Initializes a new instance of the <see cref="Generic"/> struct.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="part">The part.</param>
            public Generic(string name, string part)
            {
                Name = name;
                Part = part;
            }
        }

        /// <summary>
        /// Serializable settings class.
        /// </summary>
        [Serializable]
        public class ConfigurableSlopeLimitsSettings
        {
            /// <summary>
            /// The generic net information names.
            /// </summary>
            public List<Generic> GenericNetInfoNames = new List<Generic>();

            /// <summary>
            /// The generic slope limits.
            /// </summary>
            public List<SlopeLimit> GenericSlopeLimits = new List<SlopeLimit>();

            /// <summary>
            /// The ignored net information names.
            /// </summary>
            public List<SlopeLimit> IgnoredSlopeLimits = new List<SlopeLimit>();

            /// <summary>
            /// The net information that is be ignored.
            /// </summary>
            public string IgnoreNetInfoPattern = "";

            /// <summary>
            /// The original slope limits.
            /// </summary>
            public List<SlopeLimit> OriginalSlopeLimits = new List<SlopeLimit>();

            /// <summary>
            /// The save count.
            /// </summary>
            public uint SaveCount = 0;

            /// <summary>
            /// The slope limits.
            /// </summary>
            public List<SlopeLimit> SlopeLimits = new List<SlopeLimit>();

            /// <summary>
            /// The settings version.
            /// </summary>
            public int Version = 0;

            /// <summary>
            /// Initializes a new instance of the <see cref="ConfigurableSlopeLimitsSettings"/> class.
            /// </summary>
            public ConfigurableSlopeLimitsSettings()
            {
            }

            /// <summary>
            /// Gets the slope limits dictionary.
            /// </summary>
            /// <value>
            /// The slope limits dictionary.
            /// </value>
            public Dictionary<string, float> GetSlopeLimits()
            {
                return GetLimitsDictionary(SlopeLimits, "SlopeLimitsDictionary");
            }

            /// <summary>
            /// Sets the generic slope limits.
            /// </summary>
            /// <param name="limitsDictionary">The limits dictionary.</param>
            public void SetGenericSlopeLimits(Dictionary<string, float> limitsDictionary)
            {
                SetLimitsDictionary(GenericSlopeLimits, limitsDictionary, "SetSlopeLimits");
            }

            /// <summary>
            /// Sets the ignored slope limits.
            /// </summary>
            /// <param name="limitsDictionary">The limits dictionary.</param>
            public void SetIgnoredtSlopeLimits(Dictionary<string, float> limitsDictionary)
            {
                SetLimitsDictionary(IgnoredSlopeLimits, limitsDictionary, "SetSlopeLimits");
            }

            /// <summary>
            /// Sets the original slope limits.
            /// </summary>
            /// <param name="limitsDictionary">The limits dictionary.</param>
            public void SetOriginalSlopeLimits(Dictionary<string, float> limitsDictionary)
            {
                SetLimitsDictionary(OriginalSlopeLimits, limitsDictionary, "SetSlopeLimits");
            }

            /// <summary>
            /// Sets the slope limits.
            /// </summary>
            /// <param name="limitsDictionary">The limits dictionary.</param>
            public void SetSlopeLimits(Dictionary<string, float> limitsDictionary)
            {
                SetLimitsDictionary(SlopeLimits, limitsDictionary, "SetSlopeLimits");
            }

            /// <summary>
            /// Gets the limits dictionary.
            /// </summary>
            /// <param name="limits">The limits.</param>
            /// <param name="name">The name.</param>
            /// <returns></returns>
            private Dictionary<string, float> GetLimitsDictionary(List<SlopeLimit> limits, string name = null)
            {
                try
                {
                    return limits.ToDictionary(l => l.Name, l => l.Limit);
                }
                catch (Exception ex)
                {
                    Log.Error(this, String.IsNullOrEmpty(name) ? "LimitsDictionary" : name, ex);
                    return new Dictionary<string, float>();
                }
            }

            /// <summary>
            /// Sets the limits dictionary.
            /// </summary>
            /// <param name="limits">The limits.</param>
            /// <param name="limitsDictionary">The limits dictionary.</param>
            /// <param name="name">The name.</param>
            private void SetLimitsDictionary(List<SlopeLimit> limits, Dictionary<string, float> limitsDictionary, string name = null)
            {
                try
                {
                    if (limitsDictionary != null)
                    {
                        limits.Clear();
                        limits.AddRange(limitsDictionary.ToList().ConvertAll(kvp => new SlopeLimit(kvp.Key, kvp.Value)));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(this, String.IsNullOrEmpty(name) ? "SetLimitsDictionary" : name, ex);
                }
            }

            /// <summary>
            /// Slope limit pair.
            /// </summary>
            [Serializable]
            public class SlopeLimit
            {
                /// <summary>
                /// The limit.
                /// </summary>
                public float Limit;

                /// <summary>
                /// The name.
                /// </summary>
                public string Name;

                /// <summary>
                /// Initializes a new instance of the <see cref="SlopeLimit"/> class.
                /// </summary>
                public SlopeLimit()
                {
                }

                /// <summary>
                /// Initializes a new instance of the <see cref="SlopeLimit"/> class.
                /// </summary>
                /// <param name="name">The name.</param>
                /// <param name="limit">The limit.</param>
                public SlopeLimit(string name, float limit)
                {
                    Name = name;
                    Limit = limit;
                }
            }
        }
    }
}