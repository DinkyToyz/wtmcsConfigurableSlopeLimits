using System;
using System.Linq;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
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
            Original = 1,
            Custom = 2,
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
                return group;
            }

            set
            {
                if (value == Groups.Original)
                {
                    Restore();
                }
                else
                {
                    SetLimits(value);
                }
            }
        }

        /// <summary>
        /// Initializes the slopes.
        /// </summary>
        /// <returns>True on success.</returns>
        public void Initialize()
        {
            if (isBroken || isInitialized)
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
                    foreach (NetInfo netInfo in netCollection.m_prefabs)
                    {
                        //LogNetInfo(this, "Initialize", netInfo);

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

                        if (Settings.GenericNames.Contains(netName))
                        {
                            if (!Global.Settings.SlopeLimits.ContainsKey(netName))
                            {
                                Log.Info(null, null, "NewLimit", netName, netInfo.m_maxSlope);
                                Global.Settings.SlopeLimits[netName] = netInfo.m_maxSlope;
                                missing = true;
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

                    isInitialized = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "Initialize", ex);
                isBroken = true;
            }

            Log.Debug(this, "Initialize", "End");
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
                    foreach (NetInfo netInfo in netCollection.m_prefabs)
                    {
                        LogNetInfo((caller == null) ? this : caller, String.IsNullOrEmpty(block) ? "LogNets" : block, netInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "LogNets", ex);
            };
        }

        /// <summary>
        /// Restores the limits.
        /// </summary>
        public void Restore()
        {
            if (!isInitialized || group == Groups.Original)
            {
                return;
            }

            Log.Debug(this, "Restore", "Begin");

            try
            {
                foreach (NetCollection netCollection in UnityEngine.Object.FindObjectsOfType<NetCollection>())
                {
                    foreach (NetInfo netInfo in netCollection.m_prefabs)
                    {
                        //LogNetInfo(this, "Initialize", netInfo);

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
                isBroken = true;
            }
            finally
            {
                group = Groups.Original;
            }

            Log.Debug(this, "Restore", "End");
        }

        /// <summary>
        /// Sets the limits.
        /// </summary>
        /// <param name="setToGroup">The limits group.</param>
        public void SetLimits(Groups setToGroup)
        {
            if (isBroken || !isInitialized || group == setToGroup)
            {
                return;
            }

            if (setToGroup != Groups.Custom && setToGroup != Groups.Disabled)
            {
                throw new ArgumentOutOfRangeException("setToGroup != Groups.Custom && setToGroup != Groups.Disabled");
            }

            Log.Debug(this, "SetLimits", "Begin");

            try
            {
                bool missing = false;
                bool found = false;

                foreach (NetCollection netCollection in UnityEngine.Object.FindObjectsOfType<NetCollection>())
                {
                    foreach (NetInfo netInfo in netCollection.m_prefabs)
                    {
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

                            float disLimit = (cfgLimit.Value < orgLimit) ? orgLimit + cfgLimit.Value : orgLimit;

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

                group = setToGroup;
            }
            catch (Exception ex)
            {
                isBroken = true;
                Log.Error(this, "SetLimits", ex);
            }

            Log.Debug(this, "SetLimits", "End");
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
                Log.Debug((caller == null) ? this : caller, String.IsNullOrEmpty(block) ? "LogNetInfo" : block,
                          "netInfo", netInfo.NetName(), netInfo.CanBeBuilt(), netInfo.m_canCrossLanes, netInfo.m_lanes.Length, netInfo.name, netInfo.ToString(), netInfo.GetLocalizedTitle());
            }
            catch { }
        }
    }
}