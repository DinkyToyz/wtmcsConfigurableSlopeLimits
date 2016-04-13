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
        protected readonly Dictionary<string, string> FallBackNameList = new Dictionary<string, string>
        {
            { "Tiny Road", "Small Road" },
            { "Small Heavy Road", "Small Road" },
            { "Rural Highway", "Highway" },
            { "National Road", "Highway" }
        };

        /// <summary>
        /// The generics.
        /// </summary>
        protected readonly List<Generic> GenericList = new List<Generic>
        {
            new Generic("Highway Ramp", "ramp", "Roads", SteamHelper.DLC.None, 500),
            new Generic("Highway Ramp Tunnel", "ramp", "Roads", SteamHelper.DLC.None, 500, true),
            new Generic("Highway", "high", "Roads", SteamHelper.DLC.None, 600),
            new Generic("Highway Tunnel", "high", "Roads", SteamHelper.DLC.None, 600, true),
            new Generic("Large Road", "large", "Roads", SteamHelper.DLC.None, 400),
            new Generic("Large Road Tunnel", "large", "Roads", SteamHelper.DLC.None, 400, true),
            new Generic("Medium Road", "medium", "Roads", SteamHelper.DLC.None, 300),
            new Generic("Medium Road Tunnel", "medium", "Roads", SteamHelper.DLC.None, 300, true),
            new Generic("Small Road", "small", "Roads", SteamHelper.DLC.None, 200),
            new Generic("Small Road Tunnel", "small", "Roads", SteamHelper.DLC.None, 200, true),
            new Generic("Gravel Road", "gravel", "Roads", SteamHelper.DLC.None, 100),
            new Generic("Gravel Road Tunnel", "gravel", "Roads", SteamHelper.DLC.None, 100, true),
            new Generic("Train Track", "track", "Railroads", SteamHelper.DLC.None, 1200),
            new Generic("Train Track Tunnel", "track", "Railroads", SteamHelper.DLC.None, 1200, true),
            new Generic("Metro Track", "track", "Railroads", SteamHelper.DLC.None, 1000),
            new Generic("Pedestrian Path", "pedestrian", "Paths", SteamHelper.DLC.None, 700),
            new Generic("Pedestrian Bridge", "pedestrian", "Paths", SteamHelper.DLC.None, 700, true),
            new Generic("Pedestrian Tunnel", "pedestrian", "Paths", SteamHelper.DLC.None, 700, true),
            new Generic("Bicycle Path", "bicycle", "Paths", SteamHelper.DLC.AfterDarkDLC, 800),
            new Generic("Bicycle Tunnel", "bicycle", "Paths", SteamHelper.DLC.AfterDarkDLC, 800, true),
            new Generic("Airplane Runway", "runway", "Runways", SteamHelper.DLC.None, 1200),
            new Generic("Tram Track", "tram", "Railroads", SteamHelper.DLC.SnowFallDLC, 900),
            new Generic("Tram Track Tunnel", "tram", "Railroads", SteamHelper.DLC.SnowFallDLC, 900, true),
        };

        /// <summary>
        /// The generic names.
        /// </summary>
        protected readonly HashSet<String> GenericNames;

        /// <summary>
        /// The net groups.
        /// </summary>
        protected readonly Dictionary<string, int> NetGroupList = new Dictionary<string, int>
        {
            { "Roads", 1 },
            { "Paths", 2 },
            { "Railroads", 3 },
            { "Runways", 4 },
            { "Other", 5 }
        };

        /// <summary>
        /// Matches canal names.
        /// </summary>
        private readonly Regex canal = new Regex("(?:^|Landscaping )?Canal(?: ?\\d+)?$");

        /// <summary>
        /// Matches castle wall names.
        /// </summary>
        private readonly Regex castleWall = new Regex("^Castle Walls?(?: ?\\d+)?$");

        /// <summary>
        /// The display names.
        /// </summary>
        private readonly Dictionary<string, string> displayNames = new Dictionary<string, string>
        {
            { "Rural Highway", "National Road" }
        };

        /// <summary>
        /// The display orders.
        /// </summary>
        private readonly Dictionary<string, int> displayOrders = new Dictionary<string, int>
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

        /// <summary>
        /// Matches castle wall names.
        /// </summary>
        private readonly Regex floodWall = new Regex("(?:^|Landscaping )?Flood Wall(?: ?\\d+)?$");

        /// <summary>
        /// The net collections that should be ignored.
        /// </summary>
        private readonly Regex ignoreNetCollectionsRex = new Regex("^(?:Electricity|Water)$", RegexOptions.IgnoreCase);

        /// <summary>
        /// The nets that should be ignored.
        /// </summary>
        private readonly Regex ignoreNetsRex = new Regex("(?:(?:^NExt)|(?:^Bus Stop$)|(?:(?: (?:Pipe|Transport|Connection|Line|Dock|Wire|Dam))|(?:(?<!Pedestrian|Bicycle) Path)$))", RegexOptions.IgnoreCase);

        /// <summary>
        /// Matches large road class name.
        /// </summary>
        private readonly Regex largeRoad = new Regex("Large.*?(?:Road|Avenue)(?:TL)?$");

        /// <summary>
        /// Matches medium road class name.
        /// </summary>
        private readonly Regex mediumRoad = new Regex("Medium.*?(?:Road|Avenue)(?:TL)?$");

        /// <summary>
        /// Matches Network Extensions double tunnel class name left-over.
        /// </summary>
        private readonly Regex nextDoubleTunnelRest = new Regex("Tunnel(\\d+L)$");

        /// <summary>
        /// Matches Network Extensions highway class name.
        /// </summary>
        private readonly Regex nextHighway = new Regex("^NExt.*?Highway(?:\\d+L)$");

        /// <summary>
        /// Matches Network Extensions large road class name.
        /// </summary>
        private readonly Regex nextLargeRoad = new Regex("^NExt.*?Large.*?(?:Road|Avenue)(?:TL)?$");

        /// <summary>
        /// Matches Network Extensions medium road class name.
        /// </summary>
        private readonly Regex nextMediumRoad = new Regex("^NExt.*?Medium.*?(?:Road|Avenue)(?:TL)?$");

        /// <summary>
        /// Matches Network Extensions small heavy road class name.
        /// </summary>
        private readonly Regex nextSmallHeavyRoad = new Regex("^NExt.*?Small[3-9]L(Road|Avenue)(?:TL)?$");

        /// <summary>
        /// Matches Network Extensions small road class name.
        /// </summary>
        private readonly Regex nextSmallRoad = new Regex("^NExt.*?Small.*?(?:Road|Avenue)(?:TL)?$");

        /// <summary>
        /// Matches Network Extensions tiny road class name.
        /// </summary>
        private readonly Regex nextTinyRoad = new Regex("^NExt.*?(?:[12]LAlley|1LOneway)(?:TL)?$");

        /// <summary>
        /// Matches quay names.
        /// </summary>
        private readonly Regex quay = new Regex("(?:^|Landscaping )?Quay(?: ?\\d+)?$");

        /// <summary>
        /// Matches small road class name.
        /// </summary>
        private readonly Regex smallRoad = new Regex("Small.*?(?:Road|Avenue)(?:TL)?$");

        /// <summary>
        /// Matches tram track/road object name.
        /// </summary>
        private readonly Regex tramTrackRoad = new Regex("(?:(^| )Road(?: .*?)? Tram( |$)|(?:^| )Tram(?: Depot)? (?:Track|Road)( |$))");

        /// <summary>
        /// Matches trench names.
        /// </summary>
        private readonly Regex trench = new Regex("^Trench Ruins?(?: ?\\d+)?$");

        /// <summary>
        /// The net collections and nets combinations for which to warn about ignored nets.
        /// </summary>
        private readonly Regex warnIgnoreNetCollectionsNetsRex = new Regex("^(?:(?:[^;]*?Road|Beautification);.*|Expansion \\d+;.*(?:Road|Path|Tunnel|Track)|Public Transport;(?:Road|Tunnel|Track))$", RegexOptions.IgnoreCase);

        /// <summary>
        /// The map.
        /// </summary>
        private Dictionary<string, string> map = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NetNameMap"/> class.
        /// </summary>
        public NetNameMap()
        {
            this.GenericNames = new HashSet<string>(this.Generics.Select(g => g.Name));
        }

        /// <summary>
        /// Gets the generics.
        /// </summary>
        /// <value>
        /// The generics.
        /// </value>
        public IEnumerable<Generic> Generics
        {
            get
            {
                return GenericList.Where(g => !g.IsVariant);
            }
        }

        /// <summary>
        /// Gets the supported generics.
        /// </summary>
        /// <value>
        /// The supported generics.
        /// </value>
        public IEnumerable<string> SupportedGenerics
        {
            get
            {
                return this.GenericList.Where(g => Settings.IsDLCOwned(g.DLC)).Select(g => g.Name);
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

                if (this.displayNames.TryGetValue(name, out displayName))
                {
                    return displayName + " Tunnel";
                }
                else
                {
                    return name + " Tunnel";
                }
            }
            else if (this.displayNames.TryGetValue(name, out displayName))
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
            return GenericNames.Contains(name);
        }

        /// <summary>
        /// Gets the fallback name for the net name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The fallback name.</returns>
        public string GetFallbackName(string name)
        {
            string fallBackName;
            return FallBackNameList.TryGetValue(name, out fallBackName) ? fallBackName : null;
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

            foreach (Generic generic in this.Generics)
            {
                if (generic.LowerCaseName == lowerName)
                {
                    return (order >= 0) ? generic.Copy(order) : generic;
                }
            }

            string fallBackName = null;
            if (this.FallBackNameList.TryGetValue(name, out fallBackName))
            {
                fallBackName = fallBackName.ToLowerInvariant();

                foreach (Generic generic in this.Generics)
                {
                    if (generic.LowerCaseName == fallBackName)
                    {
                        return (order >= 0) ? generic.Copy(order) : generic;
                    }
                }
            }

            foreach (Generic generic in this.Generics)
            {
                if (lowerName.Contains(generic.LowerCaseName))
                {
                    return generic.Copy(name, (order >= 0) ? order : generic.Order);
                }
            }

            if (!String.IsNullOrEmpty(fallBackName))
            {
                foreach (Generic generic in this.Generics)
                {
                    if (fallBackName.Contains(generic.LowerCaseName))
                    {
                        return generic.Copy(name, (order >= 0) ? order : generic.Order);
                    }
                }
            }

            foreach (Generic generic in this.Generics)
            {
                if (lowerName.Contains(generic.Part))
                {
                    return generic.Copy(name, (order >= 0) ? order : generic.Order);
                }
            }

            if (!String.IsNullOrEmpty(fallBackName))
            {
                foreach (Generic generic in this.Generics)
                {
                    if (fallBackName.Contains(generic.Part))
                    {
                        return generic.Copy(name, (order >= 0) ? order : generic.Order);
                    }
                }
            }

            Generic result = new Generic(-1);

            foreach (KeyValuePair<string, int> group in this.NetGroupList)
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
        /// Gets he order index for the net group.
        /// </summary>
        /// <param name="name">The group name.</param>
        /// <returns>The order index.</returns>
        public int NetGroupOrder(string name)
        {
            return NetGroupList[name];
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

            if (this.displayOrders.TryGetValue(name, out order))
            {
                return order;
            }

            if (name.SafeSubstring(name.Length - 7) == " Tunnel" && this.displayOrders.TryGetValue(name.Substring(0, name.Length - 7), out order))
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

            // Figure out name.
            try
            {
                if (className.SafeSubstring(className.Length - 6, 6) == "Tunnel")
                {
                    className = className.SafeSubstring(0, className.Length - 6).TrimEnd(' ');
                    tunnel = true;

                    // Order from Network Extensions chaos.
                    className = this.nextDoubleTunnelRest.Replace(className, "$1");
                }
                else if (objectName.Contains("Tunnel"))
                {
                    tunnel = true;
                }

                if (className == "Water Pipe" || objectName == "Water Pipe")
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
                else if (this.canal.IsMatch(objectName) || this.canal.IsMatch(className))
                {
                    name = "Canal";
                }
                else if (this.quay.IsMatch(objectName) || this.quay.IsMatch(className))
                {
                    name = "Quay";
                }
                else if (this.floodWall.IsMatch(objectName) || this.floodWall.IsMatch(className))
                {
                    name = "Flood Wall";
                }
                else if (this.castleWall.IsMatch(objectName) || this.castleWall.IsMatch(className))
                {
                    name = "Castle Wall";
                }
                else if (this.trench.IsMatch(objectName) || this.trench.IsMatch(className))
                {
                    name = "Trench";
                }
                else if (this.tramTrackRoad.IsMatch(objectName))
                {
                    name = "Tram Track";
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
                    name = "Tiny Road";
                }
                else if (className.SafeSubstring(0, 10) == "Pedestrian")
                {
                    // Standard game. Separate bicyle from pedestrian.
                    if (objectName.Contains("Bicycle"))
                    {
                        name = "Bicycle";
                    }
                    else
                    {
                        name = "Pedestrian";
                    }

                    if (!tunnel)
                    {
                        name += " Path";
                    }
                }
                else if (this.nextSmallHeavyRoad.IsMatch(className))
                {
                    // Network Extensions small heavy.
                    name = "Small Heavy Road";
                }
                else if (this.nextSmallRoad.IsMatch(className))
                {
                    // Network Extensions small.
                    name = "Small Road";
                }
                else if (this.nextTinyRoad.IsMatch(className))
                {
                    // Network Extensions tiny.
                    name = "Tiny Road";
                }
                else if (this.nextMediumRoad.IsMatch(className))
                {
                    // Network Extensions medium.
                    name = "Medium Road";
                }
                else if (this.nextLargeRoad.IsMatch(className))
                {
                    // Network Extensions large.
                    name = "Large Road";
                }
                else if (this.nextHighway.IsMatch(className))
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
                else
                {
                    if (tunnel && !name.Contains("Tunnel"))
                    {
                        name += " Tunnel";
                    }
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
                this.DLC = null;
                this.IsVariant = false;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Generic" /> struct.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="part">The part.</param>
            /// <param name="group">The group.</param>
            /// <param name="dlc">The DLC.</param>
            /// <param name="order">The sort order.</param>
            /// <param name="isVariant">If set to <c>true</c>, set as variant.</param>
            public Generic(string name, string part, string group, SteamHelper.DLC? dlc, int order, bool isVariant = false)
            {
                this.Name = name;
                this.Part = part;
                this.Group = group;
                this.Order = order;
                this.DLC = dlc;
                this.LowerCaseName = name.ToLowerInvariant();
                this.IsVariant = isVariant;
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
                return new Generic(name, this.Part, this.Group, this.DLC, order, this.IsVariant);
            }
        }
    }
}