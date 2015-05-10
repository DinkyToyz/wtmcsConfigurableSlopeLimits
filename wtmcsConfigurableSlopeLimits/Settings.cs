using ColossalFramework.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Mod settings.
    /// </summary>
    [DataContract]
    public class Settings
    {
        /// <summary>
        /// The save count.
        /// </summary>
        [DataMember]
        public uint SaveCount = 0;

        /// <summary>
        /// The slope limits.
        /// </summary>
        [DataMember]
        public Dictionary<string, float> SlopeLimits = new Dictionary<string, float>();

        /// <summary>
        /// The generic slope limits.
        /// </summary>
        [NonSerialized]
        public Dictionary<string, float> SlopeLimitsGeneric = new Dictionary<string, float>();

        /// <summary>
        /// The current settings.
        /// </summary>
        private static Settings currentSettings;

        /// <summary>
        /// Gets the current settings.
        /// </summary>
        /// <value>
        /// The current settings.
        /// </value>
        public static Settings Current
        {
            get
            {
                if (currentSettings == null)
                {
                    currentSettings = Load();
                    foreach (string name in currentSettings.SlopeLimits.Keys)
                    {
                        Log.Info("CfgLimit: " + name + "=" + currentSettings.SlopeLimits[name].ToString());
                    }
                    foreach (string name in currentSettings.SlopeLimits.Keys)
                    {
                        Log.Info("GenLimit: " + name + "=" + currentSettings.SlopeLimitsGeneric[name].ToString());
                    }
                }

                return currentSettings;
            }
        }

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
        public static string FileName
        {
            get
            {
                return Assembly.Name + ".xml";
            }
        }

        /// <summary>
        /// Gets the complete path.
        /// </summary>
        /// <value>
        /// The complete path.
        /// </value>
        public static string FilePathName
        {
            get
            {
                return Path.GetFullPath(Path.Combine(FilePath, FileName));
            }
        }

        /// <summary>
        /// Initializes a generic limit.
        /// </summary>
        /// <param name="name">The road type name.</param>
        /// <param name="generic">The generic name.</param>
        private void InitGeneric(string name, string generic)
        {
            try
            {
                if (!SlopeLimitsGeneric.ContainsKey(generic))
                {
                    if (SlopeLimits.ContainsKey(name))
                    {
                        SlopeLimits[generic] = SlopeLimits[name];
                    }
                    else
                    {
                        string lName = name.ToLowerInvariant();
                        if (SlopeLimitsGeneric.ContainsKey(lName))
                        {
                            SlopeLimitsGeneric[generic] = SlopeLimitsGeneric[lName];
                        }
                        else
                        {
                            foreach (string sName in SlopeLimits.Keys)
                            {
                                if (sName.ToLowerInvariant().Contains(generic))
                                {
                                    SlopeLimitsGeneric[generic] = SlopeLimits[sName];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "InitGeneric", ex);
            }
        }

        /// <summary>
        /// Initializes the generic limits.
        /// </summary>
        private void initGenerics()
        {
            try
            {
                foreach (string name in SlopeLimits.Keys)
                {
                    string lName = name.ToLowerInvariant();
                    if (!SlopeLimitsGeneric.ContainsKey(lName))
                    {
                        SlopeLimitsGeneric[lName] = SlopeLimits[name];
                    }
                }

                InitGeneric("Highway", "high");
                InitGeneric("LargeRoad", "large");
                InitGeneric("MediumRoad", "medium");
                InitGeneric("SmallRoad", "small");
                InitGeneric("TrainTrack", "track");
                InitGeneric("TrainTrack", "rail");
            }
            catch (Exception ex)
            {
                Log.Error(this, "InitGenerics", ex);
            }
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public void Update()
        {
            initGenerics();
        }

        /// <summary>
        /// Loads settings from the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The settings.</returns>
        public static Settings Load(string fileName = null)
        {
            try
            {
                Log.Info(typeof(Settings), "Load", "Begin");
                
                if (fileName == null)
                {
                    fileName = FilePathName;
                }

                if (File.Exists(fileName))
                {
                    Log.Info(typeof(Settings), "Load", fileName);

                    using (FileStream file = File.OpenRead(fileName))
                    {
                        DataContractSerializer ser = new DataContractSerializer(typeof(Settings));
                        Settings sets = ser.ReadObject(file) as Settings;

                        if (sets != null)
                        {
                            sets.initGenerics();
                            sets.SaveCount = 0;

                            Log.Info(typeof(Settings), "Load", "End");
                            return sets;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Settings), "Load", ex);
            }

            return new Settings();
        }

        /// <summary>
        /// Saves settings to the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public void Save(string fileName = null)
        {
            try
            {
                Log.Info(this, "Save", "Begin");

                if (fileName == null)
                {
                    fileName = FilePathName;
                }

                string filePath = Path.GetDirectoryName(Path.GetFullPath(fileName));
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                Log.Info(this, "Save", fileName);

                SaveCount++;
                using (FileStream file = File.Create(fileName))
                {
                    DataContractSerializer ser = new DataContractSerializer(typeof(Settings));
                    ser.WriteObject(file, this);
                    file.Flush();
                    file.Close();
                }

                Log.Info(this, "Save", "End");
            }
            catch (Exception ex)
            {
                Log.Error(this, "Save", ex);
            }
        }
    }
}