using System.Text;
using System.Text.RegularExpressions;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Type extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Cleans the new lines.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The clean text.</returns>
        public static string CleanNewLines(this string text)
        {
            return (new Regex("[\r\n]+")).Replace(text, "\n");
        }

        /// <summary>
        /// Cleans the new lines.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The clean text.</returns>
        public static string CleanNewLines(this StringBuilder text)
        {
            return text.ToString().CleanNewLines();
        }

        /// <summary>
        /// Get the nets name.
        /// </summary>
        /// <param name="netInfo">The net information.</param>
        /// <returns>The name.</returns>
        public static string NetName(this NetInfo netInfo)
        {
            string name = netInfo.m_class.name;
            if (name == "Highway" && netInfo.m_lanes.Length == 1)
            {
                return "Highway Ramp";
            }
            else
            {
                return name;
            }
        }
    }
}