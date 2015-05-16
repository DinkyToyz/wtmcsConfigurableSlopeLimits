using System.Text;
using System.Text.RegularExpressions;
using System;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Type extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Cleans the newlines.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The clean text.</returns>
        public static string CleanNewLines(this string text)
        {
            return (new Regex("[\r\n]+")).Replace(text, "\n");
        }

        /// <summary>
        /// Cleans the newlines.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The clean text.</returns>
        public static string CleanNewLines(this StringBuilder text)
        {
            return text.ToString().CleanNewLines();
        }

        /// <summary>
        /// Conforms the newlines to the environment.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The comforming text.</returns>
        public static string ConformNewlines(this string text)
        {
            return (new Regex("[\r\n]+")).Replace(text, Environment.NewLine);
        }

        /// <summary>
        /// Conforms the newlines to the environment.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The comforming text.</returns>
        public static string ConformNewlines(this StringBuilder text)
        {
            return text.ToString().ConformNewlines();
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