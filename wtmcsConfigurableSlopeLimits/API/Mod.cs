﻿using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Mod interface.
    /// </summary>
    public class Mod : IUserMod
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mod"/> class.
        /// </summary>
        public Mod()
        {
            Log.NoOp();
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get
            {
                return Library.Description;
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get
            {
                return Library.Title;
            }
        }

        /// <summary>
        /// Called when initializing mod settings UI.
        /// </summary>
        /// <param name="helper">The helper.</param>
        public void OnSettingsUI(UIHelperBase helper)
        {
            try
            {
                if (Log.LogToFile)
                {
                    Log.BufferFileWrites = true;
                }

                bool inIgnored;
                bool ignoredNet;
                bool isUsed;
                bool isAdded;
                bool doAdd;

                HashSet<string> keys = new HashSet<string>();
                Dictionary<string, List<SlopeLimitSlider>> sliders = new Dictionary<string, List<SlopeLimitSlider>>();

                foreach (string name in Global.Settings.SlopeLimits.Keys)
                {
                    isAdded = keys.Contains(name);
                    inIgnored = Global.Settings.SlopeLimitsIgnored.ContainsKey(name);
                    ignoredNet = Global.NetNames.IgnoreNet(name);
                    isUsed = Global.Settings.SlopeLimitIsUsed(name);
                    doAdd = isUsed && !(isAdded || inIgnored || ignoredNet);

                    Log.Debug(this, "OnSettingsUI", "SlopeLimitsKeys", name, doAdd, inIgnored, ignoredNet, isUsed, isAdded);

                    if (doAdd)
                    {
                        NetNameMap.Generic generic = Global.NetNames.GetGeneric(name);
                        Log.Debug(this, "OnSettingsUI", "Generic", generic.Name, generic.Group);

                        if (!sliders.ContainsKey(generic.Group))
                        {
                            sliders.Add(generic.Group, new List<SlopeLimitSlider>());
                        }
                        sliders[generic.Group].Add(new SlopeLimitSlider(name, generic));

                        keys.Add(name);
                    }
                }

                foreach (string name in NetNameMap.SupportedGenerics)
                {
                    isAdded = keys.Contains(name);
                    inIgnored = Global.Settings.SlopeLimitsIgnored.ContainsKey(name);
                    ignoredNet = Global.NetNames.IgnoreNet(name);
                    doAdd = !(isAdded || inIgnored || ignoredNet);

                    Log.Debug(this, "OnSettingsUI", "SupportedGenerics", name, doAdd, inIgnored, ignoredNet, isAdded);

                    if (doAdd)
                    {
                        NetNameMap.Generic generic = Global.NetNames.GetGeneric(name);

                        if (!sliders.ContainsKey(generic.Group))
                        {
                            sliders.Add(generic.Group, new List<SlopeLimitSlider>());
                        }
                        sliders[generic.Group].Add(new SlopeLimitSlider(name, generic));

                        keys.Add(name);
                    }
                }

                UIHelperBase generalGroup = helper.AddGroup("General");

                generalGroup.AddExtendedSlider(
                    "Horizontal Button Position",
                    -6,
                    +42,
                    1,
                    Global.Settings.ButtonPositionHorizontal,
                    false,
                    value =>
                    {
                        Global.Settings.ButtonPositionHorizontal = (short)value;
                        Global.ButtonPositionUpdateNeeded = true;
                    });

                generalGroup.AddExtendedSlider(
                    "Vertical Button Position",
                    -24,
                    +2,
                    1,
                    Global.Settings.ButtonPositionVertical,
                    false,
                    value =>
                    {
                        Global.Settings.ButtonPositionVertical = (short)value;
                        Global.ButtonPositionUpdateNeeded = true;
                    });

                foreach (string groupName in sliders.Keys.OrderBy(g => Global.NetNames.NetGroupOrder(g)))
                {
                    UIHelperBase group = helper.AddGroup(groupName);

                    foreach (SlopeLimitSlider slider in sliders[groupName].OrderBy(s => s, new SlopeLimitSliderComparer()))
                    {
                        string label = slider.Label;
                        ////+" (" + slider.MaxLimit.ToString("0.00") + ")";

                        Log.Debug(this, "OnSettingsUI", slider.Order, slider.Name, label, groupName, slider.CurLimit, slider.MinLimit, slider.MaxLimit, slider.OrgLimit);

                        group.AddExtendedSlider(
                            label,
                            slider.MinLimit,
                            slider.MaxLimit,
                            0.01f,
                            slider.CurLimit,
                            true,
                            "F2",
                            value =>
                            {
                                Global.Settings.SetLimit(slider.Name, value);
                                Global.LimitUpdateNeeded = Global.Limits != null && Global.Limits.Group != Limits.Groups.Original;
                            });
                    }
                }

                UIHelperBase miscellaneousGroup = helper.AddGroup("Miscellaneous");

                try
                {
                    miscellaneousGroup.AddInformationalText("Config Path:", FileSystem.FilePath);

                    string version = null;
                    Assembly modAss = this.GetType().Assembly;
                    if (modAss != null)
                    {
                        version = modAss.GetName().Version.ToString() + ", " + version + " ";
                    }

                    version += "(" + AssemblyInfo.PreBuildStamps.DateTime.ToString("yyyy-MM-dd HH:mm") + ")";

                    miscellaneousGroup.AddInformationalText("Mod Version:", version);
                }
                catch
                { }

                miscellaneousGroup.AddInformationalText("Note:", "Dumping is only possible when a city is loaded.");

                miscellaneousGroup.AddButton(
                    "Dump network names",
                    () =>
                    {
                        Limits.DumpNetNames();
                    });
            }
            catch (System.Exception ex)
            {
                Log.Error(this, "OnSettingsUI", ex);
            }
            finally
            {
                if (Log.LogToFile)
                {
                    Log.BufferFileWrites = false;
                }
            }
        }

        /// <summary>
        /// Slope limit config slider data.
        /// </summary>
        private struct SlopeLimitSlider
        {
            /// <summary>
            /// The current limit.
            /// </summary>
            public float CurLimit;

            /// <summary>
            /// The maximum limit.
            /// </summary>
            public float MaxLimit;

            /// <summary>
            /// The minimum limit.
            /// </summary>
            public float MinLimit;

            /// <summary>
            /// The name.
            /// </summary>
            public string Name;

            /// <summary>
            /// The sort order.
            /// </summary>
            public int Order;

            /// <summary>
            /// The original limit.
            /// </summary>
            public float OrgLimit;

            /// <summary>
            /// Initializes a new instance of the <see cref="SlopeLimitSlider" /> struct.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="generic">The generic.</param>
            public SlopeLimitSlider(string name, NetNameMap.Generic generic)
            {
                this.Name = name;

                float limit;
                if (Global.Settings.SlopeLimits.TryGetValue(name, out limit))
                {
                    this.CurLimit = limit;
                }
                else if (Global.Settings.TryGetFallBackLimit(name, out limit))
                {
                    this.CurLimit = limit;
                }
                else
                {
                    this.CurLimit = 0.25f;
                }

                this.OrgLimit = Global.Settings.SlopeLimitsOriginal.TryGetValue(name, out limit) ? limit : this.CurLimit;
                this.MinLimit = Global.Settings.MinimumLimit;
                this.MaxLimit = Math.Max(Math.Max(generic.MaxLimit, this.OrgLimit), this.CurLimit);
                this.Order = generic.Order;
            }

            /// <summary>
            /// Gets the label.
            /// </summary>
            /// <value>
            /// The label.
            /// </value>
            public string Label
            {
                get
                {
                    return Global.NetNames.DisplayName(this.Name);
                }
            }
        }

        /// <summary>
        /// Comparer for SlopeLimitSlider.
        /// </summary>
        private class SlopeLimitSliderComparer : IComparer<SlopeLimitSlider>
        {
            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// </summary>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            /// <returns>
            /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.
            /// </returns>
            public int Compare(SlopeLimitSlider x, SlopeLimitSlider y)
            {
                int r = x.Order - y.Order;
                if (r != 0)
                {
                    return r;
                }
                else
                {
                    return string.Compare(x.Name, y.Name, System.StringComparison.InvariantCultureIgnoreCase);
                }
            }
        }
    }
}