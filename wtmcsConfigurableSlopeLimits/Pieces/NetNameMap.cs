using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Network name map extension.
    /// </summary>
    public class NetNameMap
    {
        /// <summary>
        /// The fall back limits.
        /// </summary>
        protected static readonly Dictionary<string, string> FallBackNameList;

        /// <summary>
        /// The generics.
        /// </summary>
        protected static readonly List<Generic> GenericList;

        /// <summary>
        /// The group map.
        /// </summary>
        protected static readonly List<KeyValuePair<string, string>> GroupMap;

        /// <summary>
        /// The maximum limits.
        /// </summary>
        protected static readonly Dictionary<string, float> MaxLimits;

        /// <summary>
        /// The net groups.
        /// </summary>
        protected static readonly Dictionary<string, int> NetGroupList;

        /// <summary>
        /// The generic names.
        /// </summary>
        protected readonly HashSet<String> GenericNames;

        /// <summary>
        /// The display names.
        /// </summary>
        private static readonly Dictionary<string, string> DisplayNames;

        /// <summary>
        /// The display orders.
        /// </summary>
        private static readonly Dictionary<string, int> DisplayOrders;

        /// <summary>
        /// Matches canal names.
        /// </summary>
        private readonly Regex canalNameRex = new Regex("(?:^|Landscaping )?Canal(?: ?\\d+)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Matches castle wall names.
        /// </summary>
        private readonly Regex castleWallNameRex = new Regex("^Castle Walls?(?: ?\\d+)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Matches castle wall names.
        /// </summary>
        private readonly Regex floodWallNameRex = new Regex("(?:^|Landscaping )?Flood Wall(?: ?\\d+)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// The net collections that should be ignored.
        /// </summary>
        private readonly Regex ignoreNetCollectionsRex = new Regex("^(?:Electricity|Water)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// The nets that should be ignored.
        /// </summary>
        private readonly Regex ignoreNetsRex = new Regex("(?:(?:^NExt)|(?:^(?:Bus Stop|Radio)$)|(?:(?: (?:Pipe|Transport|Connection|Line|Dock|Wire|Dam|Cables))|(?:(?<!CableCar)(?: Cables))|(?:(?<!Pedestrian|Bicycle|Cable ?Car|Blimp) Path)$))", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        ////private readonly Regex ignoreNetsRex = new Regex("(?:(?:^NExt)|(?:^(?:Bus Stop|Radio)$)|(?:(?: (?:Pipe|Transport|Connection|Line|Dock|Wire|Dam|Cables))|(?:(?<!Pedestrian|Bicycle) Path)$))", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Matches large road class name.
        /// </summary>
        private readonly Regex largeRoadClassNameRex = new Regex("Large.*?(?:Road|Avenue)(?:TL)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Matches medium road class name.
        /// </summary>
        private readonly Regex mediumRoadClassNameRex = new Regex("Medium.*?(?:Road|Avenue)(?:TL)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Matches monorail track/road object name.
        /// </summary>
        private readonly Regex monorailTrackRoadObjectNameRex = new Regex("(?:(^| )Road(?: .*?)? Monorail( |$)|(?:^| )Monorail(?: Station|Oneway)? (?:Track|Road)( |$))", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Matches Network Extensions double tunnel class name left-over.
        /// </summary>
        private readonly Regex nextDoubleTunnelClasNameRestRex = new Regex("Tunnel(\\d+L)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Matches Network Extensions highway class name.
        /// </summary>
        private readonly Regex nextHighwayClassNameRex = new Regex("^NExt.*?Highway(?:\\d+L)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Matches Network Extensions large road class name.
        /// </summary>
        private readonly Regex nextLargeRoadClassNameRex = new Regex("^NExt.*?Large.*?(?:Road|Avenue)(?:TL)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Matches Network Extensions medium road class name.
        /// </summary>
        private readonly Regex nextMediumRoadClassNameRex = new Regex("^NExt.*?Medium.*?(?:Road|Avenue)(?:TL)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Matches Network Extensions small heavy road class name.
        /// </summary>
        private readonly Regex nextSmallHeavyRoadClassNameRex = new Regex("^NExt.*?(?:Small[3-9]L|basic)(?:Road|Avenue)[a-z]*?(?:TL)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Matches Network Extensions small road class name.
        /// </summary>
        private readonly Regex nextSmallRoadClassNameRex = new Regex("^NExt.*?Small.*?(?:Road|Avenue)(?:TL)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Matches Network Extensions tiny road class name.
        /// </summary>
        private readonly Regex nextTinyRoadClassNameRex = new Regex("^NExt.*?(?:[12]LAlley|1LOneway[a-z]*?|PedRoad[a-z]*?(?:\\d+m)?)(?:TL)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Matches quay names.
        /// </summary>
        private readonly Regex quayNameRex = new Regex("(?:^|Landscaping )?Quay(?: ?\\d+)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Matches small road class name.
        /// </summary>
        private readonly Regex smallRoadClassNameRex = new Regex("Small.*?(?:Road|Avenue)(?:TL)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Matches tram track/road object name.
        /// </summary>
        private readonly Regex tramTrackRoadObjectNameRex = new Regex("(?:(^| )Road(?: .*?)? Tram( |$)|(?:^| )Tram(?: Depot|Oneway)? (?:Track|Road)( |$))", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Matches trench names.
        /// </summary>
        private readonly Regex trenchNameRex = new Regex("^Trench Ruins?(?: ?\\d+)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// The net collections and nets combinations for which to warn about ignored nets.
        /// </summary>
        private readonly Regex warnIgnoreNetCollectionsNetsRex = new Regex("^(?:(?:[^;]*?Road|Beautification);.*|Expansion \\d+;.*(?:Road|Tunnel|Track|(?<!Ferry )Path)|Public Transport;(?:Road|Tunnel|Track))$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        ////private readonly Regex warnIgnoreNetCollectionsNetsRex = new Regex("^(?:(?:[^;]*?Road|Beautification);.*|Expansion \\d+;.*(?:Road|Tunnel|Track|(?<!(?:Ferry|CableCar|Blimp) )Path)|Public Transport;(?:Road|Tunnel|Track))$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// The map.
        /// </summary>
        private Dictionary<string, string> map = new Dictionary<string, string>();

        /// <summary>
        /// Initializes static members of the <see cref="NetNameMap"/> class.
        /// </summary>
        static NetNameMap()
        {
            try
            {
                // The fall back limits.
                FallBackNameList = new Dictionary<string, string>
                {
                    { "Tiny Road", "Small Road" },
                    { "Small Heavy Road", "Small Road" },
                    { "Rural Highway", "Highway" },
                    { "National Road", "Highway" }
                };

                // The display names.
                DisplayNames = new Dictionary<string, string>
                {
                    { "Rural Highway", "National Road" }
                };

                // The display orders.
                DisplayOrders = new Dictionary<string, int>
                {
                    { "Tiny Road", 150 },
                    { "Small Heavy Road", 250 },
                    { "Rural Highway", 450 },
                    { "National Road", 450 },
                    { "Pedestrian Bridge", 701 },
                    { "Pedestrian Tunnel", 702 },
                    { "Canal", 2000 },
                    { "Quay", 2010 },
                    { "Flood Wall", 2020 },
                    { "Dam", 2030 },
                    { "Pipe", 3000 },
                    { "Wire", 3000 },
                    { "Castle Wall", 4000 },
                    { "Trench", 4000 }
                };

                // The net groups.
                NetGroupList = new Dictionary<string, int>
                {
                    { "Tiny Roads", 1 },
                    { "Small Roads", 2 },
                    { "Medium & Large Roads", 3 },
                    { "Highways", 4 },
                    { "Paths", 5 },
                    { "Railroads", 6 },
                    { "Misc Transit", 7 },
                    { "Waterworks", 8 },
                    { "Other", 9 }
                };

                GroupMap = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Gravel Road", "Tiny Roads"),
                    new KeyValuePair<string, string>("Tiny Road", "Tiny Roads"),
                    new KeyValuePair<string, string>("Small Road", "Small Roads"),
                    new KeyValuePair<string, string>("Medium Road", "Medium & Large Roads"),
                    new KeyValuePair<string, string>("Large Road", "Medium & Large Roads"),
                    new KeyValuePair<string, string>("Highway", "Highways"),
                    new KeyValuePair<string, string>("Pedestrian", "Paths"),
                    new KeyValuePair<string, string>("Bicycle", "Paths"),
                    new KeyValuePair<string, string>("Train Track", "Railroads"),
                    new KeyValuePair<string, string>("Metro Track", "Railroads"),
                    new KeyValuePair<string, string>("Monorail Track", "Railroads"),
                    new KeyValuePair<string, string>("Tram Track", "Railroads"),
                    new KeyValuePair<string, string>("Airplane Runway", "Misc Transit"),
                    new KeyValuePair<string, string>("Blimp Path", "Misc Transit"),
                    new KeyValuePair<string, string>("Cable Car Path", "Misc Transit"),
                    new KeyValuePair<string, string>("Canal", "Waterworks"),
                    new KeyValuePair<string, string>("Quay", "Waterworks"),
                    new KeyValuePair<string, string>("Flood Wall", "Waterworks")
                };

                // The maximum limits.
                MaxLimits = new Dictionary<string, float>
                {
                    { "blimp", 3 }
                };

                // The generics.
                GenericList = new List<Generic>
                {
                    new Generic("Highway Ramp", "ramp", SteamHelper.DLC.None, 500),
                    new Generic("Highway Ramp Tunnel", "ramp", SteamHelper.DLC.None, 500, true),
                    new Generic("Highway", "high", SteamHelper.DLC.None, 600),
                    new Generic("Highway Tunnel", "high", SteamHelper.DLC.None, 600, true),
                    new Generic("Large Road", "large", SteamHelper.DLC.None, 400),
                    new Generic("Large Road Tunnel", "large", SteamHelper.DLC.None, 400, true),
                    new Generic("Medium Road", "medium", SteamHelper.DLC.None, 300),
                    new Generic("Medium Road Tunnel", "medium", SteamHelper.DLC.None, 300, true),
                    new Generic("Small Road", "small", SteamHelper.DLC.None, 200),
                    new Generic("Small Road Tunnel", "small", SteamHelper.DLC.None, 200, true),
                    new Generic("Gravel Road", "gravel", SteamHelper.DLC.None, 100),
                    new Generic("Gravel Road Tunnel", "gravel", SteamHelper.DLC.None, 100, true),
                    new Generic("Train Track", "track", SteamHelper.DLC.None, 1200),
                    new Generic("Train Track Tunnel", "track", SteamHelper.DLC.None, 1200, true),
                    new Generic("Metro Track", "track", SteamHelper.DLC.None, 1000),
                    new Generic("Monorail Track", "track", SteamHelper.DLC.InMotionDLC, 1300),
                    new Generic("Pedestrian Path", "pedestrian", SteamHelper.DLC.None, 700),
                    new Generic("Pedestrian Bridge", "pedestrian", SteamHelper.DLC.None, 700, true),
                    new Generic("Pedestrian Tunnel", "pedestrian", SteamHelper.DLC.None, 700, true),
                    new Generic("Bicycle Path", "bicycle", SteamHelper.DLC.AfterDarkDLC, 800),
                    new Generic("Bicycle Tunnel", "bicycle", SteamHelper.DLC.AfterDarkDLC, 800, true),
                    new Generic("Airplane Runway", "runway", SteamHelper.DLC.None, 1200),
                    new Generic("Tram Track", "tram", SteamHelper.DLC.SnowFallDLC, 900),
                    new Generic("Tram Track Tunnel", "tram", SteamHelper.DLC.SnowFallDLC, 900, true),
                    new Generic("Blimp Path", "blimp", SteamHelper.DLC.InMotionDLC, 900, true),
                    new Generic("Cable Car Path", "cable", SteamHelper.DLC.InMotionDLC, 900, true)
                };
            }
            catch (Exception ex)
            {
                Log.Error(typeof(NetNameMap), "ConstructStatic", ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetNameMap"/> class.
        /// </summary>
        public NetNameMap()
        {
            if (Log.LogToFile && Log.LogALot)
            {
                foreach (Generic generic in Generics)
                {
                    Log.Debug(this, "Constructor", "Generic", generic.Name, generic.Group);
                }
            }

            this.GenericNames = new HashSet<string>(NetNameMap.Generics.Select(g => g.Name));
        }

        /// <summary>
        /// Gets the generics.
        /// </summary>
        /// <value>
        /// The generics.
        /// </value>
        public static IEnumerable<Generic> Generics
        {
            get
            {
                return NetNameMap.GenericList.Where(g => !g.IsVariant);
            }
        }

        /// <summary>
        /// Gets the supported generics.
        /// </summary>
        /// <value>
        /// The supported generics.
        /// </value>
        public static IEnumerable<string> SupportedGenerics
        {
            get
            {
                return NetNameMap.GenericList.Where(g => Settings.IsDLCOwned(g.DLC)).Select(g => g.Name);
            }
        }

        /// <summary>
        /// Gets the mapped name for the specified net information.
        /// </summary>
        /// <value>
        /// The mapped names.
        /// </value>
        /// <param name="netInfo">The net information.</param>
        /// <returns>The mapped name.</returns>
        public string this[NetInfo netInfo]
        {
            get
            {
                return this.NetName(netInfo);
            }
        }

        /// <summary>
        /// Gets the fallback name for the net name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The fallback name.</returns>
        public static string GetFallbackName(string name)
        {
            string fallBackName;
            return NetNameMap.FallBackNameList.TryGetValue(name, out fallBackName) ? fallBackName : null;
        }

        /// <summary>
        /// Gets the group.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultGroup">The default group.</param>
        /// <param name="fallBack">If set to <c>true</c> fall back based on net name.</param>
        /// <returns>The group.</returns>
        public static string GetGroup(string name, string defaultGroup = null, bool fallBack = true)
        {
            if (!String.IsNullOrEmpty(name))
            {
                foreach (KeyValuePair<string, string> group in NetNameMap.GroupMap)
                {
                    if (name.Length >= group.Key.Length && name.Substring(0, group.Key.Length) == group.Key)
                    {
                        return group.Value;
                    }
                }
            }

            string groupName = null;

            if (!String.IsNullOrEmpty(name))
            {
                if (fallBack)
                {
                    string fallBackName = GetFallbackName(name);
                    if (!String.IsNullOrEmpty(fallBackName))
                    {
                        groupName = NetNameMap.GetGroup(fallBackName, "", false);
                        if (!String.IsNullOrEmpty(groupName))
                        {
                            return groupName;
                        }
                    }
                }
            }

            if (defaultGroup != null)
            {
                return (defaultGroup == "") ? null : defaultGroup;
            }

            int order = -1;

            foreach (KeyValuePair<string, int> group in NetNameMap.NetGroupList)
            {
                if (group.Value > order)
                {
                    groupName = group.Key;
                    order = group.Value;
                }
            }

            return groupName;
        }

        /// <summary>
        /// Determines whether the specified name is a supported generic.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if the specified name is a supported generic; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSupportedGeneric(string name)
        {
            name = name.ToLowerInvariant();
            return NetNameMap.GenericList.FindIndex(g => g.Name.ToLowerInvariant() == name) >= 0;
        }

        /// <summary>
        /// Gets the net display name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The display name.</returns>
        public string DisplayName(string name)
        {
            string displayName;

            if (name.Length > 6 && name.Substring(name.Length - 7, 7) == " Tunnel")
            {
                name = name.Substring(0, name.Length - 7);

                if (NetNameMap.DisplayNames.TryGetValue(name, out displayName))
                {
                    return displayName + " Tunnel";
                }
                else
                {
                    return name + " Tunnel";
                }
            }
            else if (NetNameMap.DisplayNames.TryGetValue(name, out displayName))
            {
                return displayName;
            }
            else
            {
                return name;
            }
        }

        /// <summary>
        /// Check if generics exists.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>True if generics exists.</returns>
        public bool GenericExists(string name)
        {
            return this.GenericNames.Contains(name);
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

            foreach (Generic generic in NetNameMap.Generics)
            {
                if (generic.LowerCaseName == lowerName)
                {
                    return (order >= 0) ? generic.Copy(order, name) : generic;
                }
            }

            string fallBackName = null;
            if (NetNameMap.FallBackNameList.TryGetValue(name, out fallBackName))
            {
                fallBackName = fallBackName.ToLowerInvariant();

                foreach (Generic generic in NetNameMap.Generics)
                {
                    if (generic.LowerCaseName == fallBackName)
                    {
                        return (order >= 0) ? generic.Copy(order, name) : generic;
                    }
                }
            }

            foreach (Generic generic in NetNameMap.Generics)
            {
                if (lowerName.Contains(generic.LowerCaseName))
                {
                    return generic.Copy(name, (order >= 0) ? order : generic.Order, name);
                }
            }

            if (!String.IsNullOrEmpty(fallBackName))
            {
                foreach (Generic generic in NetNameMap.Generics)
                {
                    if (fallBackName.Contains(generic.LowerCaseName))
                    {
                        return generic.Copy(name, (order >= 0) ? order : generic.Order, name);
                    }
                }
            }

            foreach (Generic generic in NetNameMap.Generics)
            {
                if (lowerName.Contains(generic.Part))
                {
                    return generic.Copy(name, (order >= 0) ? order : generic.Order, name);
                }
            }

            if (!String.IsNullOrEmpty(fallBackName))
            {
                foreach (Generic generic in NetNameMap.Generics)
                {
                    if (fallBackName.Contains(generic.Part))
                    {
                        return generic.Copy(name, (order >= 0) ? order : generic.Order, name);
                    }
                }
            }

            return new Generic(null, null, null, null, false, NetNameMap.GetGroup(name));
        }

        /// <summary>
        /// Check if net should be ignored.
        /// </summary>
        /// <param name="name">The net name.</param>
        /// <returns>True if net should be ignored.</returns>
        public bool IgnoreNet(string name)
        {
            return String.IsNullOrEmpty(name) ? true : this.ignoreNetsRex.IsMatch(name);
        }

        /// <summary>
        /// Check if net should be ignored.
        /// </summary>
        /// <param name="netInfo">The net information.</param>
        /// <returns>
        /// True if net should be ignored.
        /// </returns>
        public bool IgnoreNet(NetInfo netInfo)
        {
            return this.IgnoreNet(this.NetName(netInfo));
        }

        /// <summary>
        /// Check if net collection should be ignored.
        /// </summary>
        /// <param name="netCollection">The net collection.</param>
        /// <returns>
        /// True if net should be ignored.
        /// </returns>
        public bool IgnoreNetCollection(NetCollection netCollection)
        {
            return (netCollection == null || String.IsNullOrEmpty(netCollection.name)) ? true : this.ignoreNetCollectionsRex.IsMatch(netCollection.name);
        }

        /// <summary>
        /// Get text showing whether net collection should be ignored.
        /// </summary>
        /// <param name="netCollection">The net collection.</param>
        /// <returns>
        /// The text "Ignore" if net collection should be ignored, otherwise null.
        /// </returns>
        public string IgnoreNetCollectionText(NetCollection netCollection)
        {
            return (netCollection == null || String.IsNullOrEmpty(netCollection.name)) ? (string)null : this.ignoreNetCollectionsRex.IsMatch(netCollection.name) ? "Ignore" : (string)null;
        }

        /// <summary>
        /// Get text showing whether net should be ignored.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The text "Ignore" if net should be ignored, otherwise null.</returns>
        public string IgnoreNetText(string name)
        {
            return String.IsNullOrEmpty(name) ? (string)null : this.ignoreNetsRex.IsMatch(name) ? "Ignore" : (string)null;
        }

        /// <summary>
        /// Get text showing whether net should be ignored.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="netName">Name of the net.</param>
        /// <returns>The text "Ignore" if net should be ignored (with exclamation case if warning specified), otherwise null.</returns>
        public string IgnoreNetText(string collectionName, string netName)
        {
            if (String.IsNullOrEmpty(netName) || !this.ignoreNetsRex.IsMatch(netName))
            {
                return null;
            }

            if (this.WarnAboutIgnoredNet(collectionName, netName))
            {
                return "Ignore!";
            }

            return "Ignore";
        }

        /// <summary>
        /// Gets he order index for the net group.
        /// </summary>
        /// <param name="name">The group name.</param>
        /// <returns>The order index.</returns>
        public int NetGroupOrder(string name)
        {
            int order;
            return NetNameMap.NetGroupList.TryGetValue(name, out order) ? order : 999999;
        }

        /// <summary>
        /// Check if warning should be issued for ignored net.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="netName">Name of the net.</param>
        /// <returns>True if warning should be issued.</returns>
        public bool WarnAboutIgnoredNet(string collectionName, string netName)
        {
            return String.IsNullOrEmpty(collectionName) ? false : String.IsNullOrEmpty(netName) ? false : this.warnIgnoreNetCollectionsNetsRex.IsMatch(collectionName + ";" + netName);
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The order.</returns>
        private int GetOrder(string name)
        {
            int order;

            if (NetNameMap.DisplayOrders.TryGetValue(name, out order))
            {
                return order;
            }

            if (name.SafeSubstring(name.Length - 7) == " Tunnel" && NetNameMap.DisplayOrders.TryGetValue(name.Substring(0, name.Length - 7), out order))
            {
                return order;
            }

            return -1;
        }

        /// <summary>
        /// Get the nets name.
        /// </summary>
        /// <param name="netInfo">The net information.</param>
        /// <returns>The name.</returns>
        private string NetName(NetInfo netInfo)
        {
            string className = netInfo.m_class.name;
            string objectName = netInfo.name;

            string key = className + "|" + objectName;
            string name = null;

            // Return from map if exists.
            if (this.map.TryGetValue(key, out name))
            {
                return name;
            }

            bool tunnel = false;
            bool bridge = false;

            // Figure out name.
            try
            {
                if (className.SafeSubstring(className.Length - 6, 6) == "Tunnel")
                {
                    className = className.SafeSubstring(0, className.Length - 6).TrimEnd(' ');
                    tunnel = true;

                    // Order from Network Extensions chaos.
                    className = this.nextDoubleTunnelClasNameRestRex.Replace(className, "$1");
                }
                else if (objectName.Contains("Tunnel"))
                {
                    tunnel = true;
                }
                else if (className.SafeSubstring(className.Length - 6, 6) == "Bridge" || objectName.Contains("Bridge") ||
                         className.SafeSubstring(className.Length - 8, 8) == "Elevated" || objectName.Contains("Elevated"))
                {
                    bridge = true;
                }

                if (className == "Water Pipe" || objectName == "Water Pipe" || className == "Heating Pipe" || objectName == "Heating Pipe")
                {
                    name = "Pipe";
                }
                else if (className == "Electricity Wire" || objectName == "Electricity Wire")
                {
                    name = "Wire";
                }
                else if (className == "Electricity Dam" || objectName == "Dam")
                {
                    name = "Dam";
                }
                else if (this.canalNameRex.IsMatch(objectName) || this.canalNameRex.IsMatch(className))
                {
                    name = "Canal";
                }
                else if (this.quayNameRex.IsMatch(objectName) || this.quayNameRex.IsMatch(className))
                {
                    name = "Quay";
                }
                else if (this.floodWallNameRex.IsMatch(objectName) || this.floodWallNameRex.IsMatch(className))
                {
                    name = "Flood Wall";
                }
                else if (this.castleWallNameRex.IsMatch(objectName) || this.castleWallNameRex.IsMatch(className))
                {
                    name = "Castle Wall";
                }
                else if (this.trenchNameRex.IsMatch(objectName) || this.trenchNameRex.IsMatch(className))
                {
                    name = "Trench";
                }
                else if (this.tramTrackRoadObjectNameRex.IsMatch(objectName))
                {
                    name = "Tram Track";
                }
                else if (this.monorailTrackRoadObjectNameRex.IsMatch(objectName))
                {
                    name = "Monorail Track";
                }
                else if (className == "Highway")
                {
                    // Standard game. Separate ramp from highways.
                    if (objectName.Contains("Ramp"))
                    {
                        name = "Highway Ramp";
                    }
                }
                else if (objectName.SafeSubstring(0, 19) == "Zonable Pedestrian ")
                {
                    // Network Extensions zonable pedestrian.
                    if (objectName.SafeSubstring(19, 1) == "Gravel")
                    {
                        name = "Gravel Road";
                    }
                    else
                    {
                        name = "Tiny Road";
                    }
                }
                else if (className.SafeSubstring(0, 10) == "Pedestrian")
                {
                    // Standard game. Separate bicyle from pedestrian.
                    if (objectName.Contains("Bicycle"))
                    {
                        name = "Bicycle";

                        if (!tunnel)
                        {
                            name += " Path";
                        }
                    }
                    else
                    {
                        name = "Pedestrian";

                        if (!tunnel && !bridge)
                        {
                            name += " Path";
                        }
                    }
                }
                else if (className.SafeLeftString(6) == "Blimp " && className.SafeRightString(5) == " Path")
                {
                    name = "Blimp Path";
                }
                else if (className.SafeLeftString(9) == "CableCar " && className.SafeRightString(5) == " Path")
                {
                    name = "Cable Car Path";
                }
                else if (this.nextSmallHeavyRoadClassNameRex.IsMatch(className))
                {
                    // Network Extensions small heavy.
                    name = "Small Heavy Road";
                }
                else if (this.nextSmallRoadClassNameRex.IsMatch(className))
                {
                    // Network Extensions small.
                    name = "Small Road";
                }
                else if (this.nextTinyRoadClassNameRex.IsMatch(className))
                {
                    // Network Extensions tiny.
                    name = "Tiny Road";
                }
                else if (this.nextMediumRoadClassNameRex.IsMatch(className))
                {
                    // Network Extensions medium.
                    name = "Medium Road";
                }
                else if (this.nextLargeRoadClassNameRex.IsMatch(className))
                {
                    // Network Extensions large.
                    name = "Large Road";
                }
                else if (this.nextHighwayClassNameRex.IsMatch(className))
                {
                    // Network Extensions highways.
                    if ((objectName.Contains("Small") && objectName.Contains("Rural")) || netInfo.GetLocalizedTitle().Contains("National"))
                    {
                        // Rural Highway (National Road).
                        name = "Rural Highway";
                    }
                    else
                    {
                        name = "Highway";
                    }
                }
                else if (className == "Large Road")
                {
                    // Order from Network Extensions chaos.
                    if (objectName.Contains("Highway"))
                    {
                        name = "Highway";
                    }
                }
                else if (objectName.Contains("Rural Highway"))
                {
                    // Order from Network Extensions chaos.
                    if (className.SafeSubstring(className.Length - 2, 2) == "2L" || netInfo.GetLocalizedTitle().Contains("Two-Lane Highway"))
                    {
                        name = "Highway";
                    }
                    else
                    {
                        name = "Rural Highway";
                    }
                }

                if (name == null)
                {
                    // Use original name.
                    name = netInfo.m_class.name;
                }
                else if (tunnel && !name.Contains("Tunnel"))
                {
                    name += " Tunnel";
                }
                else if (bridge && name.SafeSubstring(0, 10) == "Pedestrian" && !name.Contains("Bridge") && !name.Contains("Elevated"))
                {
                    name += " Bridge";
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(NetNameMap), "NetName", ex, netInfo, key);

                name = null;
            }

            // Store in map and return.
            this.map[key] = name;

            if (Log.LogToFile && Log.LogALot)
            {
                Log.Debug(typeof(NetNameMap), "NetName", netInfo.m_class.name, objectName, netInfo.GetLocalizedTitle(), tunnel, className, name);
            }

            return name;
        }

        /// <summary>
        /// Generic net thingy name-part pair.
        /// </summary>
        public struct Generic
        {
            /// <summary>
            /// The expansion DLC.
            /// </summary>
            public SteamHelper.DLC? DLC;

            /// <summary>
            /// The group.
            /// </summary>
            public string Group;

            /// <summary>
            /// The generic is a variant and not a main type.
            /// </summary>
            public bool IsVariant;

            /// <summary>
            /// The lower case name.
            /// </summary>
            public string LowerCaseName;

            /// <summary>
            /// The maximum limit value.
            /// </summary>
            private float? maxLimitValue;

            /// <summary>
            /// The maximum limit.
            /// </summary>
            public float MaxLimit
            {
                get
                {
                    return (this.maxLimitValue != null && this.maxLimitValue.HasValue) ? Math.Max(this.maxLimitValue.Value, Global.Settings.MaximumLimit)  : Global.Settings.MaximumLimit;
                }
            }

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
            /// <param name="name">The name.</param>
            /// <param name="part">The part.</param>
            /// <param name="dlc">The DLC.</param>
            /// <param name="order">The sort order.</param>
            /// <param name="isVariant">If set to <c>true</c>, set as variant.</param>
            /// <param name="group">The group.</param>
            public Generic(string name, string part, SteamHelper.DLC? dlc, int? order, bool isVariant = false, string group = null)
            {
                this.Name = name;
                this.Part = part;
                this.DLC = dlc;
                this.LowerCaseName = (name == null) ? null : name.ToLowerInvariant();
                this.IsVariant = isVariant;

                this.Group = String.IsNullOrEmpty(group) ? NetNameMap.GetGroup(name) : group;

                if (order != null && order.HasValue)
                {
                    this.Order = order.Value;
                }
                else if (String.IsNullOrEmpty(group) || !NetNameMap.NetGroupList.TryGetValue(group, out this.Order))
                {
                    this.Order = -1;
                }

                if (this.Order < 0)
                {
                    this.Order += 10000;
                }

                float limit;
                if (!String.IsNullOrEmpty(Part) && MaxLimits.TryGetValue(this.Part, out limit))
                {
                    this.maxLimitValue = limit;
                }
                else
                {
                    this.maxLimitValue = null;
                }
            }

            /// <summary>
            /// Copies this instance.
            /// </summary>
            /// <returns>A copy of this instance.</returns>
            public Generic Copy()
            {
                return this.Copy(this.Name, this.Order, (string)null);
            }

            /// <summary>
            /// Copies this instance.
            /// </summary>
            /// <param name="order">The order.</param>
            /// <param name="netName">Name of the net.</param>
            /// <returns>
            /// A copy of this instance.
            /// </returns>
            public Generic Copy(int order, string netName)
            {
                return this.Copy(this.Name, order, netName);
            }

            /// <summary>
            /// Copies this instance.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="netName">Name of the net.</param>
            /// <returns>
            /// A copy of this instance.
            /// </returns>
            public Generic Copy(string name, string netName)
            {
                return this.Copy(name, this.Order, netName);
            }

            /// <summary>
            /// Copies this instance.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="order">The order.</param>
            /// <param name="netName">Name of the net.</param>
            /// <returns>
            /// A copy of this instance.
            /// </returns>
            public Generic Copy(string name, int order, string netName)
            {
                return new Generic(name, this.Part, this.DLC, order, this.IsVariant, NetNameMap.GetGroup(netName, this.Group));
            }
        }
    }
}