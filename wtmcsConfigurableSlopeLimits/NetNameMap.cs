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
        private static readonly Regex LargeRoad = new Regex("Large.*?(?:Road|Avenue)$");

        /// <summary>
        /// Matches medium road class name.
        /// </summary>
        private static readonly Regex MediumRoad = new Regex("Medium.*?(?:Road|Avenue)$");

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
        private static readonly Regex NExtLargeRoad = new Regex("^NExt.*?Large.*?(?:Road|Avenue)$");

        /// <summary>
        /// Matches Network Extensions medium road class name.
        /// </summary>
        private static readonly Regex NExtMediumRoad = new Regex("^NExt.*?Medium.*?(?:Road|Avenue)$");

        /// <summary>
        /// Matches Network Extensions small heavy road class name.
        /// </summary>
        private static readonly Regex NExtSmallHeavyRoad = new Regex("^NExt.*?Small3L(Road|Avenue)$");

        /// <summary>
        /// Matches Network Extensions small road class name.
        /// </summary>
        private static readonly Regex NExtSmallRoad = new Regex("^NExt.*?Small.*?(?:Road|Avenue)$");

        /// <summary>
        /// Matches small road class name.
        /// </summary>
        private static readonly Regex SmallRoad = new Regex("Small.*?(?:Road|Avenue)$");

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

            // Figure out name.
            string name = null;
            bool tunnel = false;
            string className = netInfo.m_class.name;

            if (className.Substring(className.Length - 6, 6) == "Tunnel")
            {
                className = className.Substring(0, className.Length - 6).TrimEnd(' ');
                tunnel = true;

                // Order from Network Extensions chaos.
                className = NExtDoubleTunnelRest.Replace(className, "$1");
            }
            else if (netInfo.name.Contains("Tunnel"))
            {
                tunnel = true;
            }

            if (className == "Highway")
            {
                // Standard game. Separate ramp from highways.
                if (netInfo.name.Contains("Ramp"))
                {
                    name = "Highway Ramp";
                }
            }
            else if (className.Length >= 10 && className.Substring(0, 10) == "Pedestrian")
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
                if ((netInfo.name.Contains("Small") && netInfo.name.Contains("Rural")) || netInfo.GetLocalizedTitle().Contains("National"))
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
                if (netInfo.name.Contains("Highway"))
                {
                    name = "Highway";
                }
            }
            else if (netInfo.name.Contains("Rural Highway"))
            {
                // Order from Network Extensions chaos.
                if (className.Substring(className.Length - 2, 2) == "2L" || netInfo.GetLocalizedTitle().Contains("Two-Lane Highway"))
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
            else if (tunnel)
            {
                name += " Tunnel";
            }

            // Store in map and return.
            map[key] = name;

            if (Log.LogToFile && Log.LogALot)
            {
                Log.Debug(typeof(NetNameMap), "NetName", netInfo.m_class.name, netInfo.name, netInfo.GetLocalizedTitle(), className, tunnel, name);
            }

            return name;
        }
    }
}