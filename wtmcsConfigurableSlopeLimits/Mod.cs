using ICities;
using System.Collections.Generic;
using System.Linq;

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
        /// <exception cref="System.NotImplementedException"></exception>
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
                    if (!Global.Settings.SlopeLimitsIgnored.ContainsKey(name))
                    {
                        Settings.Generic generic = Global.Settings.GetGeneric(name);

                        if (!sliders.ContainsKey(generic.Group))
                        {
                            sliders.Add(generic.Group, new List<SlopeLimitSlider>());
                        }
                        sliders[generic.Group].Add(new SlopeLimitSlider(name, generic.Order));
                    }
                }

                foreach (string groupName in sliders.Keys.OrderBy(g => Settings.NetGroups[g]))
                {
                    UIHelperBase group = helper.AddGroup(groupName);

                    foreach (SlopeLimitSlider slider in sliders[groupName].OrderBy(s => s.Order))
                    {
                        string label = slider.Name + " (" + slider.MaxLimit.ToString("0.00") + ")";

                        Log.Debug(this, "OnSettingsUI", slider.Order, label, groupName, slider.CurLimit, slider.MinLimit, slider.MaxLimit, slider.OrgLimit);

                        group.AddSlider(label, slider.MinLimit, slider.MaxLimit, 0.01f, slider.CurLimit, value => Global.Settings.SetLimit(slider.Name, value));
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
                Name = name;
                CurLimit = Global.Settings.SlopeLimits[name];
                OrgLimit = Global.Settings.SlopeLimitsOriginal.ContainsKey(name) ? Global.Settings.SlopeLimitsOriginal[name] : CurLimit;
                MinLimit = 0.01f;
                MaxLimit = OrgLimit * 3;
                Order = order;
            }
        }

        ///// <summary>
        ///// Called when mod is enabled.
        ///// </summary>
        //public void OnEnabled()
        //{
        //    Log.Debug(this, "OnEnabled");
        //}

        ///// <summary>
        ///// Called when mod is disabled.
        ///// </summary>
        //public void OnDisabled()
        //{
        //    Log.Debug(this, "OnDisabled", "Begin");

        //    Global.RestoreLimits();
        //}
    }
}