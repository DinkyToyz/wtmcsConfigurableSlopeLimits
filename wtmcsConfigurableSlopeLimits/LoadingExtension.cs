using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Mod stuff loader.
    /// </summary>
    public class LoadingExtension : LoadingExtensionBase
    {
        /// <summary>
        /// The original limits.
        /// </summary>
        private Dictionary<string, float> originalLimits = null;

        /// <summary>
        /// The mod is loaded.
        /// </summary>
        private bool isLoaded = false;

        /// <summary>
        /// The mod is broken.
        /// </summary>
        private bool isBroken = false;

        /// <summary>
        /// Initializes the slopes.
        /// </summary>
        /// <returns>True on success.</returns>
        protected void InitializeSlopes()
        {
            if (isBroken)
            {
                return;
            }

            try
            {
                Log.Info(this, "InitializeSlopes", "Begin");

                bool missing = false;

                ObjectIDGenerator idGen = new ObjectIDGenerator();
                bool firstTime;

                foreach (NetCollection netCollection in UnityEngine.Object.FindObjectsOfType<NetCollection>())
                {
                    Log.Info(this, "InitializeSlopes", "netCollection: " + netCollection.GetInstanceID().ToString() + ", " + netCollection.GetHashCode().ToString());

                    foreach (NetInfo netInfo in netCollection.m_prefabs)
                    {
                        long id = idGen.GetId(netInfo, out firstTime);
                        if (!firstTime)
                        {
                            continue;
                        }

                        if (!originalLimits.ContainsKey(netInfo.m_class.name))
                        {
                            originalLimits[netInfo.m_class.name] = netInfo.m_maxSlope;
                            Log.Info("OrgLimit: " + netInfo.m_class.name + "=" + netInfo.m_maxSlope.ToString());
                        }

                        bool found = false;

                        if (Settings.Current.SlopeLimits.ContainsKey(netInfo.m_class.name))
                        {
                            netInfo.m_maxSlope = Settings.Current.SlopeLimits[netInfo.m_class.name];
                            found = true;
                        }
                        else
                        {
                            string name = netInfo.m_class.name.ToLowerInvariant();
                            if (Settings.Current.SlopeLimitsGeneric.ContainsKey(name))
                            {
                                netInfo.m_maxSlope = Settings.Current.SlopeLimitsGeneric[name];
                                found = true;
                            }
                            else
                            {
                                foreach (string generic in Settings.Current.SlopeLimitsGeneric.Keys.ToList().OrderBy(sName => name.Length).Reverse())
                                {
                                    if (name.Contains(generic))
                                    {
                                        netInfo.m_maxSlope = Settings.Current.SlopeLimitsGeneric[generic];
                                        found = true;
                                        break;
                                    }
                                }
                            }
                        }

                        Log.Info("NewLimit: " + netInfo.m_class.name + "=" + netInfo.m_maxSlope.ToString());

                        if (!found)
                        {
                            Settings.Current.SlopeLimits[netInfo.m_class.name] = netInfo.m_maxSlope;
                            missing = true;
                        }
                    }
                }

                if (missing)
                {
                    Settings.Current.Update();
                    Settings.Current.Save();
                }

                isLoaded = true;

                Log.Info(this, "InitializeSlopes", "End");
            }
            catch (Exception ex)
            {
                isBroken = true;
                Log.Error(this, "InitializeSlopes", ex);
            }
        }

        /// <summary>
        /// Restores the limits.
        /// </summary>
        protected void RestoreLimits()
        {
            try
            {
                Log.Info(this, "RestoreLimits", "Begin");

                foreach (NetCollection netCollection in UnityEngine.Object.FindObjectsOfType<NetCollection>())
                {
                    foreach (NetInfo netInfo in netCollection.m_prefabs)
                    {
                        if (originalLimits.ContainsKey(netInfo.m_class.name))
                        {
                            netInfo.m_maxSlope = originalLimits[netInfo.m_class.name];
                            Log.Info("OldLimit: " + netInfo.m_class.name + "=" + netInfo.m_maxSlope.ToString());
                        }
                    }
                }

                Log.Info(this, "RestoreLimits", "End");
            }
            catch (Exception ex)
            {
                isBroken = true;
                Log.Error(this, "RestoreLimits", ex);
            }
            finally
            {
                isLoaded = false;
            }
        }

        /// <summary>
        /// Called when map (etc) is loaded.
        /// </summary>
        /// <param name="mode">The load mode.</param>
        public override void OnLevelLoaded(LoadMode mode)
        {
            Log.Info(this, "OnLevelLoaded");

            try
            {
                if (!isBroken)
                {
                    bool setOriginals = false;
                    if (originalLimits == null)
                    {
                        originalLimits = new Dictionary<string, float>();
                        setOriginals = true;
                    }

                    if (setOriginals || !isLoaded)
                    {
                        InitializeSlopes();
                    }
                }
            }
            catch(Exception ex) 
            {
                Log.Error(this, "OnLevelLoaded", ex);
            }

            Log.Info(this, "OnLevelLoaded", "Base");
            base.OnLevelLoaded(mode);
        }

        /// <summary>
        /// Called when map (etc) unloads.
        /// </summary>
        public override void OnLevelUnloading()
        {
            Log.Info(this, "OnLevelUnloading");
            if (isLoaded)
            {
                RestoreLimits();
            }

            Log.Info(this, "OnLevelUnloading", "Base");
            base.OnLevelUnloading();
        }
    }
}