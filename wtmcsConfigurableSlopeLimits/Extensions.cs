using System;
using System.Reflection;
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
        /// Get only ASCII capitals.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The ASCII capitals.</returns>
        public static string ASCIICapitals(this string text)
        {
            return Regex.Replace(text, "[^A-Z]", "");
        }

        /// <summary>
        /// Invokes method in base class.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The return object.</returns>
        public static object BaseInvoke(this object instance, string methodName, object[] parameters)
        {
            try
            {
                Type baseType = instance.GetType().BaseType;

                if (baseType == null)
                {
                    return null;
                }

                MethodInfo methodInfo = baseType.GetMethod(methodName);
                if (methodInfo == null)
                {
                    return null;
                }

                return methodInfo.Invoke(instance, parameters);
            }
            catch (Exception ex)
            {
                Log.Error(instance, "BaseInvoke", ex, methodName);
                return null;
            }
        }

        /// <summary>
        /// Cleans the newlines.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The clean text.</returns>
        public static string CleanNewLines(this string text)
        {
            return Regex.Replace(text, "[\r\n]+", "\n");
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
        /// Compacts the name. "AppleSauce" becomes "ApSa".
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string CompactName(this string text)
        {
            StringBuilder compact = new StringBuilder();

            bool wuc = false;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] >= 'A' && text[i] <= 'Z')
                {
                    compact.Append(text[i]);
                    wuc = true;
                }
                else if (text[i] >= 'a' && text[i] <= 'z')
                {
                    if (wuc)
                    {
                        compact.Append(text[i]);
                        wuc = false;
                    }
                }
                else
                {
                    compact.Append(text[i]);
                    wuc = false;
                }
            }

            return compact.ToString();
        }

        /// <summary>
        /// Conforms the newlines to the environment.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The comforming text.</returns>
        public static string ConformNewlines(this string text)
        {
            return Regex.Replace(text, "[\r\n]+", Environment.NewLine);
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

            if (name == "Highway" && netInfo.name.Contains("Ramp"))
            {
                return "Highway Ramp";
            }

            if (name == "Highway Tunnel" && netInfo.name.Contains("Ramp"))
            {
                return "Highway Ramp Tunnel";
            }

            /*
             * TODO: Compatibility with Network Extensions
             *
             * netInfo; Small Road; True; True; 6; Small Avenue; Small Avenue (NetInfo); Small Four-Lane Road
             * netInfo; NExtMediumAvenue; True; True; 8; Medium Avenue; Medium Avenue (NetInfo); Four-Lane Road
             * netInfo; NExtMediumAvenue; True; True; 10; Medium Avenue TL; Medium Avenue TL (NetInfo); Four-Lane Road with Turning Lane
             * netInfo; NExtHighway; True; True; 6; Small Rural Highway; Small Rural Highway (NetInfo); Rural Highway
             * netInfo; NExtHighway; True; True; 6; Rural Highway; Rural Highway (NetInfo); Two-Lane Highway
             * netInfo; Small Road; True; True; 4; Rural Highway Elevated; Rural Highway Elevated (NetInfo); NET_TITLE[Rural Highway Elevated]:0
             * netInfo; Small Road; True; True; 4; Rural Highway Bridge; Rural Highway Bridge (NetInfo); NET_TITLE[Rural Highway Bridge]:0
             * netInfo; Small Road Tunnel; True; True; 4; Rural Highway Tunnel; Rural Highway Tunnel (NetInfo); NET_TITLE[Rural Highway Tunnel]:0
             * netInfo; Small Road; True; True; 4; Rural Highway Slope; Rural Highway Slope (NetInfo); NET_TITLE[Rural Highway Slope]:0
             * netInfo; NExtHighway; True; True; 10; Large Highway; Large Highway (NetInfo); Six-Lane Highway
             * netInfo; Large Road; True; True; 8; Large Highway Elevated; Large Highway Elevated (NetInfo); NET_TITLE[Large Highway Elevated]:0
             * netInfo; Large Road; True; True; 8; Large Highway Bridge; Large Highway Bridge (NetInfo); NET_TITLE[Large Highway Bridge]:0
             * netInfo; Large Road Tunnel; True; True; 8; Large Highway Tunnel; Large Highway Tunnel (NetInfo); NET_TITLE[Large Highway Tunnel]:0
             * netInfo; Large Road; True; True; 8; Large Highway Slope; Large Highway Slope (NetInfo); NET_TITLE[Large Highway Slope]:0
             *
             */

            return name;
        }
    }
}