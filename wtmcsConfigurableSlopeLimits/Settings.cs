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
        /// The net groups.
        /// </summary>
        public static Dictionary<string, int> NetGroups = new Dictionary<string, int>
        {
            { "Roads", 1 },
            { "Paths", 2 },
            { "Railroads", 3 },
            { "Runways", 4 },
            { "Other", 5 }
        };

        /// <summary>
        /// The settings version.
        /// </summary>
        public readonly int Version = 1;

        /// <summary>
        /// The maximum slope limit.
        /// </summary>
        public float MaximumLimit = 1.00f;

        /// <summary>
        /// The minimum slope limit.
        /// </summary>
        public float MinimumLimit = 0.01f;

        /// <summary>
        /// The save count.
        /// </summary>
        public uint SaveCount = 0;

        /// <summary>
        /// The display names.
        /// </summary>
        private static readonly Dictionary<string, string> DisplayNames = new Dictionary<string, string>
        {
            { "Rural Highway", "National Road" }
        };

        /// <summary>
        /// The display orders.
        /// </summary>
        private static readonly Dictionary<string, int> DisplayOrders = new Dictionary<string, int>
        {
            { "Tiny Road", 150 },
            { "Small Heavy Road", 250 },
            { "Rural Highway", 450 },
            { "National Road", 450 }
        };

        /// <summary>
        /// The fall back limits.
        /// </summary>
        private static readonly Dictionary<string, string> FallBackNames = new Dictionary<string, string>
        {
            { "Tiny Road", "Small Road" },
            { "Small Heavy Road", "Small Road" },
            { "Rural Highway", "Highway" },
            { "National Road", "Highway" }
        };

        /// <summary>
        /// The generics.
        /// </summary>
        private static readonly List<Generic> Generics = new List<Generic>
        {
            new Generic("Highway Ramp", "ramp", "Roads", 500),
            new Generic("Highway", "high", "Roads", 600),
            new Generic("Large Road", "large", "Roads", 400),
            new Generic("Medium Road", "medium", "Roads", 300),
            new Generic("Small Road", "small", "Roads", 200),
            new Generic("Gravel Road", "gravel", "Roads", 100),
            new Generic("Train Track", "track", "Railroads", 1000),
            new Generic("Metro Track", "track", "Railroads", 900),
            new Generic("Pedestrian Path", "pedestrian", "Paths", 700),
            new Generic("Bicycle Path", "bicycle", "Paths", 800),
            new Generic("Airplane Runway", "runway", "Runways", 1100)
        };

        /// <summary>
        /// The pattern for the net collections and nets combinations for wich to warn about ignored nets.
        /// </summary>
        private static readonly string WarnIgnoreNetCollectionsNetsPattern = "^(?:(?:[^;]*?Road|Beautification);.*|Expansion \\d+;.*(?:Road|Path|Tunnel|Track)|Public Transport;(?:Road|Tunnel|Track))$";

        /// <summary>
        /// The net collections and nets combinations for wich to warn about ignored nets.
        /// </summary>
        private static readonly Regex WarnIgnoreNetCollectionsNetsRex = new Regex(WarnIgnoreNetCollectionsNetsPattern, RegexOptions.IgnoreCase);

        /// <summary>
        /// The pattern for the net collections that should be ignored.
        /// </summary>
        private static readonly string IgnoreNetCollectionsPattern = "^(?:Electricity|Water)$";

        /// <summary>
        /// The net collections that should be ignored.
        /// </summary>
        private static readonly Regex IgnoreNetCollectionsRex = new Regex(IgnoreNetCollectionsPattern, RegexOptions.IgnoreCase);

        /// <summary>
        /// The pattern for the nets that should be ignored.
        /// </summary>
        private static readonly string IgnoreNetsPattern = "(?:(?:^NExt)|(?:^Bus Stop$)|(?:(?: (?:Pipe|Transport|Connection|Line|Dock|Wire|Dam))|(?:(?<!Pedestrian|Bicycle) Path)$))";

        /// <summary>
        /// The nets that should be ignored.
        /// </summary>
        private static readonly Regex IgnoreNetsRex = new Regex(IgnoreNetsPattern, RegexOptions.IgnoreCase);

        /// <summary>
        /// The horizontal button position.
        /// </summary>
        private short buttonPositionX = 0;

        /// <summary>
        /// The vertical button position.
        /// </summary>
        private short buttonPositionY = 1;

        /// <summary>
        /// The instance is initializing.
        /// </summary>
        private bool initializing = false;

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
            this.initializing = true;

            this.SlopeLimitsGeneric = new Dictionary<string, float>();
            this.SlopeLimitsOriginal = new Dictionary<string, float>();
            this.SlopeLimitsIgnored = new Dictionary<string, float>();

            Dictionary<string, float> slopeLimits = null;

            if (settings != null)
            {
                this.loadedVersion = settings.Version;

                this.buttonPositionX = settings.ButtonPositionHorizontal;
                this.buttonPositionY = settings.ButtonPositionVertical;

                this.MinimumLimit = settings.MinimumLimit;
                this.MaximumLimit = settings.MaximumLimit;

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

            this.initializing = false;
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
        /// Gets or sets the horizontal button position.
        /// </summary>
        /// <value>
        /// The horizontal button position.
        /// </value>
        public short ButtonPositionHorizontal
        {
            get
            {
                return this.buttonPositionX;
            }

            set
            {
                this.buttonPositionX = value;
                this.Save();
            }
        }

        /// <summary>
        /// Gets or sets the vertical button position.
        /// </summary>
        /// <value>
        /// The vertical button position.
        /// </value>
        public short ButtonPositionVertical
        {
            get
            {
                return this.buttonPositionY;
            }

            set
            {
                this.buttonPositionY = value;
                this.Save();
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
        public Dictionary<string, float> SlopeLimits
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the generic slope limits.
        /// </summary>
        /// <value>
        /// The generic slope limits.
        /// </value>
        public Dictionary<string, float> SlopeLimitsGeneric
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the ignored slope limits.
        /// </summary>
        /// <value>
        /// The ignored slope limits.
        /// </value>
        public Dictionary<string, float> SlopeLimitsIgnored
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the original slope limits.
        /// </summary>
        /// <value>
        /// The original slope limits.
        /// </value>
        public Dictionary<string, float> SlopeLimitsOriginal
        {
            get;
            private set;
        }

        /// <summary>
        /// Check if net should be ignored.
        /// </summary>
        /// <param name="name">The net name.</param>
        /// <returns>True if net should be ignored.</returns>
        public static bool IgnoreNet(string name)
        {
            return String.IsNullOrEmpty(name) ? true : IgnoreNetsRex.IsMatch(name);
        }

        /// <summary>
        /// Check if net collection should be ignored.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>True if net should be ignored.</returns>
        public static bool IgnoreNetCollection(string name)
        {
            return String.IsNullOrEmpty(name) ? true : IgnoreNetCollectionsRex.IsMatch(name);
        }

        /// <summary>
        /// Check if warning shoud be issued for ignored net.
        /// </summary>
        public static bool WarnAboutIgnoredNet(string collectionName, string netName)
        {
            return String.IsNullOrEmpty(collectionName) ? false : String.IsNullOrEmpty(netName) ? false : WarnIgnoreNetCollectionsNetsRex.IsMatch(collectionName + ";" + netName);
        }

        /// <summary>
        /// Get text showing whether net collection should be ignored.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The text "Ignore" if net collection should be ignored, otherwise null.</returns>
        public static string IgnoreNetCollectionText(string name)
        {
            return String.IsNullOrEmpty(name) ? (string)null : IgnoreNetCollectionsRex.IsMatch(name) ? "Ignore" : (string)null;
        }

        /// <summary>
        /// Get text showing whether net should be ignored.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The text "Ignore" if net should be ignored, otherwise null.</returns>
        public static string IgnoreNetText(string name)
        {
            return String.IsNullOrEmpty(name) ? (string)null : IgnoreNetsRex.IsMatch(name) ? "Ignore" : (string)null;
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
        /// Gets the net display name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The display name.</returns>
        public string DisplayName(string name)
        {
            if (name.Length > 6 && name.Substring(name.Length - 7, 7) == " Tunnel")
            {
                name = name.Substring(0, name.Length - 7);
                return (DisplayNames.ContainsKey(name) ? DisplayNames[name] : name) + " Tunnel";
            }
            else
            {
                return DisplayNames.ContainsKey(name) ? DisplayNames[name] : name;
            }
        }

        /// <summary>
        /// Gets the fall back limit.
        /// </summary>
        /// <param name="name">The mapped net name.</param>
        /// <returns>The fall back limit.</returns>
        public float GetFallBackLimit(string name)
        {
            float limit;
            if (this.SlopeLimits.TryGetValue(name, out limit))
            {
                return limit;
            }

            string tuff = "";

            if (name.SafeSubstring(name.Length - 7) == " Tunnel")
            {
                name = name.Substring(0, name.Length - 7);
                tuff = " Tunnel";

                if (this.SlopeLimits.TryGetValue(name, out limit))
                {
                    return limit;
                }
            }

            string fallBack;
            if (FallBackNames.TryGetValue(name, out fallBack))
            {
                if (this.SlopeLimits.TryGetValue(fallBack + tuff, out limit))
                {
                    return limit;
                }
                else if (this.SlopeLimits.TryGetValue(fallBack, out limit))
                {
                    return limit;
                }
            }

            return float.NaN;
        }

        /// <summary>
        /// Gets the matching generic.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A generic.</returns>
        public Generic GetGeneric(string name)
        {
            int order = this.GetOrder(name);

            if (name.SafeSubstring(name.Length - 7) == " Tunnel")
            {
                name = name.Substring(0, name.Length - 7);
            }
            string lowerName = name.ToLowerInvariant();

            foreach (Generic generic in Generics)
            {
                if (generic.LowerCaseName == lowerName)
                {
                    return (order >= 0) ? generic.Copy(order) : generic;
                }
            }

            string fallBackName = null;
            if (FallBackNames.TryGetValue(name, out fallBackName))
            {
                fallBackName = fallBackName.ToLowerInvariant();

                foreach (Generic generic in Generics)
                {
                    if (generic.LowerCaseName == fallBackName)
                    {
                        return (order >= 0) ? generic.Copy(order) : generic;
                    }
                }
            }

            foreach (Generic generic in Generics)
            {
                if (lowerName.Contains(generic.LowerCaseName))
                {
                    return generic.Copy(name, (order >= 0) ? order : generic.Order);
                }
            }

            if (!String.IsNullOrEmpty(fallBackName))
            {
                foreach (Generic generic in Generics)
                {
                    if (fallBackName.Contains(generic.LowerCaseName))
                    {
                        return generic.Copy(name, (order >= 0) ? order : generic.Order);
                    }
                }
            }

            foreach (Generic generic in Generics)
            {
                if (lowerName.Contains(generic.Part))
                {
                    return generic.Copy(name, (order >= 0) ? order : generic.Order);
                }
            }

            if (!String.IsNullOrEmpty(fallBackName))
            {
                foreach (Generic generic in Generics)
                {
                    if (fallBackName.Contains(generic.Part))
                    {
                        return generic.Copy(name, (order >= 0) ? order : generic.Order);
                    }
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

            result.Order = (order >= 0) ? order : result.Order + 10000;
            return result;
        }

        /// <summary>
        /// Saves settings to the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public void Save(string fileName = null)
        {
            if (this.initializing)
            {
                return;
            }

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
                    cfg.ButtonPositionHorizontal = this.buttonPositionX;
                    cfg.ButtonPositionVertical = this.buttonPositionY;
                    cfg.IgnoreNetInfoPattern = IgnoreNetsPattern;
                    cfg.IgnoreNetCollectionPattern = IgnoreNetCollectionsPattern;
                    cfg.GenericNetInfoNames = Generics;
                    cfg.SetSlopeLimits(this.SlopeLimits);
                    cfg.SetGenericSlopeLimits(this.SlopeLimitsGeneric);
                    cfg.SetOriginalSlopeLimits(this.SlopeLimitsOriginal);
                    cfg.SetIgnoredtSlopeLimits(this.SlopeLimitsIgnored);
                    cfg.SetDisplayNames(DisplayNames);
                    cfg.SetDisplayOrders(DisplayOrders);
                    cfg.MinimumLimit = this.MinimumLimit;
                    cfg.MaximumLimit = this.MaximumLimit;

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

            try
            {
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
            catch (Exception ex)
            {
                Log.Error(this, "SetLimit", ex);
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
        /// Gets the order.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The order.</returns>
        private int GetOrder(string name)
        {
            int order;

            if (DisplayOrders.TryGetValue(name, out order))
            {
                return order;
            }

            if (name.SafeSubstring(name.Length - 7) == " Tunnel" && DisplayOrders.TryGetValue(name.Substring(0, name.Length - 7), out order))
            {
                return order;
            }

            return -1;
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

            /// <summary>
            /// Copies this instance.
            /// </summary>
            /// <returns>A copy of this instance.</returns>
            public Generic Copy()
            {
                return this.Copy(this.Name, this.Order);
            }

            /// <summary>
            /// Copies this instance.
            /// </summary>
            /// <param name="order">The order.</param>
            /// <returns>
            /// A copy of this instance.
            /// </returns>
            public Generic Copy(int order)
            {
                return this.Copy(this.Name, order);
            }

            /// <summary>
            /// Copies this instance.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <returns>
            /// A copy of this instance.
            /// </returns>
            public Generic Copy(string name)
            {
                return this.Copy(name, this.Order);
            }

            /// <summary>
            /// Copies this instance.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="order">The order.</param>
            /// <returns>
            /// A copy of this instance.
            /// </returns>
            public Generic Copy(string name, int order)
            {
                return new Generic(name, this.Part, this.Group, order);
            }
        }

        /// <summary>
        /// Serializable settings class.
        /// </summary>
        [Serializable]
        public class ConfigurableSlopeLimitsSettings
        {
            /// <summary>
            /// The horizontal button position.
            /// </summary>
            public short ButtonPositionHorizontal = 0;

            /// <summary>
            /// The vertical button position.
            /// </summary>
            public short ButtonPositionVertical = 1;

            /// <summary>
            /// The net display names.
            /// </summary>
            public List<DisplayName> DisplayNames = new List<DisplayName>();

            /// <summary>
            /// The net display orders.
            /// </summary>
            public List<DisplayOrder> DisplayOrders = new List<DisplayOrder>();

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
            /// The net collections that are be ignored.
            /// </summary>
            public string IgnoreNetCollectionPattern = "";

            /// <summary>
            /// The net information that is be ignored.
            /// </summary>
            public string IgnoreNetInfoPattern = "";

            /// <summary>
            /// The maximum slope limit.
            /// </summary>
            public float MaximumLimit = 1.00f;

            /// <summary>
            /// The minimum slope limit.
            /// </summary>
            public float MinimumLimit = 0.00f;

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
            /// Sets the display names.
            /// </summary>
            /// <param name="displayNames">The display names.</param>
            /// <param name="name">The name.</param>
            public void SetDisplayNames(Dictionary<string, string> displayNames, string name = null)
            {
                try
                {
                    if (displayNames != null)
                    {
                        this.DisplayNames.Clear();
                        this.DisplayNames.AddRange(displayNames.ToList().ConvertAll(kvp => new DisplayName(kvp.Key, kvp.Value)));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(this, String.IsNullOrEmpty(name) ? "SetDisplayNames" : name, ex);
                }
            }

            /// <summary>
            /// Sets the display orders.
            /// </summary>
            /// <param name="displayOrders">The display orders.</param>
            /// <param name="name">The name.</param>
            public void SetDisplayOrders(Dictionary<string, int> displayOrders, string name = null)
            {
                try
                {
                    if (displayOrders != null)
                    {
                        this.DisplayOrders.Clear();
                        this.DisplayOrders.AddRange(displayOrders.ToList().ConvertAll(kvp => new DisplayOrder(kvp.Key, kvp.Value)));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(this, String.IsNullOrEmpty(name) ? "SetDisplayOrders" : name, ex);
                }
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
            /// Display name map entry.
            /// </summary>
            [Serializable]
            public class DisplayName
            {
                /// <summary>
                /// The display name.
                /// </summary>
                public string Display;

                /// <summary>
                /// The name.
                /// </summary>
                public string Name;

                /// <summary>
                /// Initializes a new instance of the <see cref="DisplayName"/> class.
                /// </summary>
                public DisplayName()
                {
                }

                /// <summary>
                /// Initializes a new instance of the <see cref="DisplayName"/> class.
                /// </summary>
                /// <param name="name">The name.</param>
                /// <param name="display">The display name.</param>
                public DisplayName(string name, string display)
                {
                    this.Name = name;
                    this.Display = display;
                }
            }

            /// <summary>
            /// Display order map entry.
            /// </summary>
            [Serializable]
            public class DisplayOrder
            {
                /// <summary>
                /// The name.
                /// </summary>
                public string Name;

                /// <summary>
                /// The display order.
                /// </summary>
                public int Order;

                /// <summary>
                /// Initializes a new instance of the <see cref="DisplayOrder"/> class.
                /// </summary>
                public DisplayOrder()
                {
                }

                /// <summary>
                /// Initializes a new instance of the <see cref="DisplayOrder" /> class.
                /// </summary>
                /// <param name="name">The name.</param>
                /// <param name="order">The order.</param>
                public DisplayOrder(string name, int order)
                {
                    this.Name = name;
                    this.Order = order;
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