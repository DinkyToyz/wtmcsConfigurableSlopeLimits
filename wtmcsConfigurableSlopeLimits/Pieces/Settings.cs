using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Mod settings.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// The settings version.
        /// </summary>
        public readonly int Version = 2;

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

                    foreach (string limit in slopeLimits.Keys.Where(l => Global.NetNames.IgnoreNet(l)).ToList())
                    {
                        slopeLimits.Remove(limit);
                    }
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
        /// Check if expansion (DLC) is owned.
        /// </summary>
        /// <param name="dlc">The DLC.</param>
        /// <returns>
        /// True if expansion is owned.
        /// </returns>
        public static bool IsDLCOwned(SteamHelper.DLC? dlc)
        {
            if (dlc == null || !dlc.HasValue)
            {
                return false;
            }
            else
            {
                return SteamHelper.IsDLCOwned(dlc.Value);
            }
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

            string fallBack = NetNameMap.GetFallbackName(name);
            if (fallBack != null)
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
        /// Gets the limit.
        /// </summary>
        /// <param name="netName">The net name.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="cfgLimit">The configured limit.</param>
        /// <param name="match">The match type.</param>
        /// <returns>True if limit found.</returns>
        public bool GetLimit(string netName, out float limit, out float? cfgLimit, out string match)
        {
            if (this.SlopeLimits.TryGetValue(netName, out limit) && !float.IsNaN(limit) && !float.IsInfinity(limit))
            {
                cfgLimit = limit;
                match = "name";

                return true;
            }

            string name = netName.ToLowerInvariant();
            if (this.SlopeLimitsGeneric.TryGetValue(name, out limit) && !float.IsNaN(limit) && !float.IsInfinity(limit))
            {
                cfgLimit = limit;
                match = "generic";

                return true;
            }

            foreach (string generic in this.SlopeLimitsGeneric.Keys.ToList().OrderBy(sName => name.Length).Reverse())
            {
                limit = this.SlopeLimitsGeneric[generic];
                if (!float.IsNaN(limit) && !float.IsInfinity(limit))
                {
                    if (name.Contains(generic))
                    {
                        cfgLimit = limit;
                        match = "part";

                        return true;
                    }
                }
            }

            limit = float.NaN;
            cfgLimit = null;
            match = null;

            return false;
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
                    cfg.SetSlopeLimits(this.SlopeLimits);
                    cfg.SetGenericSlopeLimits(this.SlopeLimitsGeneric);
                    cfg.SetOriginalSlopeLimits(this.SlopeLimitsOriginal);
                    cfg.SetIgnoredtSlopeLimits(this.SlopeLimitsIgnored);
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
                float oldLimit = float.NaN;

                bool exists = this.SlopeLimits.TryGetValue(name, out oldLimit);
                bool supported = exists || NetNameMap.IsSupportedGeneric(name);

                if (supported && (!exists || oldLimit != limit))
                {
                    this.SlopeLimits[name] = limit;
                    changed = true;
                }

                if (this.SlopeLimitsGeneric.TryGetValue(lowerName, out oldLimit) && oldLimit != limit)
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
                Log.Error(this, "SetLimit", ex, name, limit);
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
                if (!this.SlopeLimitsGeneric.ContainsKey(generic) && !Global.NetNames.IgnoreNet(name))
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

                foreach (NetNameMap.Generic gen in NetNameMap.Generics)
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
                return this.GetLimitsDictionary(this.SlopeLimits, "GetSlopeLimits", true);
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
            private Dictionary<string, float> GetLimitsDictionary(List<SlopeLimit> limits, string name = null, bool versionCheck = false)
            {
                try
                {
                    Dictionary<string, float> dict = limits.ToDictionary(l => l.Name, l => l.Limit);

                    List<KeyValuePair<string, float>> lims = dict.ToList();
                    foreach (KeyValuePair<string, float> lim in lims)
                    {
                        Log.Debug(this, "GetLimitsDictionary", "Check", lim.Key, lim.Value, versionCheck);

                        if (float.IsNaN(lim.Value) || float.IsInfinity(lim.Value) ||
                            (versionCheck && this.Version < 2 && lim.Key == "Bicycle"))
                        {
                            Log.Info(this, "GetLimitsDictionary", "Remove", lim.Key, lim.Value);
                            dict.Remove(lim.Key);
                        }
                    }

                    return dict;
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