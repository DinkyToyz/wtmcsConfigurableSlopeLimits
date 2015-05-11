using ColossalFramework.IO;
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
        /// Serializable settings class.
        /// </summary>
        [Serializable]
        public class ConfigurableSlopeLimitsSettings
        {
            /// <summary>
            /// Slope limit pair.
            /// </summary>
            [Serializable]
            public class SlopeLimit
            {
                /// <summary>
                /// The name.
                /// </summary>
                public string Name;

                /// <summary>
                /// The limit.
                /// </summary>
                public float Limit;

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

            /// <summary>
            /// The save count.
            /// </summary>
            public uint SaveCount = 0;

            /// <summary>
            /// The slope limits.
            /// </summary>
            public List<SlopeLimit> SlopeLimits = new List<SlopeLimit>();

            /// <summary>
            /// Gets the slope limits dictionary.
            /// </summary>
            /// <value>
            /// The slope limits dictionary.
            /// </value>
            public Dictionary<string, float> SlopeLimitsDictionary()
            {
                try
                {
                    return SlopeLimits.ToDictionary(l => l.Name, l => l.Limit);
                }
                catch (Exception ex)
                {
                    Log.Error(this, "SlopeLimitsDictionary", ex);
                    return new Dictionary<string, float>();
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ConfigurableSlopeLimitsSettings"/> class.
            /// </summary>
            public ConfigurableSlopeLimitsSettings()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ConfigurableSlopeLimitsSettings"/> class.
            /// </summary>
            /// <param name="saveCount">The save count.</param>
            /// <param name="limits">The limits.</param>
            public ConfigurableSlopeLimitsSettings(uint saveCount, Dictionary<string, float> limits)
            {
                SaveCount = saveCount;
                SlopeLimits.AddRange(limits.ToList().ConvertAll(kvp => new SlopeLimit(kvp.Key, kvp.Value)));
            }
        }

        /// <summary>
        /// The save count.
        /// </summary>
        public uint SaveCount = 0;

        /// <summary>
        /// The slope limits.
        /// </summary>
        public Dictionary<string, float> SlopeLimits { get; private set; }

        /// <summary>
        /// The generic slope limits.
        /// </summary>
        public Dictionary<string, float> SlopeLimitsGeneric { get; private set; }

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
        /// The nets that should be ignored.
        /// </summary>
        private static Regex ignoreNets = new Regex("(?: (?:Pipe|Transport|Connection|Line|Dock|Wire|Dam)|(?<!Pedestrian) Path)$", RegexOptions.IgnoreCase);

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
        /// The generic names.
        /// </summary>
        public static readonly HashSet<String> GenericNames = new HashSet<string>(Generics.ConvertAll<string>(g => g.Name));

        /// <summary>
        /// The current settings.
        /// </summary>
        private static Settings currentSettings;

        /// <summary>
        /// Gets the current settings.
        /// </summary>
        /// <value>
        /// The current settings.
        /// </value>
        public static Settings Current
        {
            get
            {
                if (currentSettings == null)
                {
                    currentSettings = Load();
                    foreach (string name in currentSettings.SlopeLimits.Keys)
                    {
                        Log.Info("CfgLimit: " + name + "=" + currentSettings.SlopeLimits[name].ToString());
                    }
                    foreach (string name in currentSettings.SlopeLimits.Keys)
                    {
                        Log.Info("GenLimit: " + name + "=" + currentSettings.SlopeLimitsGeneric[name].ToString());
                    }
                }

                return currentSettings;
            }
        }

        /// <summary>
        /// Check if net should be ignored.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>True if net should be ignored.</returns>
        public static bool IgnoreNet(string name)
        {
            return ignoreNets.Match(name).Success;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
            SlopeLimits = new Dictionary<string, float>();
            SlopeLimitsGeneric = new Dictionary<string, float>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        /// <param name="slopeLimits">The slope limits.</param>
        public Settings(Dictionary<string, float> slopeLimits)
        {
            SlopeLimits = slopeLimits;
            SlopeLimitsGeneric = new Dictionary<string, float>();
            InitGenerics();
        }

        /// <summary>
        /// Gets the file path.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        public static string FilePath
        {
            get
            {
                return Path.Combine(DataLocation.localApplicationData, "ModConfig");
            }
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public static string FileName
        {
            get
            {
                return Assembly.Name + ".xml";
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
                return Path.GetFullPath(Path.Combine(FilePath, FileName));
            }
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
        /// Updates this instance.
        /// </summary>
        public void Update()
        {
            InitGenerics();
        }

        /// <summary>
        /// Loads settings from the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The settings.</returns>
        public static Settings Load(string fileName = null)
        {
            try
            {
                Log.Info(typeof(Settings), "Load", "Begin");

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
                            Settings sets = new Settings(cfg.SlopeLimitsDictionary());

                            Log.Info(typeof(Settings), "Load", "End");
                            return sets;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Settings), "Load", ex);
            }

            return new Settings();
        }

        /// <summary>
        /// Saves settings to the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public void Save(string fileName = null)
        {
            try
            {
                Log.Info(this, "Save", "Begin");

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
                    ConfigurableSlopeLimitsSettings cfg = new ConfigurableSlopeLimitsSettings(SaveCount, SlopeLimits);

                    XmlSerializer ser = new XmlSerializer(typeof(ConfigurableSlopeLimitsSettings));
                    ser.Serialize(file, cfg);
                    file.Flush();
                    file.Close();
                }

                Log.Info(this, "Save", "End");
            }
            catch (Exception ex)
            {
                Log.Error(this, "Save", ex);
            }
        }
    }
}