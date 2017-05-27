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
        /// Compacts the name.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>A compacted name.</returns>
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
        /// <returns>The conforming text.</returns>
        public static string ConformNewlines(this string text)
        {
            return Regex.Replace(text, "[\r\n]+", Environment.NewLine);
        }

        /// <summary>
        /// Conforms the newlines to the environment.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The conforming text.</returns>
        public static string ConformNewlines(this StringBuilder text)
        {
            return text.ToString().ConformNewlines();
        }

        /// <summary>
        /// Retrieves a substring from this instance, containing the left-most characters.
        /// </summary>
        /// <param name="text">The string instance.</param>
        /// <param name="maxLength">The maximum number of characters in the substring.</param>
        /// <returns>A String object equivalent to the left-most characters.</returns>
        public static string SafeLeftString(this string text, int maxLength)
        {
            return text.SafeSubstring(0, maxLength);
        }

        /// <summary>
        /// Retrieves a substring from this instance, containing the right-most characters.
        /// </summary>
        /// <param name="text">The string instance.</param>
        /// <param name="maxLength">The maximum number of characters in the substring.</param>
        /// <returns>A String object equivalent to the right-most characters.</returns>
        public static string SafeRightString(this string text, int maxLength)
        {
            if (text == null)
            {
                return null;
            }
            else if (text.Length == 0)
            {
                return String.Empty;
            }
            else if (maxLength >= text.Length)
            {
                return text.Substring(0, text.Length);
            }
            else
            {
                return text.Substring(text.Length - maxLength);
            }
        }

        /// <summary>
        /// Retrieves a substring from this instance. The substring starts at a specified character position.
        /// </summary>
        /// <param name="text">The string instance.</param>
        /// <param name="startIndex">The zero-based starting character position of a substring in this instance.</param>
        /// <returns>A String object equivalent to the substring that begins at startIndex in this instance, or Empty if startIndex is out of range.</returns>
        public static string SafeSubstring(this string text, int startIndex)
        {
            if (text == null)
            {
                return null;
            }
            else if (startIndex >= text.Length)
            {
                return String.Empty;
            }
            else
            {
                if (startIndex < 0)
                {
                    startIndex = 0;
                }

                return text.Substring(startIndex);
            }
        }

        /// <summary>
        /// Retrieves a substring from this instance. The substring starts at a specified character position and has a specified maximum length.
        /// </summary>
        /// <param name="text">The string instance.</param>
        /// <param name="startIndex">The zero-based starting character position of a substring in this instance.</param>
        /// <param name="maxLength">The maximum number of characters in the substring.</param>
        /// <returns>A String equivalent to a substring of maximum length length that begins at startIndex in this instance, or Empty if startIndex is out of range.</returns>
        public static string SafeSubstring(this string text, int startIndex, int maxLength)
        {
            if (text == null)
            {
                return null;
            }
            else if (startIndex >= text.Length || maxLength <= 0)
            {
                return String.Empty;
            }
            else
            {
                if (startIndex < 0)
                {
                    startIndex = 0;
                }

                if (text.Length - startIndex < maxLength)
                {
                    return text.Substring(startIndex);
                }
                else
                {
                    return text.Substring(startIndex, maxLength);
                }
            }
        }
    }
}