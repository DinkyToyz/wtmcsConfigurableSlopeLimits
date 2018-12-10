using System.Reflection;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Mod info.
    /// </summary>
    internal static class Library
    {
        /// <summary>
        /// The description.
        /// </summary>
        public const string Description = "Allows slope limits to be configured separately for different network types.";

        /// <summary>
        /// The name.
        /// </summary>
        public const string Name = "wtmcsConfigurableSlopeLimits";

        /// <summary>
        /// The title.
        /// </summary>
        public const string Title = "Slope Limits (WtM)";

        /// <summary>
        /// Gets the build.
        /// </summary>
        /// <value>
        /// The build.
        /// </value>
        public static string Build
        {
            get
            {
                try
                {
                    AssemblyName name = Assembly.GetExecutingAssembly().GetName();
                    return name.Name + " " + name.Version.ToString() + " (" + AssemblyInfo.PreBuildStamps.DateTime.ToString("yyyy-MM-dd HH:mm") + ")";
                }
                catch
                {
                    try
                    {
                        return Name + " (" + AssemblyInfo.PreBuildStamps.DateTime.ToString("yyyy-MM-dd HH:mm") + ")";
                    }
                    catch
                    {
                        return Name;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this is a debug build.
        /// </summary>
        /// <value>
        /// <c>true</c> if this is a debug build; otherwise, <c>false</c>.
        /// </value>
        public static bool IsDebugBuild
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
    }
}