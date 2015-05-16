﻿using ICities;
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
        /// The mod is broken.
        /// </summary>
        private bool isBroken = false;

        /// <summary>
        /// The mod is initialized.
        /// </summary>
        private bool isInitialized = false;

        /// <summary>
        /// The mod is loaded.
        /// </summary>
        private bool isLoaded = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingExtension"/> class.
        /// </summary>
        public LoadingExtension()
            : base()
        {
            Log.Debug(this, "Constructing");
        }

        /// <summary>
        /// Called when mod is created.
        /// </summary>
        /// <param name="loading">The loading.</param>
        public override void OnCreated(ILoading loading)
        {
            Log.Debug(this, "OnCreated", "Begin");

            try
            {
                if (!isBroken && !isLoaded)
                {
                    InitializeLimits();
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnCreated", ex);
                isBroken = true;
            }

            Log.Debug(this, "OnCreated", "Base");
            base.OnCreated(loading);

            Log.Debug(this, "OnCreated", "End");
        }

        /// <summary>
        /// Called when map (etc) is loaded.
        /// </summary>
        /// <param name="mode">The load mode.</param>
        public override void OnLevelLoaded(LoadMode mode)
        {
            Log.Debug(this, "OnLevelLoaded", "Begin");

            try
            {
                if (!isBroken && !isLoaded)
                {
                    InitializeLimits();
                    SetLimits();
                }

                //Global.ReInitializeToolButton();
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnLevelLoaded", ex);
                isBroken = true;
            }

            Log.Debug(this, "OnLevelLoaded", "Base");
            base.OnLevelLoaded(mode);

            Log.Debug(this, "OnLevelLoaded", "End");
        }

        /// <summary>
        /// Called when map (etc) unloads.
        /// </summary>
        public override void OnLevelUnloading()
        {
            Log.Debug(this, "OnLevelUnloading", "Begin");

            try
            {
                Global.DeInitializeToolButton();
                Global.DeInitializeUI();
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnLevelUnloading", ex);
                isBroken = true;
            }

            try
            {
                RestoreLimits();
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnLevelUnloading", ex);
                isBroken = true;
            }

            Log.Debug(this, "OnLevelUnloading", "Base");
            base.OnLevelUnloading();

            Log.Debug(this, "OnLevelUnloading", "End");
        }

        /// <summary>
        /// Called when mod is released.
        /// </summary>
        public override void OnReleased()
        {
            Log.Debug(this, "OnReleased", "Begin");

            try
            {
                Global.DisposeToolButton();
                Global.DisposeUI();
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnLevelUnloading", ex);
                isBroken = true;
            }

            try
            {
                RestoreLimits();
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnReleased", ex);
                isBroken = true;
            }

            Log.Debug(this, "OnReleased", "Base");
            base.OnReleased();

            Log.Debug(this, "OnReleased", "End");
        }

        /// <summary>
        /// Initializes the slopes.
        /// </summary>
        /// <returns>True on success.</returns>
        protected void InitializeLimits()
        {
            if (isBroken || isInitialized)
            {
                return;
            }

            Log.Debug(this, "InitializeLimits", "Begin");

            try
            {
                bool missing = false;
                bool found = false;

                Global.Settings.SlopeLimitsOriginal.Clear();
                Global.Settings.SlopeLimitsIgnored.Clear();

                foreach (NetCollection netCollection in UnityEngine.Object.FindObjectsOfType<NetCollection>())
                {
                    foreach (NetInfo netInfo in netCollection.m_prefabs)
                    {
                        string netName = netInfo.NetName();

                        if (Settings.IgnoreNet(netName))
                        {
                            if (!Global.Settings.SlopeLimitsIgnored.ContainsKey(netName))
                            {
                                Log.Info("NotLimit", netName, netInfo.m_maxSlope);
                                Global.Settings.SlopeLimitsIgnored[netName] = netInfo.m_maxSlope;
                            }

                            continue;
                        }

                        if (Settings.GenericNames.Contains(netName))
                        {
                            if (!Global.Settings.SlopeLimits.ContainsKey(netName))
                            {
                                Log.Info("NewLimit", netName, netInfo.m_maxSlope);
                                Global.Settings.SlopeLimits[netName] = netInfo.m_maxSlope;
                                missing = true;
                            }
                        }

                        if (!Global.Settings.SlopeLimitsOriginal.ContainsKey(netName))
                        {
                            Log.Info("OrgLimit", netName, netInfo.m_maxSlope);
                            Global.Settings.SlopeLimitsOriginal[netName] = netInfo.m_maxSlope;
                            found = true;
                        }
                    }
                }

                if (Global.Settings.SlopeLimitsOriginal.Count > 0)
                {
                    if (missing || found)
                    {
                        if (missing)
                        {
                            Global.Settings.Update();
                        }

                        Global.Settings.Save();
                    }

                    isInitialized = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "InitializeLimits", ex);
                isBroken = true;
            }

            Log.Debug(this, "InitializeLimits", "End");
        }

        /// <summary>
        /// Restores the limits.
        /// </summary>
        protected void RestoreLimits()
        {
            if (!isLoaded || !isInitialized)
            {
                return;
            }

            Log.Debug(this, "RestoreLimits", "Begin");

            try
            {
                foreach (NetCollection netCollection in UnityEngine.Object.FindObjectsOfType<NetCollection>())
                {
                    foreach (NetInfo netInfo in netCollection.m_prefabs)
                    {
                        string netName = netInfo.NetName();

                        if (Settings.IgnoreNet(netName))
                        {
                            if (!Global.Settings.SlopeLimitsIgnored.ContainsKey(netName))
                            {
                                Log.Info("NotLimit", netName, netInfo.m_maxSlope);
                                Global.Settings.SlopeLimitsIgnored[netName] = netInfo.m_maxSlope;
                            }

                            continue;
                        }

                        if (Global.Settings.SlopeLimitsOriginal.ContainsKey(netName))
                        {
                            netInfo.m_maxSlope = Global.Settings.SlopeLimitsOriginal[netName];
                            Log.Info("OldLimit", netName, netInfo.m_maxSlope);
                        }
                        else
                        {
                            Log.Info("NonLimit", netName, netInfo.m_maxSlope);
                        }
                    }
                }
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

            Log.Debug(this, "RestoreLimits", "End");
        }

        /// <summary>
        /// Sets the limits.
        /// </summary>
        protected void SetLimits()
        {
            if (isBroken || !isInitialized)
            {
                return;
            }

            Log.Debug(this, "SetLimits", "Begin");

            try
            {
                bool missing = false;

                foreach (NetCollection netCollection in UnityEngine.Object.FindObjectsOfType<NetCollection>())
                {
                    foreach (NetInfo netInfo in netCollection.m_prefabs)
                    {
                        string netName = netInfo.NetName();

                        if (Settings.IgnoreNet(netName))
                        {
                            if (!Global.Settings.SlopeLimitsIgnored.ContainsKey(netName))
                            {
                                Log.Info("NotLimit", netName, netInfo.m_maxSlope);
                                Global.Settings.SlopeLimitsIgnored[netName] = netInfo.m_maxSlope;
                            }

                            continue;
                        }

                        string found = null;

                        if (Global.Settings.SlopeLimits.ContainsKey(netName))
                        {
                            netInfo.m_maxSlope = Global.Settings.SlopeLimits[netName];
                            found = "name";
                        }
                        else
                        {
                            string name = netName.ToLowerInvariant();
                            if (Global.Settings.SlopeLimitsGeneric.ContainsKey(name))
                            {
                                netInfo.m_maxSlope = Global.Settings.SlopeLimitsGeneric[name];
                                found = "generic";
                            }
                            else
                            {
                                foreach (string generic in Global.Settings.SlopeLimitsGeneric.Keys.ToList().OrderBy(sName => name.Length).Reverse())
                                {
                                    if (name.Contains(generic))
                                    {
                                        netInfo.m_maxSlope = Global.Settings.SlopeLimitsGeneric[generic];
                                        found = "part";
                                        break;
                                    }
                                }
                            }
                        }

                        if (found == null || found != "name")
                        {
                            if (found == null)
                            {
                                Log.Info("NewLimit", netName, netInfo.m_maxSlope);
                            }
                            else
                            {
                                Log.Info("SetLimit", netName, netInfo.m_maxSlope, found);
                            }
                            Global.Settings.SlopeLimits[netName] = netInfo.m_maxSlope;
                            missing = true;
                        }
                        else
                        {
                            Log.Info("SetLimit", netName, netInfo.m_maxSlope);
                        }
                    }
                }

                if (missing)
                {
                    Global.Settings.Update();
                    Global.Settings.Save();
                }

                isLoaded = true;
            }
            catch (Exception ex)
            {
                isBroken = true;
                Log.Error(this, "SetLimits", ex);
            }

            Log.Debug(this, "SetLimits", "End");
        }
    }
}