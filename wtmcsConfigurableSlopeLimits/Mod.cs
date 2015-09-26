using System.Collections.Generic;
using System.Linq;
using ICities;

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
                Dictionary<string, List<SlopeLimitSlider>> sliders = new Dictionary<string, List<SlopeLimitSlider>>();

                foreach (string name in Global.Settings.SlopeLimits.Keys)
                {
                    Log.Debug(this, "OnSettingsUI", name, Global.Settings.SlopeLimitsIgnored.ContainsKey(name), Settings.IgnoreNet(name));

                    if (!Global.Settings.SlopeLimitsIgnored.ContainsKey(name) && !Settings.IgnoreNet(name))
                    {
                        Settings.Generic generic = Global.Settings.GetGeneric(name);

                        if (!sliders.ContainsKey(generic.Group))
                        {
                            sliders.Add(generic.Group, new List<SlopeLimitSlider>());
                        }
                        sliders[generic.Group].Add(new SlopeLimitSlider(name, generic.Order));
                    }
                }

                UIHelperBase generalGroup = helper.AddGroup("General");
                generalGroup.AddSlider(
                    "Horizontal Button Position",
                    -10,
                    +50,
                    1,
                    Global.Settings.ButtonPositionHorizontal,
                    value =>
                    {
                        Global.Settings.ButtonPositionHorizontal = (short)value;
                        Global.ButtonPositionUpdateNeeded = true;
                    });

                foreach (string groupName in sliders.Keys.OrderBy(g => Settings.NetGroups[g]))
                {
                    UIHelperBase group = helper.AddGroup(groupName);

                    foreach (SlopeLimitSlider slider in sliders[groupName].OrderBy(s => s, new SlopeLimitSliderComparer()))
                    {
                        string label = slider.Label;
                        ////+" (" + slider.MaxLimit.ToString("0.00") + ")";

                        Log.Debug(this, "OnSettingsUI", slider.Order, slider.Name, label, groupName, slider.CurLimit, slider.MinLimit, slider.MaxLimit, slider.OrgLimit);

                        group.AddSlider(
                            label,
                            slider.MinLimit,
                            slider.MaxLimit,
                            0.01f,
                            slider.CurLimit,
                            value =>
                            {
                                Global.Settings.SetLimit(slider.Name, value);
                                Global.LimitUpdateNeeded = Global.Limits != null && Global.Limits.Group != Limits.Groups.Original;
                            });
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(this, "OnSettingsUI", ex);
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
            /// Initializes a new instance of the <see cref="SlopeLimitSlider"/> struct.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="order">The sort order.</param>
            public SlopeLimitSlider(string name, int order)
            {
                this.Name = name;
                this.CurLimit = Global.Settings.SlopeLimits[name];
                this.OrgLimit = Global.Settings.SlopeLimitsOriginal.ContainsKey(name) ? Global.Settings.SlopeLimitsOriginal[name] : this.CurLimit;
                this.MinLimit = 0.01f;
                this.MaxLimit = 1.00f;
                ////this.MaxLimit = this.OrgLimit * 3;
                ////if (this.MaxLimit > 1.0f)
                ////{
                ////    this.MaxLimit = 1.0f;
                ////}
                this.Order = order;
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
                    return Global.Settings.DisplayName(this.Name);
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
