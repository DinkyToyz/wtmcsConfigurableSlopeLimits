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
            new Generic("Highway Ramp", "ramp", "Roads", 5),
            new Generic("Highway", "high", "Roads", 6),
            new Generic("Large Road", "large", "Roads", 4),
            new Generic("Medium Road", "medium", "Roads", 3),
            new Generic("Small Road", "small", "Roads", 2),
            new Generic("Gravel Road", "gravel", "Roads", 1),
            new Generic("Train Track", "track", "Railroads", 10),
            new Generic("Metro Track", "track", "Railroads", 9),
            new Generic("Pedestrian Path", "pedestrian", "Paths", 7),
            new Generic("Bicycle Path", "bicycle", "Paths", 8),
            new Generic("Airplane Runway", "runway", "Runways", 11)
        };

        /// <summary>
        /// The pattern for the nets that should be ignored.
        /// </summary>
        public static readonly string IgnoreNetsPattern = "(?: (?:Pipe|Transport|Connection|Line|Dock|Wire|Dam)|(?<!Pedestrian|Bicycle) Path|Bus Stop)$";

        /// <summary>
        /// The nets that should be ignored.
        /// </summary>
        public static readonly Regex IgnoreNetsRex = new Regex(IgnoreNetsPattern, RegexOptions.IgnoreCase);

        /// <summary>
        /// The net groups.
        /// </summary>
        public static Dictionary<string, int> NetGroups = new Dictionary<string, int>
        {
            { "Roads", 1 },
            { "Paths", 2 },
            { "Railroads", 3 },
            { "Runways", 4 },
            { "Others", 5 }
        };

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
        /// Initializes static members of the <see cref="Settings"/> class.
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
            this.SlopeLimitsGeneric = new Dictionary<string, float>();
            this.SlopeLimitsOriginal = new Dictionary<string, float>();
            this.SlopeLimitsIgnored = new Dictionary<string, float>();

            Dictionary<string, float> slopeLimits = null;

            if (settings != null)
            {
                this.loadedVersion = settings.Version;

                try
                {
                    slopeLimits = settings.GetSlopeLimits();
                }
                catch (Exception ex)
                {
                    Log.Error(typeof(Settings), "Constructor", ex, "settings.GetSlopeLimits()");
                }

                try
                {
                    Dictionary<string, float> orgLimits = settings.GetOriginalSlopeLimits();
                    if (orgLimits != null)
                    {
                        Log.Debug(this, "Constructor", "Load original limits");
                        this.SlopeLimitsOriginal = orgLimits;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(typeof(Settings), "Constructor", ex, "settings.GetOriginalSlopeLimits()");
                }
            }

            if (slopeLimits == null)
            {
                this.SlopeLimits = new Dictionary<string, float>();
            }
            else
            {
                this.SlopeLimits = slopeLimits;
                this.InitGenerics();
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
                return (this.loadedVersion == null || !this.loadedVersion.HasValue) ? 0 : this.loadedVersion.Value;
            }
        }

        /// <summary>
        /// Gets the slope limits.
        /// </summary>
        /// <value>
        /// The slope limits.
        /// </value>
        public Dictionary<string, float> SlopeLimits { get; private set; }

        /// <summary>
        /// Gets the generic slope limits.
        /// </summary>
        /// <value>
        /// The generic slope limits.
        /// </value>
        public Dictionary<string, float> SlopeLimitsGeneric { get; private set; }

        /// <summary>
        /// Gets the ignored slope limits.
        /// </summary>
        /// <value>
        /// The ignored slope limits.
        /// </value>
        public Dictionary<string, float> SlopeLimitsIgnored { get; private set; }

        /// <summary>
        /// Gets the original slope limits.
        /// </summary>
        /// <value>
        /// The original slope limits.
        /// </value>
        public Dictionary<string, float> SlopeLimitsOriginal { get; private set; }

        /// <summary>
        /// Check if net should be ignored.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>True if net should be ignored.</returns>
        public static bool IgnoreNet(string name)
        {
            return IgnoreNetsRex.Match(name).Success;
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
        /// Gets the matching generic.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A generic.</returns>
        public Generic GetGeneric(string name)
        {
            string lowerName = name.ToLowerInvariant();

            foreach (Generic generic in Generics)
            {
                if (generic.LowerCaseName == lowerName)
                {
                    return generic;
                }
            }

            foreach (Generic generic in Generics)
            {
                if (lowerName.Contains(generic.LowerCaseName))
                {
                    return generic;
                }
            }

            foreach (Generic generic in Generics)
            {
                if (lowerName.Contains(generic.Part))
                {
                    return generic;
                }
            }

            Generic result = new Generic(-1);

            foreach (KeyValuePair<string, int> group in NetGroups)
            {
                if (group.Value > result.Order)
                {
                    result.Group = group.Key;
                    result.Order = group.Value;
                }
            }

            result.Order += 1000;
            return result;
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
                if (this.SlopeLimitsOriginal == null)
                {
                    this.SlopeLimitsOriginal = new Dictionary<string, float>();
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

                this.SaveCount++;
                using (FileStream file = File.Create(fileName))
                {
                    ConfigurableSlopeLimitsSettings cfg = new ConfigurableSlopeLimitsSettings();
                    cfg.Version = this.Version;
                    cfg.SaveCount = this.SaveCount;
                    cfg.IgnoreNetInfoPattern = IgnoreNetsPattern;
                    cfg.GenericNetInfoNames = Generics;
                    cfg.SetSlopeLimits(this.SlopeLimits);
                    cfg.SetGenericSlopeLimits(this.SlopeLimitsGeneric);
                    cfg.SetOriginalSlopeLimits(this.SlopeLimitsOriginal);
                    cfg.SetIgnoredtSlopeLimits(this.SlopeLimitsIgnored);

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
        /// Sets the limit.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        public void SetLimit(string name, float limit)
        {
            Log.Debug(this, "SetLimit", name, limit);

            string lowerName = name.ToLowerInvariant();

            bool changed = false;

            if (this.SlopeLimits[name] != limit)
            {
                this.SlopeLimits[name] = limit;
                changed = true;
            }

            if (this.SlopeLimitsGeneric.ContainsKey(lowerName) && this.SlopeLimitsGeneric[lowerName] != limit)
            {
                this.SlopeLimitsGeneric[lowerName] = limit;
                changed = true;
            }

            if (changed)
            {
                this.Save();
            }
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public void Update()
        {
            this.InitGenerics();
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
                if (!this.SlopeLimitsGeneric.ContainsKey(generic))
                {
                    if (this.SlopeLimits.ContainsKey(name))
                    {
                        this.SlopeLimitsGeneric[generic] = this.SlopeLimits[name];
                    }
                    else
                    {
                        string lowerName = name.ToLowerInvariant();
                        if (this.SlopeLimitsGeneric.ContainsKey(lowerName))
                        {
                            this.SlopeLimitsGeneric[generic] = this.SlopeLimitsGeneric[lowerName];
                        }
                        else
                        {
                            foreach (string limitName in this.SlopeLimits.Keys)
                            {
                                if (limitName.ToLowerInvariant().Contains(generic))
                                {
                                    this.SlopeLimitsGeneric[generic] = this.SlopeLimits[limitName];
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
                foreach (string name in this.SlopeLimits.Keys)
                {
                    string lowerName = name.ToLowerInvariant();
                    if (!this.SlopeLimitsGeneric.ContainsKey(lowerName))
                    {
                        this.SlopeLimitsGeneric[lowerName] = this.SlopeLimits[name];
                    }
                }

                foreach (Generic gen in Generics)
                {
                    this.InitGeneric(gen.Name, gen.Part);
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
            /// The group.
            /// </summary>
            public string Group;

            /// <summary>
            /// The lower case name.
            /// </summary>
            public string LowerCaseName;

            /// <summary>
            /// The name.
            /// </summary>
            public string Name;

            /// <summary>
            /// The order.
            /// </summary>
            public int Order;

            /// <summary>
            /// The part.
            /// </summary>
            public string Part;

            /// <summary>
            /// Initializes a new instance of the <see cref="Generic" /> struct.
            /// </summary>
            /// <param name="order">The order.</param>
            public Generic(int order)
            {
                this.Name = null;
                this.Part = null;
                this.Group = null;
                this.Order = order;
                this.LowerCaseName = null;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Generic"/> struct.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="part">The part.</param>
            /// <param name="group">The group.</param>
            /// <param name="order">The sort order.</param>
            public Generic(string name, string part, string group, int order)
            {
                this.Name = name;
                this.Part = part;
                this.Group = group;
                this.Order = order;
                this.LowerCaseName = name.ToLowerInvariant();
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
            /// <returns>
            /// The slope limits dictionary.
            /// </returns>
            public Dictionary<string, float> GetOriginalSlopeLimits()
            {
                return this.GetLimitsDictionary(this.OriginalSlopeLimits, "GetOriginalSlopeLimits");
            }

            /// <summary>
            /// Gets the slope limits dictionary.
            /// </summary>
            /// <returns>
            /// The slope limits dictionary.
            /// </returns>
            public Dictionary<string, float> GetSlopeLimits()
            {
                return this.GetLimitsDictionary(this.SlopeLimits, "GetSlopeLimits");
            }

            /// <summary>
            /// Sets the generic slope limits.
            /// </summary>
            /// <param name="limitsDictionary">The limits dictionary.</param>
            public void SetGenericSlopeLimits(Dictionary<string, float> limitsDictionary)
            {
                this.SetLimitsDictionary(this.GenericSlopeLimits, limitsDictionary, "SetSlopeLimits");
            }

            /// <summary>
            /// Sets the ignored slope limits.
            /// </summary>
            /// <param name="limitsDictionary">The limits dictionary.</param>
            public void SetIgnoredtSlopeLimits(Dictionary<string, float> limitsDictionary)
            {
                this.SetLimitsDictionary(this.IgnoredSlopeLimits, limitsDictionary, "SetSlopeLimits");
            }

            /// <summary>
            /// Sets the original slope limits.
            /// </summary>
            /// <param name="limitsDictionary">The limits dictionary.</param>
            public void SetOriginalSlopeLimits(Dictionary<string, float> limitsDictionary)
            {
                this.SetLimitsDictionary(this.OriginalSlopeLimits, limitsDictionary, "SetSlopeLimits");
            }

            /// <summary>
            /// Sets the slope limits.
            /// </summary>
            /// <param name="limitsDictionary">The limits dictionary.</param>
            public void SetSlopeLimits(Dictionary<string, float> limitsDictionary)
            {
                this.SetLimitsDictionary(this.SlopeLimits, limitsDictionary, "SetSlopeLimits");
            }

            /// <summary>
            /// Gets the limits dictionary.
            /// </summary>
            /// <param name="limits">The limits.</param>
            /// <param name="name">The name.</param>
            /// <returns>The limits dictionary.</returns>
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
                    this.Name = name;
                    this.Limit = limit;
                }
            }
        }
    }
}
