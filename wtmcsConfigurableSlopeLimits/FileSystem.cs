using ColossalFramework.IO;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// File system helper.
    /// </summary>
    internal static class FileSystem
    {
        /// <summary>
        /// Gets the file path.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        public static string FilePath
        {
            get
            {
                return Path.Combine(DataLocation.localApplicationData, "ModConfig");
            }
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public static string FileName(string extension = "")
        {
                return Library.Name + extension;
        }

        /// <summary>
        /// Gets the complete path.
        /// </summary>
        /// <value>
        /// The complete path.
        /// </value>
        public static string FilePathName(string fileName = null)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                fileName = FileName(".tmp");
            }
            else if (fileName[0] == '.')
            {
                fileName = FileName(fileName);
            }
               
            return Path.GetFullPath(Path.Combine(FilePath, fileName));
        }

        public static bool Exists(string fileName = null)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                fileName = FileName(".tmp");
            }
            else if (fileName[0] == '.')
            {
                fileName = FileName(fileName);
            }

            return File.Exists(fileName);
        }
    }
}
