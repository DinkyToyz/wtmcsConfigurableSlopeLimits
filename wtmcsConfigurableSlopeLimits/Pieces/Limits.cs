using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        /// Gets a value indicating whether this <see cref="Limits"/> is broken.
        /// </summary>
        /// <value>
        ///   <c>true</c> if broken; otherwise, <c>false</c>.
        /// </value>
        public bool Broken => this.isBroken;

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
        /// Dumps the nets.
        /// </summary>
        /// <exception cref="InvalidDataException">No network objects.</exception>
        public static void DumpNetNames(bool appendGenerics = false)
        {
            try
            {
                bool foundNetInfo = false;

                List<String> netNames = new List<string>();

                foreach (NetData netData in NetData.FindAll())
                {
                    if (!foundNetInfo)
                    {
                        foundNetInfo = true;
                        NetData.AddHeaderToDumpList(netNames);
                        netNames.Add("");
                    }

                    if (netData.InfoIndex == 0)
                    {
                        netNames.Add("");
                        netData.AddCollectionToDumpList(netNames);
                    }

                    netData.AddInfoToDumpList(netNames);
                }

                bool foundGeneric = false;

                if (appendGenerics)
                {
                    foreach (string name in Global.Settings.SlopeLimits.Keys.Union(NetNameMap.SupportedGenerics).Distinct())
                    {
                        if (!foundGeneric)
                        {
                            foundGeneric = true;

                            if (foundNetInfo)
                            {
                                netNames.Add("");
                                netNames.Add("");
                            }

                            netNames.Add(Log.MessageString(
                                "Generic",
                                "name",
                                "Name",
                                "LowerCaseName",
                                "Group",
                                "DLC",
                                "MaxLimit",
                                "Part",
                                "Order",
                                "IsVariant",
                                "SlopeLimitIgnore",
                                "NetIgnore"));
                            netNames.Add("");
                        }

                        NetNameMap.Generic generic = Global.NetNames.GetGeneric(name);

                        netNames.Add(Log.MessageString(
                            "Generic",
                            name,
                            generic.Name,
                            generic.LowerCaseName,
                            generic.Group,
                            generic.Dependency,
                            generic.MaxLimit,
                            generic.Part,
                            generic.Order,
                            generic.IsVariant ? "IsVariant" : "",
                            Global.Settings.SlopeLimitsIgnored.ContainsKey(name) ? "SlopeLimitIgnore" : "",
                            Global.NetNames.IgnoreNet(name) ? "NetIgnore" : ""));
                    }
                }

                if (foundNetInfo || foundGeneric)
                {
                    netNames.Add("");
                    using (StreamWriter dumpFile = new StreamWriter(FileSystem.FilePathName(".NetNames.txt"), false))
                    {
                        dumpFile.Write(String.Join("\n", netNames.ToArray()).ConformNewlines(false));
                        dumpFile.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Limits), "DumpNetNames", ex);
            }
        }

        /// <summary>
        /// Assures the limits.
        /// </summary>
        public void AssureLimits()
        {
            if (this.group == Groups.Original)
            {
                this.RestoreLimits(true);
            }
            else
            {
                this.SetLimits(this.group, true);
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

                    foreach (NetData netData in NetData.FindUsed())
                    {
                        try
                        {
                            if (Log.LogToFile && Log.LogALot)
                            {
                                this.LogNetInfo(this, "Initialize", netData.Collection, netData.Info);
                            }

                            string netName = netData.NetName;
                            if (netName == null)
                            {
                                continue;
                            }

                            if (Global.NetNames.IgnoreNet(netName))
                            {
                                if (!Global.Settings.SlopeLimitsIgnored.ContainsKey(netName))
                                {
                                    if (Global.NetNames.WarnAboutIgnoredNet(netData.CollectionName, netName))
                                    {
                                        Log.Warning(this, "Initialize", "NotLimit", netName, netData.Info.m_maxSlope);
                                    }
                                    else
                                    {
                                        Log.Info(this, "Initialize", "NotLimit", netName, netData.Info.m_maxSlope);
                                    }

                                    Global.Settings.SlopeLimitsIgnored[netName] = netData.Info.m_maxSlope;
                                    found = true;
                                }

                                continue;
                            }

                            if (!Global.Settings.SlopeLimits.ContainsKey(netName))
                            {
                                float fallBack;
                                bool gotFallback = Global.Settings.TryGetFallBackLimit(netName, out fallBack);

                                Log.Info(this, "Initialize", "NewLimit", netName, netData.Info.m_maxSlope, fallBack);

                                Global.Settings.SlopeLimits[netName] = gotFallback ? fallBack : netData.Info.m_maxSlope;
                                missing = true;
                            }

                            if (!Global.Settings.SlopeLimitsOriginal.ContainsKey(netName))
                            {
                                Log.Info(this, "Initialize", "OrgLimit", netName, netData.Info.m_maxSlope);

                                Global.Settings.SlopeLimitsOriginal[netName] = netData.Info.m_maxSlope;
                                found = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "Initialize", ex, netData);
                            this.isBroken = true;
                            break;
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
                foreach (NetData netData in NetData.FindAll())
                {
                    if (netData.InfoIndex == 0)
                    {
                        netData.LogCollection(this, "LogNetNames");
                    }

                    netData.LogInfo(this, "LogNetNames");
                }
            }
            catch
            {
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

                Log.Debug(this, "RestoreLimits", "Begin");

                try
                {
                    foreach (NetData netData in NetData.FindUsed())
                    {
                        try
                        {
                            if (Log.LogToFile && Log.LogALot)
                            {
                                this.LogNetInfo(this, "Restore", netData.Collection, netData.Info);
                            }

                            string netName = netData.NetName;
                            if (netName == null)
                            {
                                continue;
                            }

                            if (Global.NetNames.IgnoreNet(netName))
                            {
                                if (!Global.Settings.SlopeLimitsIgnored.ContainsKey(netName))
                                {
                                    Log.Info(this, "RestoreLimits", "NotLimit", netName, netData.Info.m_maxSlope);
                                    Global.Settings.SlopeLimitsIgnored[netName] = netData.Info.m_maxSlope;
                                }

                                continue;
                            }

                            if (Global.Settings.SlopeLimitsOriginal.ContainsKey(netName))
                            {
                                if (float.IsNaN(Global.Settings.SlopeLimitsOriginal[netName]) || float.IsInfinity(Global.Settings.SlopeLimitsOriginal[netName]))
                                {
                                    Log.Info(this, "RestoreLimits", "NaNLimit", netName);
                                }
                                else
                                {
                                    netData.Info.m_maxSlope = Global.Settings.SlopeLimitsOriginal[netName];
                                    Log.Info(this, "RestoreLimits", "OldLimit", netName, netData.Info.m_maxSlope);
                                }
                            }
                            else
                            {
                                Log.Info(this, "RestoreLimits", "NonLimit", netName, netData.Info.m_maxSlope);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "RestoreLimits", ex, netData);
                            this.isBroken = true;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(this, "RestoreLimits", ex);
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

                    foreach (NetData netData in NetData.FindUsed())
                    {
                        try
                        {
                            if (Log.LogToFile && Log.LogALot)
                            {
                                this.LogNetInfo(this, "SetLimits", netData.Collection, netData.Info);
                            }

                            string netName = netData.NetName;

                            if (Global.NetNames.IgnoreNet(netName))
                            {
                                if (!Global.Settings.SlopeLimitsIgnored.ContainsKey(netName))
                                {
                                    Log.Info(this, "SetLimits", "NotLimit", netName, netData.Info.m_maxSlope);
                                    Global.Settings.SlopeLimitsIgnored[netName] = netData.Info.m_maxSlope;
                                    found = true;
                                }

                                continue;
                            }

                            if (!Global.Settings.SlopeLimitsOriginal.ContainsKey(netName))
                            {
                                Log.Info(this, "SetLimits", "OrgLimit", netName, netData.Info.m_maxSlope);
                                Global.Settings.SlopeLimitsOriginal[netName] = netData.Info.m_maxSlope;
                                found = true;
                            }

                            string match = null;
                            float? cfgLimit = null;
                            float limit;

                            if (Global.Settings.GetLimit(netName, out limit, out cfgLimit, out match))
                            {
                                netData.Info.m_maxSlope = limit;
                            }

                            if (match == null || match != "name")
                            {
                                if (match == null)
                                {
                                    Log.Info(this, "SetLimits", "NewLimit", netName, netData.Info.m_maxSlope);
                                }
                                else
                                {
                                    Log.Info(this, "SetLimits", "NewLimit", netName, netData.Info.m_maxSlope, match);
                                }

                                Global.Settings.SlopeLimits[netName] = netData.Info.m_maxSlope;
                                missing = true;
                            }

                            if (setToGroup == Groups.Custom)
                            {
                                if (cfgLimit != null && cfgLimit.HasValue)
                                {
                                    Log.Info(this, "SetLimits", "SetLimit", netName, cfgLimit.Value);
                                    netData.Info.m_maxSlope = cfgLimit.Value;
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

                                Log.Info(this, "SetLimits", "DisLimit", netName, disLimit, orgLimit, cfgLimit.Value);
                                netData.Info.m_maxSlope = disLimit;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "SetLimits", ex, netData);
                            this.isBroken = true;
                            break;
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
                Log.Error(this, "SetLimits", ex);
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
        /// <param name="netCollection">The net collection.</param>
        /// <param name="netInfo">The net information.</param>
        private void LogNetInfo(object caller, string block, NetCollection netCollection, NetInfo netInfo)
        {
            try
            {
                Log.Debug(
                    (caller == null) ? this : caller,
                    String.IsNullOrEmpty(block) ? "LogNetInfo" : block,
                    "netInfo",
                    Global.NetNames.GetNetName(netCollection, netInfo),
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

        /// <summary>
        /// Utility for finding network info objects.
        /// </summary>
        private struct NetData
        {
            /// <summary>
            /// The collection.
            /// </summary>
            public readonly NetCollection Collection;

            /// <summary>
            /// The collection index.
            /// </summary>
            public readonly int CollectionIndex;

            /// <summary>
            /// The information.
            /// </summary>
            public readonly NetInfo Info;

            /// <summary>
            /// The item index.
            /// </summary>
            public readonly int InfoIndex;

            /// <summary>
            /// Initializes a new instance of the <see cref="NetData" /> struct.
            /// </summary>
            /// <param name="collectionIndex">Index of the collection.</param>
            /// <param name="infoIndex">Index of the item.</param>
            /// <param name="netCollection">The net collection.</param>
            /// <param name="netInfo">The net information.</param>
            public NetData(int collectionIndex, int infoIndex, NetCollection netCollection, NetInfo netInfo)
            {
                this.Collection = netCollection;
                this.Info = netInfo;
                this.CollectionIndex = collectionIndex;
                this.InfoIndex = infoIndex;
            }

            public string CollectionName
            {
                get
                {
                    return (this.Collection == null) ? null : this.Collection.name;
                }
            }

            /// <summary>
            /// Gets the name of the net.
            /// </summary>
            /// <value>
            /// The name of the net.
            /// </value>
            public string NetName
            {
                get
                {
                    if (this.Info == null)
                    {
                        return null;
                    }
                    else if (this.Collection != null && Global.NetNames.IgnoreNetCollection(this.Collection))
                    {
                        return this.Info.m_class.name;
                    }
                    else
                    {
                        string name = Global.NetNames.GetNetName(this.Collection, this.Info);
                        return (name == null) ? this.Info.m_class.name : name;
                    }
                }
            }

            /// <summary>
            /// Adds the header to dump list.
            /// </summary>
            /// <param name="netNames">The dump list.</param>
            public static void AddHeaderToDumpList(List<String> netNames)
            {
                // Collection
                netNames.Add(Log.MessageString("NetCollection", "netCollection", "netCollection.name", "IgnoreNetCollectionText"));
                netNames.Add(Log.MessageString("Prefabs", "netCollection.m_prefabs", "Count()"));

                //Info
                netNames.Add(Log.MessageString(
                    "Type",
                    "netInfo",
                    "netInfo.m_class.name",
                    "netInfo.name",
                    "netInfo.GetLocalizedTitle()",
                    "netName",
                    "IgnoreNetText",
                    "netInfo.m_maxSlope",
                    "Group",
                    "match",
                    "limit",
                    "cfgLimit"));
            }

            /// <summary>
            /// Finds all network information objects.
            /// </summary>
            /// <returns>All network info objects.</returns>
            public static IEnumerable<NetData> FindAll()
            {
                return Find(true, false);
            }

            /// <summary>
            /// Finds network information objects slope limits should apply to.
            /// </summary>
            /// <returns>Used info objects.</returns>
            public static IEnumerable<NetData> FindUsed()
            {
                return Find(false, true);
            }

            /// <summary>
            /// Adds the collection to dump list.
            /// </summary>
            /// <param name="netNames">The dump list.</param>
            public void AddCollectionToDumpList(List<String> netNames)
            {
                if (this.Collection == null)
                {
                    netNames.Add("(uncollected)");
                }
                else
                {
                    netNames.Add(Log.MessageString("NetCollection", this.Collection, this.Collection.name, Global.NetNames.IgnoreNetCollectionText(this.Collection)));
                    netNames.Add(Log.MessageString("Prefabs", this.Collection.m_prefabs, this.Collection.m_prefabs.Count()));
                }
            }

            /// <summary>
            /// Adds the network information to dump list.
            /// </summary>
            /// <param name="netNames">The dump list.</param>
            public void AddInfoToDumpList(List<String> netNames)
            {
                List<object> info = new List<object>();

                String netName = this.NetName;

                info.Add("NetInfo");
                info.Add(this.Info);
                info.Add(this.Info.m_class.name);
                info.Add(this.Info.name);
                info.Add(this.Info.GetLocalizedTitle());
                info.Add(netName);

                info.Add(Global.NetNames.IgnoreNetText((this.Collection == null) ? (string)null : this.Collection.name, netName));

                info.Add(this.Info.m_maxSlope);
                info.Add(NetNameMap.GetGroup(netName) ?? "-");

                string match = null;
                float? cfgLimit = null;
                float limit;

                if (Global.Settings.GetLimit(netName, out limit, out cfgLimit, out match))
                {
                    info.Add(match);
                    info.Add(limit);
                    info.Add(cfgLimit);
                }

                netNames.Add(Log.ListString(info));
            }

            /// <summary>
            /// Logs the collection.
            /// </summary>
            /// <param name="sourceObject">The source object.</param>
            /// <param name="sourceBlock">The source block.</param>
            public void LogCollection(object sourceObject, string sourceBlock)
            {
                if (Collection == null)
                {
                    Log.Debug(sourceObject ?? this, sourceBlock ?? "LogCollection", "NetCollection", this.Collection, this.Collection.name, Global.NetNames.IgnoreNetCollectionText(this.Collection));
                    Log.Debug(sourceObject ?? this, sourceBlock ?? "LogCollection", "Prefabs", this, Collection.m_prefabs, Collection.m_prefabs.Count());
                }
            }

            /// <summary>
            /// Logs the information.
            /// </summary>
            /// <param name="sourceObject">The source object.</param>
            /// <param name="sourceBlock">The source block.</param>
            public void LogInfo(object sourceObject, string sourceBlock)
            {
                String netName = this.NetName;
                Log.Debug(this, "LogNetNames", "NetInfo", this.Info, this.Info.m_class.name, this.Info.name, this.Info.GetLocalizedTitle(), netName, Global.NetNames.IgnoreNetText(netName));
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                StringBuilder s = new StringBuilder();

                s.Append(this.CollectionIndex.ToString()).Append(", ").Append(this.InfoIndex.ToString());

                s.Append("; ");
                if (this.Collection == null)
                {
                    s.Append('-');
                }
                else if (String.IsNullOrEmpty(this.Collection.name))
                {
                    s.Append(this.Collection.ToString());
                }
                else
                {
                    s.Append(this.Collection.GetType().ToString()).Append(", ").Append(this.Collection.name);
                }

                s.Append("; ");
                if (this.Info == null)
                {
                    s.Append('-');
                }
                else if (String.IsNullOrEmpty(this.Info.name))
                {
                    s.Append(this.Info.ToString());
                }
                else
                {
                    s.Append(this.Info.GetType().ToString()).Append(", ").Append(this.Info.name);
                }

                return s.ToString();
            }

            /// <summary>
            /// Finds network information objects.
            /// </summary>
            /// <param name="returnIgnoredCollections">if set to <c>true</c> return objects from ignored collections.</param>
            /// <param name="logNulls">if set to <c>true</c> log when encountering undefined objects.</param>
            /// <returns>The found network info objects.</returns>
            private static IEnumerable<NetData> Find(bool returnIgnoredCollections, bool logNulls)
            {
                int collectionIndex = -1;
                int infoIndex;

                HashSet<long> collected = new HashSet<long>();

                foreach (NetCollection netCollection in UnityEngine.Object.FindObjectsOfType<NetCollection>())
                {
                    if (netCollection == null)
                    {
                        if (logNulls)
                        {
                            Log.Warning(typeof(NetData), "Find", "Null NetCollection");
                        }

                        continue;
                    }

                    if (!returnIgnoredCollections && Global.NetNames.IgnoreNetCollection(netCollection))
                    {
                        continue;
                    }

                    if (netCollection.m_prefabs == null)
                    {
                        if (logNulls)
                        {
                            Log.Warning(typeof(NetData), "Find", "Null NetCollection.m_prefabs", netCollection);
                        }

                        continue;
                    }

                    infoIndex = 0;

                    foreach (NetInfo netInfo in netCollection.m_prefabs)
                    {
                        if (netInfo == null)
                        {
                            if (logNulls)
                            {
                                Log.Warning(typeof(NetData), "Find", "Null NetInfo", netCollection, netCollection.m_prefabs);
                            }

                            continue;
                        }

                        long key = ((long)(netInfo.GetHashCode()) << 32) | ((long)(netInfo.GetInstanceID()));

                        if (!collected.Contains(key))
                        {
                            if (infoIndex == 0)
                            {
                                collectionIndex++;
                            }

                            collected.Add(key);

                            yield return new NetData(collectionIndex, infoIndex++, netCollection, netInfo);
                        }
                    }
                }

                infoIndex = 0;

                foreach (NetInfo netInfo in UnityEngine.Resources.FindObjectsOfTypeAll<NetInfo>().Where(ni => ni != null && ni.name != "temp"))
                {
                    if (netInfo == null)
                    {
                        continue;
                    }

                    if (!returnIgnoredCollections && Global.NetNames.IgnoreNetCollection(null))
                    {
                        continue;
                    }

                    long key = ((long)(netInfo.GetHashCode()) << 32) | ((long)(netInfo.GetInstanceID()));

                    if (!collected.Contains(key))
                    {
                        if (infoIndex == 0)
                        {
                            collectionIndex++;
                        }

                        collected.Add(key);

                        yield return new NetData(collectionIndex, infoIndex++, null, netInfo);
                    }
                }
            }
        }
    }
}