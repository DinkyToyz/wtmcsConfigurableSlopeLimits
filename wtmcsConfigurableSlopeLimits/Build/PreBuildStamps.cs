using System;

namespace AssemblyInfo
{
    /// <summary>
    /// Build stamps.
    /// </summary>
    public static class PreBuildStamps
    {
        /// <summary>
        /// Build-stamped ticks.
        /// </summary>
        private static long ticks = 636800779073969774; /*:TICKS:*/

        /// <summary>
        /// Gets build-stamped date-time.
        /// </summary>
        /// <value>
        /// The date-time.
        /// </value>
        public static DateTime DateTime
        {
            get
            {
                return new DateTime(ticks);
            }
        }

        /// <summary>
        /// Gets build-stamped year.
        /// </summary>
        /// <value>
        /// The year.
        /// </value>
        public static int Year
        {
            get
            {
                return PreBuildStamps.DateTime.Year;
            }
        }
    }
}