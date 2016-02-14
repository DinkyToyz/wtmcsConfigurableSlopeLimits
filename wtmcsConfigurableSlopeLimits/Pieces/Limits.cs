using System;
using System.Linq;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Handles the slope limits.
    /// </summary>
    internal class Limits
    {
        /// <summary>
        /// The current selected group.
        /// </summary>
        private Groups group = Groups.Original;

        /// <summary>
        /// The mod is broken.
        /// </summary>
        private bool isBroken = false;

        /// <summary>
        /// The mod is initialized.
        /// </summary>
        private bool isInitialized = false;

        /// <summary>
        /// Limit groups.
        /// </summary>
        public enum Groups
        {
            /// <summary>
            /// The original limits.
            /// </summary>
            Original = 1,

            /// <summary>
            /// The custom limits.
            /// </summary>
            Custom = 2,

            /// <summary>
            /// The relaxed limits.
            /// </summary>
            Disabled = 3
        }

        /// <summary>
        /// Gets or sets the current selected group.
        /// </summary>
        /// <value>
        /// The current selected group.
        /// </value>
        public Groups Group
        {
            get
            {
                return this.group;
            }

            set
            {
                if (value == Groups.Original)
                {
                    this.RestoreLimits();
                }
                else
                {
                    this.SetLimits(value);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Limits"/> is initialized.
        /// </summary>
        /// <value>
        ///   <c>True</c> if initialized; otherwise, <c>false</c>.
        /// </value>
        public bool Initialized
        {
            get
            {
                return this.isInitialized;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is usable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is usable; otherwise, <c>false</c>.
        /// </value>
        public bool IsUsable
        {
            get
            {
                return this.isInitialized && !this.isBroken;
            }
        }

        /// <summary>
        /// Initializes the slopes.
        /// </summary>
        public void Initialize()
        {
            bool locked = false;
            try
            {
                locked = Global.Lock();

                if (this.isBroken || this.isInitialized)
                {
                    return;
                }

                Log.Debug(this, "Initialize", "Begin");

                try
                {
                    bool missing = false;
                    bool found = false;

                    Global.Settings.SlopeLimitsOriginal.Clear();
                    Global.Settings.SlopeLimitsIgnored.Clear();

                    foreach (NetCollection netCollection in UnityEngine.Object.FindObjectsOfType<NetCollection>())
                    {
                        if (netCollection == null)
                        {
                            Log.Warning(this, "Initialize", "Null NetCollection");
                            continue;
                        }

                        if (Settings.IgnoreNetCollection(netCollection.name))
                        {
                            continue;
                        }

                        if (netCollection.m_prefabs == null)
                        {
                            Log.Warning(this, "Initialize", "Null NetCollection.m_prefabs", netCollection);
                            continue;
                        }

                        foreach (NetInfo netInfo in netCollection.m_prefabs)
                        {
                            if (netInfo == null)
                            {
                                Log.Warning(this, "Initialize", "Null NetInfo", netCollection, netCollection.m_prefabs);
                                continue;
                            }

                            if (Log.LogToFile && Log.LogALot)
                            {
                                this.LogNetInfo(this, "Initialize", netInfo);
                            }

                            string netName = netInfo.NetName();

                            if (Settings.IgnoreNet(netName))
                            {
                                if (!Global.Settings.SlopeLimitsIgnored.ContainsKey(netName))
                                {
                                    if (Settings.WarnAboutIgnoredNet(netCollection.name, netName))
                                    {
                                        Log.Warning(null, null, "NotLimit", netName, netInfo.m_maxSlope);
                                    }
                                    else
                                    {
                                        Log.Info(null, null, "NotLimit", netName, netInfo.m_maxSlope);
                                    }

                                    Global.Settings.SlopeLimitsIgnored[netName] = netInfo.m_maxSlope;
                                    found = true;
                                }

                                continue;
                            }

                            if (!Global.Settings.SlopeLimits.ContainsKey(netName))
                            {
                                if (Settings.GenericNames.Contains(netName))
                                {
                                    Log.Info(null, null, "NewLimit", netName, netInfo.m_maxSlope);
                                    Global.Settings.SlopeLimits[netName] = netInfo.m_maxSlope;
                                    missing = true;
                                }
                                else
                                {
                                    float fallBack = Global.Settings.GetFallBackLimit(netName);
                                    if (fallBack != float.NaN)
                                    {
                                        Log.Info(null, null, "NewLimit", netName, fallBack);
                                        Global.Settings.SlopeLimits[netName] = fallBack;
                                        missing = true;
                                    }
                                }
                            }

                            if (!Global.Settings.SlopeLimitsOriginal.ContainsKey(netName))
                            {
                                Log.Info(null, null, "OrgLimit", netName, netInfo.m_maxSlope);
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

                        this.isInitialized = true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(this, "Initialize", ex);
                    this.isBroken = true;
                }
                finally
                {
                    if (Log.LogToFile)
                    {
                        this.LogNetNames();
                    }
                }

                Log.Debug(this, "Initialize", "End");
            }
            catch (Exception ex)
            {
                Log.Error(this, "Initialize", ex);
                this.isBroken = true;
            }
            finally
            {
                Global.Release(locked);
            }
        }

        /// <summary>
        /// Logs the net names.
        /// </summary>
        public void LogNetNames()
        {
            try
            {
                foreach (NetCollection netCollection in UnityEngine.Object.FindObjectsOfType<NetCollection>())
                {
                    if (netCollection != null)
                    {
                        Log.Debug(this, "LogNetNames", "NetCollection", netCollection, netCollection.name, Settings.IgnoreNetCollectionText(netCollection.name));
                        if (netCollection.m_prefabs != null)
                        {
                            Log.Debug(this, "LogNetNames", "Prefabs", netCollection.m_prefabs);
                            foreach (NetInfo netInfo in netCollection.m_prefabs)
                            {
                                if (netInfo != null)
                                {
                                    String netName = Settings.IgnoreNetCollection(netCollection.name) ? netInfo.m_class.name : netInfo.NetName();
                                    Log.Debug(this, "LogNetNames", "NetInfo", netInfo, netInfo.m_class.name, netInfo.name, netInfo.GetLocalizedTitle(), netName, Settings.IgnoreNetText(netName));
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Logs the nets.
        /// </summary>
        /// <param name="caller">The caller.</param>
        /// <param name="block">The block.</param>
        public void LogNets(object caller, string block)
        {
            try
            {
                foreach (NetCollection netCollection in UnityEngine.Object.FindObjectsOfType<NetCollection>())
                {
                    if (netCollection != null && netCollection.m_prefabs != null && !Settings.IgnoreNetCollection(netCollection.name))
                    {
                        foreach (NetInfo netInfo in netCollection.m_prefabs)
                        {
                            if (netInfo != null && !Settings.IgnoreNet(netInfo.NetName()))
                            {
                                this.LogNetInfo((caller == null) ? this : caller, String.IsNullOrEmpty(block) ? "LogNets" : block, netInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "LogNets", ex);
            }
        }

        /// <summary>
        /// Restores the limits.
        /// </summary>
        /// <param name="allowReSet">If set to <c>true</c> set the limits even if group does not change].</param>
        public void RestoreLimits(bool allowReSet = false)
        {
            bool locked = false;
            try
            {
                locked = Global.Lock();

                if (!this.isInitialized || (this.group == Groups.Original && !allowReSet))
                {
                    return;
                }

                Log.Debug(this, "Restore", "Begin");

                try
                {
                    foreach (NetCollection netCollection in UnityEngine.Object.FindObjectsOfType<NetCollection>())
                    {
                        if (netCollection == null)
                        {
                            Log.Warning(this, "Restore", "Null NetCollection");
                            continue;
                        }

                        if (Settings.IgnoreNetCollection(netCollection.name))
                        {
                            continue;
                        }

                        if (netCollection.m_prefabs == null)
                        {
                            Log.Warning(this, "Restore", "Null NetCollection.m_prefabs", netCollection);
                            continue;
                        }

                        foreach (NetInfo netInfo in netCollection.m_prefabs)
                        {
                            if (netInfo == null)
                            {
                                Log.Warning(this, "Restore", "Null NetInfo", netCollection, netCollection.m_prefabs);
                                continue;
                            }

                            if (Log.LogToFile && Log.LogALot)
                            {
                                this.LogNetInfo(this, "Restore", netInfo);
                            }

                            string netName = netInfo.NetName();

                            if (Settings.IgnoreNet(netName))
                            {
                                if (!Global.Settings.SlopeLimitsIgnored.ContainsKey(netName))
                                {
                                    Log.Info(null, null, "NotLimit", netName, netInfo.m_maxSlope);
                                    Global.Settings.SlopeLimitsIgnored[netName] = netInfo.m_maxSlope;
                                }

                                continue;
                            }

                            if (Global.Settings.SlopeLimitsOriginal.ContainsKey(netName))
                            {
                                netInfo.m_maxSlope = Global.Settings.SlopeLimitsOriginal[netName];
                                Log.Info(null, null, "OldLimit", netName, netInfo.m_maxSlope);
                            }
                            else
                            {
                                Log.Info(null, null, "NonLimit", netName, netInfo.m_maxSlope);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(this, "Restore", ex);
                    this.isBroken = true;
                }
                finally
                {
                    this.group = Groups.Original;
                }

                Log.Debug(this, "Restore", "End");
            }
            catch (Exception ex)
            {
                Log.Error(this, "Initialize", ex);
                this.isBroken = true;
            }
            finally
            {
                Global.Release(locked);
            }
        }

        /// <summary>
        /// Sets the limits.
        /// </summary>
        /// <param name="setToGroup">The limits group.</param>
        /// <param name="allowReset">If set to <c>true</c> set the limits even if group does not change].</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Invalid group.</exception>
        public void SetLimits(Groups setToGroup, bool allowReset = false)
        {
            bool locked = false;
            try
            {
                locked = Global.Lock();

                if (this.isBroken || !this.isInitialized || (this.group == setToGroup && !allowReset))
                {
                    return;
                }

                if (setToGroup != Groups.Custom && setToGroup != Groups.Disabled)
                {
                    throw new ArgumentOutOfRangeException("Invalid group");
                }

                Log.Debug(this, "SetLimits", "Begin");

                try
                {
                    bool missing = false;
                    bool found = false;

                    foreach (NetCollection netCollection in UnityEngine.Object.FindObjectsOfType<NetCollection>())
                    {
                        if (netCollection == null)
                        {
                            Log.Warning(this, "SetLimits", "Null NetCollection");
                            continue;
                        }

                        if (Settings.IgnoreNetCollection(netCollection.name))
                        {
                            continue;
                        }

                        if (netCollection.m_prefabs == null)
                        {
                            Log.Warning(this, "SetLimits", "Null NetCollection.m_prefabs", netCollection);
                            continue;
                        }

                        foreach (NetInfo netInfo in netCollection.m_prefabs)
                        {
                            if (netInfo == null)
                            {
                                Log.Warning(this, "SetLimits", "Null NetInfo", netCollection, netCollection.m_prefabs);
                                continue;
                            }

                            if (Log.LogToFile && Log.LogALot)
                            {
                                this.LogNetInfo(this, "SetLimits", netInfo);
                            }

                            string netName = netInfo.NetName();

                            if (Settings.IgnoreNet(netName))
                            {
                                if (!Global.Settings.SlopeLimitsIgnored.ContainsKey(netName))
                                {
                                    Log.Info(null, null, "NotLimit", netName, netInfo.m_maxSlope);
                                    Global.Settings.SlopeLimitsIgnored[netName] = netInfo.m_maxSlope;
                                    found = true;
                                }

                                continue;
                            }

                            if (!Global.Settings.SlopeLimitsOriginal.ContainsKey(netName))
                            {
                                Log.Info(null, null, "OrgLimit", netName, netInfo.m_maxSlope);
                                Global.Settings.SlopeLimitsOriginal[netName] = netInfo.m_maxSlope;
                                found = true;
                            }

                            string match = null;
                            float? cfgLimit = null;

                            if (Global.Settings.SlopeLimits.ContainsKey(netName))
                            {
                                netInfo.m_maxSlope = Global.Settings.SlopeLimits[netName];
                                match = "name";
                                cfgLimit = Global.Settings.SlopeLimits[netName];
                            }
                            else
                            {
                                string name = netName.ToLowerInvariant();
                                if (Global.Settings.SlopeLimitsGeneric.ContainsKey(name))
                                {
                                    netInfo.m_maxSlope = Global.Settings.SlopeLimitsGeneric[name];
                                    cfgLimit = Global.Settings.SlopeLimitsGeneric[name];
                                    match = "generic";
                                }
                                else
                                {
                                    foreach (string generic in Global.Settings.SlopeLimitsGeneric.Keys.ToList().OrderBy(sName => name.Length).Reverse())
                                    {
                                        if (name.Contains(generic))
                                        {
                                            netInfo.m_maxSlope = Global.Settings.SlopeLimitsGeneric[generic];
                                            cfgLimit = Global.Settings.SlopeLimitsGeneric[generic];
                                            match = "part";
                                            break;
                                        }
                                    }
                                }
                            }

                            if (match == null || match != "name")
                            {
                                if (match == null)
                                {
                                    Log.Info(null, null, "NewLimit", netName, netInfo.m_maxSlope);
                                }
                                else
                                {
                                    Log.Info(null, null, "NewLimit", netName, netInfo.m_maxSlope, match);
                                }

                                Global.Settings.SlopeLimits[netName] = netInfo.m_maxSlope;
                                missing = true;
                            }

                            if (setToGroup == Groups.Custom)
                            {
                                if (cfgLimit != null && cfgLimit.HasValue)
                                {
                                    Log.Info(null, null, "SetLimit", netName, cfgLimit.Value);
                                    netInfo.m_maxSlope = cfgLimit.Value;
                                }
                            }
                            else if (setToGroup == Groups.Disabled)
                            {
                                float orgLimit = Global.Settings.SlopeLimitsOriginal[netName];

                                if (cfgLimit == null || !cfgLimit.HasValue)
                                {
                                    cfgLimit = orgLimit;
                                }

                                float disLimit;
                                if (cfgLimit.Value < orgLimit)
                                {
                                    disLimit = orgLimit + (cfgLimit.Value * 2);
                                    if (disLimit > orgLimit * 3)
                                    {
                                        disLimit = orgLimit * 3;
                                    }
                                }
                                else
                                {
                                    disLimit = orgLimit;
                                }

                                Log.Info(null, null, "DisLimit", netName, disLimit, orgLimit, cfgLimit.Value);
                                netInfo.m_maxSlope = disLimit;
                            }
                        }
                    }

                    if (found || missing)
                    {
                        if (missing)
                        {
                            Global.Settings.Update();
                        }

                        Global.Settings.Save();
                    }

                    this.group = setToGroup;
                }
                catch (Exception ex)
                {
                    this.isBroken = true;
                    Log.Error(this, "SetLimits", ex);
                }

                Log.Debug(this, "SetLimits", "End");
            }
            catch (Exception ex)
            {
                Log.Error(this, "Initialize", ex);
                this.isBroken = true;
            }
            finally
            {
                Global.Release(locked);
            }
        }

        /// <summary>
        /// Logs the net information.
        /// </summary>
        /// <param name="caller">The caller.</param>
        /// <param name="block">The block.</param>
        /// <param name="netInfo">The net information.</param>
        private void LogNetInfo(object caller, string block, NetInfo netInfo)
        {
            try
            {
                Log.Debug(
                    (caller == null) ? this : caller,
                    String.IsNullOrEmpty(block) ? "LogNetInfo" : block,
                    "netInfo",
                    netInfo.NetName(),
                    netInfo.CanBeBuilt(),
                    netInfo.m_canCrossLanes,
                    netInfo.m_lanes.Length,
                    netInfo.m_class.name,
                    netInfo.name,
                    netInfo.ToString(),
                    netInfo.GetLocalizedTitle());
            }
            catch
            {
            }
        }
    }
}