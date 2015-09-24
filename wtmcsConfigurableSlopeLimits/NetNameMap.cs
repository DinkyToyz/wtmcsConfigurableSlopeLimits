using System.Collections.Generic;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Network name map extension.
    /// </summary>
    public static class NetNameMap
    {
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

            if (className.Substring(className.Length - 7, 7) == " Tunnel")
            {
                className = className.Substring(0, className.Length - 7);
                tunnel = true;
            }
            else if (netInfo.name.Contains("Tunnel"))
            {
                tunnel = true;
            }

            /*
             * Network Extensions v0.6.1
             *
             * Small Road; Small Avenue; Small Avenue (NetInfo); Small Four-Lane Road
             *
             * NExtMediumAvenue; Medium Avenue; Medium Avenue (NetInfo); Four-Lane Road
             * NExtMediumAvenue; Medium Avenue TL; Medium Avenue TL (NetInfo); Four-Lane Road with Turning Lane
             *
             * NExtHighway; Small Rural Highway; Small Rural Highway (NetInfo); Rural Highway
             * Small Road; Rural Highway Elevated; Rural Highway Elevated (NetInfo); NET_TITLE[Rural Highway Elevated]:0
             * Small Road Tunnel; Rural Highway Tunnel; Rural Highway Tunnel (NetInfo); NET_TITLE[Rural Highway Tunnel]:0
             * Small Road; Rural Highway Slope; Rural Highway Slope (NetInfo); NET_TITLE[Rural Highway Slope]:0
             *
             * NExtHighway; Rural Highway; Rural Highway (NetInfo); Two-Lane Highway
             *
             * NExtHighway; Large Highway; Large Highway (NetInfo); Six-Lane Highway
             * Large Road; Large Highway Elevated; Large Highway Elevated (NetInfo); NET_TITLE[Large Highway Elevated]:0
             * Large Road; Large Highway Bridge; Large Highway Bridge (NetInfo); NET_TITLE[Large Highway Bridge]:0
             * Large Road Tunnel; Large Highway Tunnel; Large Highway Tunnel (NetInfo); NET_TITLE[Large Highway Tunnel]:0
             * Large Road; Large Highway Slope; Large Highway Slope (NetInfo); NET_TITLE[Large Highway Slope]:0
             *
             */

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
            else if (className == "NExtMediumAvenue")
            {
                // Network Extensions avenue.
                name = "Medium Road";
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
                if (netInfo.GetLocalizedTitle().Contains("Two-Lane Highway"))
                {
                    name = "Highway";
                }
                else
                {
                    name = "Rural Highway";
                }
            }
            else if (className == "NExtHighway")
            {
                // Network Extensions highway.
                name = "Highway";
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

            Log.Debug(typeof(NetNameMap), "NetName", netInfo.m_class.name, netInfo.name, netInfo.GetLocalizedTitle(), className, tunnel, name);
            return name;
        }
    }
}
