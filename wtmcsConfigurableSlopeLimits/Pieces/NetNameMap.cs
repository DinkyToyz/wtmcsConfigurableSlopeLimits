using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Network name map extension.
    /// </summary>
    public static class NetNameMap
    {
        /// <summary>
        /// Matches large road class name.
        /// </summary>
        private static readonly Regex LargeRoad = new Regex("Large.*?(?:Road|Avenue)(?:TL)?$");

        /// <summary>
        /// Matches medium road class name.
        /// </summary>
        private static readonly Regex MediumRoad = new Regex("Medium.*?(?:Road|Avenue)(?:TL)?$");

        /// <summary>
        /// Matches Network Extensions double tunnel class name left-over.
        /// </summary>
        private static readonly Regex NExtDoubleTunnelRest = new Regex("Tunnel(\\d+L)$");

        /// <summary>
        /// Matches Network Extensions highway class name.
        /// </summary>
        private static readonly Regex NExtHighway = new Regex("^NExt.*?Highway(?:\\d+L)$");

        /// <summary>
        /// Matches Network Extensions large road class name.
        /// </summary>
        private static readonly Regex NExtLargeRoad = new Regex("^NExt.*?Large.*?(?:Road|Avenue)(?:TL)?$");

        /// <summary>
        /// Matches Network Extensions medium road class name.
        /// </summary>
        private static readonly Regex NExtMediumRoad = new Regex("^NExt.*?Medium.*?(?:Road|Avenue)(?:TL)?$");

        /// <summary>
        /// Matches Network Extensions small heavy road class name.
        /// </summary>
        private static readonly Regex NExtSmallHeavyRoad = new Regex("^NExt.*?Small[3-9]L(Road|Avenue)(?:TL)?$");

        /// <summary>
        /// Matches Network Extensions small road class name.
        /// </summary>
        private static readonly Regex NExtSmallRoad = new Regex("^NExt.*?Small.*?(?:Road|Avenue)(?:TL)?$");

        /// <summary>
        /// Matches Network Extensions tiny road class name.
        /// </summary>
        private static readonly Regex NExtTinyRoad = new Regex("^NExt.*?(?:[12]LAlley|1LOneway)(?:TL)?$");

        /// <summary>
        /// Matches small road class name.
        /// </summary>
        private static readonly Regex SmallRoad = new Regex("Small.*?(?:Road|Avenue)(?:TL)?$");

        /// <summary>
        /// Matches tram track/road object name.
        /// </summary>
        private static readonly Regex TramTrackRoad = new Regex("(?:(^| )Road(?: .*?)? Tram( |$)|(?:^| )Tram(?: Depot)? (?:Track|Road)( |$))");

        /// <summary>
        /// The map.
        /// </summary>
        private static Dictionary<string, string> map = new Dictionary<string, string>();

        /// <summary>
        /// Get the nets name.
        /// </summary>
        /// <param name="netInfo">The net information.</param>
        /// <returns>The name.</returns>
        public static string NetName(this NetInfo netInfo)
        {
            string key = netInfo.m_class.name + "|" + netInfo.name;

            // Return from map if exists.
            if (map.ContainsKey(key))
            {
                return map[key];
            }

            string className = netInfo.m_class.name;
            string objectName = netInfo.name;

            string name = null;
            bool tunnel = false;

            // Figure out name.
            try
            {
                if (className.SafeSubstring(className.Length - 6, 6) == "Tunnel")
                {
                    className = className.SafeSubstring(0, className.Length - 6).TrimEnd(' ');
                    tunnel = true;

                    // Order from Network Extensions chaos.
                    className = NExtDoubleTunnelRest.Replace(className, "$1");
                }
                else if (objectName.Contains("Tunnel"))
                {
                    tunnel = true;
                }

                if (TramTrackRoad.IsMatch(objectName))
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
                else if (className.SafeSubstring(0, 10) == "Pedestrian")
                {
                    // Standard game. Separate bicyle from pedestrian.
                    if (tunnel)
                    {
                        name = "Bicycle";
                    }
                    else
                    {
                        name = "Bicycle Path";
                    }
                }
                else if (NExtSmallHeavyRoad.IsMatch(className))
                {
                    // Network Extensions small heavy.
                    name = "Small Heavy Road";
                }
                else if (NExtSmallRoad.IsMatch(className))
                {
                    // Network Extensions small.
                    name = "Small Road";
                }
                else if (NExtTinyRoad.IsMatch(className))
                {
                    // Network Extensions tiny.
                    name = "Tiny Road";
                }
                else if (NExtMediumRoad.IsMatch(className))
                {
                    // Network Extensions medium.
                    name = "Medium Road";
                }
                else if (NExtLargeRoad.IsMatch(className))
                {
                    // Network Extensions large.
                    name = "Large Road";
                }
                else if (NExtHighway.IsMatch(className))
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
            map[key] = name;

            if (Log.LogToFile && Log.LogALot)
            {
                Log.Debug(typeof(NetNameMap), "NetName", netInfo.m_class.name, objectName, netInfo.GetLocalizedTitle(), tunnel, className, name);
            }

            return name;
        }
    }
}