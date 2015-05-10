using System;
using System.Reflection;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Assembly info helper.
    /// </summary>
    public static class Assembly
    {
        /// <summary>
        /// Try to get main application assembly.
        /// </summary>
        /// <returns>Found assembly.</returns>
        private static System.Reflection.Assembly RunningAssembly
        {
            get
            {
                System.Reflection.Assembly a = null;

                try
                {
                    a = System.Reflection.Assembly.GetEntryAssembly();
                    if (a != null)
                        return a;
                }
                catch { }

                try
                {
                    a = System.Reflection.Assembly.GetCallingAssembly();
                    if (a != null)
                        return a;
                }
                catch { }

                try
                {
                    a = System.Reflection.Assembly.GetExecutingAssembly();
                    if (a != null)
                        return a;
                }
                catch { }

                return null;
            }
        }

        /// <summary>
        /// Gets the attribute.
        /// </summary>
        /// <param name="type">The attribute type.</param>
        /// <returns>The attribute object.</returns>
        public static object GetAttribute(Type type)
        {
            try
            {
                object[] a = RunningAssembly.GetCustomAttributes(type, false);
                return a[0];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public static string Name
        {
            get
            {
                try
                {
                    return RunningAssembly.GetName().Name;
                }
                catch
                {
                    return "wtmcsConfigurableSlopeLimits";
                }
            }
        }

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public static string Title
        {
            get
            {
                try
                {
                    AssemblyTitleAttribute a = GetAttribute(typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute;
                    return a.Title;
                }
                catch
                {
                    return "Configurable Slope Limits";
                }
            }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public static string Description
        {
            get
            {
                try
                {
                    AssemblyDescriptionAttribute a = GetAttribute(typeof(AssemblyDescriptionAttribute)) as AssemblyDescriptionAttribute;
                    return a.Description;
                }
                catch (Exception)
                {
                    return "Allows  slope limits to be configured separately for different network types.";
                }
            }
        }
    }
}