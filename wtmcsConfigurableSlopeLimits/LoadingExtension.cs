using ICities;
using System;
using System.Collections.Generic;
using System.Linq;

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
        protected void InitializeLimits()
        {
            if (isBroken || originalLimits != null)
            {
                return;
            }

            try
            {
                Log.Info(this, "InitializeLimits", "Begin");

                originalLimits = new Dictionary<string, float>();

                bool missing = false;

                foreach (NetCollection netCollection in UnityEngine.Object.FindObjectsOfType<NetCollection>())
                {
                    foreach (NetInfo netInfo in netCollection.m_prefabs)
                    {
                        string netName = netInfo.NetName();

                        if (Settings.IgnoreNet(netName))
                        {
                            Log.Info("NotLimit: " + netName + "=" + netInfo.m_maxSlope.ToString());
                            continue;
                        }

                        if (Settings.GenericNames.Contains(netName))
                        {
                            if (!Settings.Current.SlopeLimits.ContainsKey(netName))
                            {
                                Log.Info("NewLimit: " + netName + "=" + netInfo.m_maxSlope.ToString());
                                Settings.Current.SlopeLimits[netName] = netInfo.m_maxSlope;
                                missing = true;
                            }
                        }

                        if (!originalLimits.ContainsKey(netName))
                        {
                            Log.Info("OrgLimit: " + netName + "=" + netInfo.m_maxSlope.ToString());
                            originalLimits[netName] = netInfo.m_maxSlope;
                        }
                    }
                }

                if (missing)
                {
                    Settings.Current.Update();
                    Settings.Current.Save();
                }

                Log.Info(this, "InitializeLimits", "End");
            }
            catch (Exception ex)
            {
                isBroken = true;
                Log.Error(this, "InitializeLimits", ex);
            }
        }

        /// <summary>
        /// Sets the limits.
        /// </summary>
        protected void SetLimits()
        {
            if (isBroken)
            {
                return;
            }

            try
            {
                Log.Info(this, "SetLimits", "Begin");

                bool missing = false;

                foreach (NetCollection netCollection in UnityEngine.Object.FindObjectsOfType<NetCollection>())
                {
                    foreach (NetInfo netInfo in netCollection.m_prefabs)
                    {
                        string netName = netInfo.NetName();

                        if (Settings.IgnoreNet(netName))
                        {
                            Log.Info("NotLimit: " + netName + "=" + netInfo.m_maxSlope.ToString());
                            continue;
                        }

                        string found = null;

                        if (Settings.Current.SlopeLimits.ContainsKey(netName))
                        {
                            netInfo.m_maxSlope = Settings.Current.SlopeLimits[netName];
                            found = "name";
                        }
                        else
                        {
                            string name = netName.ToLowerInvariant();
                            if (Settings.Current.SlopeLimitsGeneric.ContainsKey(name))
                            {
                                netInfo.m_maxSlope = Settings.Current.SlopeLimitsGeneric[name];
                                found = "generic";
                            }
                            else
                            {
                                foreach (string generic in Settings.Current.SlopeLimitsGeneric.Keys.ToList().OrderBy(sName => name.Length).Reverse())
                                {
                                    if (name.Contains(generic))
                                    {
                                        netInfo.m_maxSlope = Settings.Current.SlopeLimitsGeneric[generic];
                                        found = "part";
                                        break;
                                    }
                                }
                            }
                        }

                        if (found == null)
                        {
                            Log.Info("NewLimit: " + netName + "=" + netInfo.m_maxSlope.ToString());
                            Settings.Current.SlopeLimits[netName] = netInfo.m_maxSlope;
                            missing = true;
                        }
                        else
                        {
                            Log.Info("SetLimit: " + netName + "=" + netInfo.m_maxSlope.ToString() + " (" + found + ")");
                        }
                    }
                }

                if (missing)
                {
                    Settings.Current.Update();
                    Settings.Current.Save();
                }

                isLoaded = true;

                Log.Info(this, "SetLimits", "End");
            }
            catch (Exception ex)
            {
                isBroken = true;
                Log.Error(this, "SetLimits", ex);
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
                        string netName = netInfo.NetName();

                        if (Settings.IgnoreNet(netName))
                        {
                            Log.Info("NotLimit: " + netName + "=" + netInfo.m_maxSlope.ToString());
                            continue;
                        }

                        if (originalLimits.ContainsKey(netName))
                        {
                            netInfo.m_maxSlope = originalLimits[netName];
                            Log.Info("OldLimit: " + netName + "=" + netInfo.m_maxSlope.ToString());
                        }
                        else
                        {
                            Log.Info("NonLimit: " + netName + "=" + netInfo.m_maxSlope.ToString());
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
                InitializeLimits();

                if (!isBroken && !isLoaded)
                {
                    SetLimits();
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnLevelLoaded", ex);
                isBroken = true;
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