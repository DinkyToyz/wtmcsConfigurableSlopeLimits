using ColossalFramework.UI;
using System;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    internal class ToolButton
    {
        /// <summary>
        /// The button name.
        /// </summary>
        public readonly string ButtonName;

        /// <summary>
        /// The tool tip
        /// </summary>
        public readonly string ToolTip;

        // <summary>
        // The built in tabstrip.
        // </summary>
        //private UITabstrip builtInTabstrip = null;

        /// <summary>
        /// The button.
        /// </summary>
        private UIMultiStateButton button = null;

        /// <summary>
        /// The parent panel.
        /// </summary>
        private UIComponent parentPanel = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolButton"/> class.
        /// </summary>
        public ToolButton(string buttonName = null, string toolTip = null)
        {
            this.ButtonName = (buttonName == null) ? Library.Name + "_ToolButton" : buttonName;
            this.ToolTip = (toolTip == null) ? Library.Name + "Toggle slope limits.\n? for menu." : toolTip;
        }

        /// <summary>
        /// Deinitialize this instance.
        /// </summary>
        public void DeInitialize()
        {
            if (button != null && parentPanel != null)
            {
                parentPanel.RemoveUIComponent(button);
            }

            button = null;
            //builtInTabstrip = null;
            parentPanel = null;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            try
            {
                if (parentPanel == null /*|| builtInTabstrip == null*/)
                {
                    if (Global.UI.RoadsToolIsVisible)
                    {
                        parentPanel = Global.UI.RoadsOptionPanel;
                        //if (parentPanel != null)
                        //{
                        //    builtInTabstrip = UI.Instance.BuiltInTabsstrip(parentPanel);
                        //}
                    }
                }

                if (parentPanel == null /*|| builtInTabstrip == null || !builtInTabstrip.gameObject.activeInHierarchy */)
                {
                    return;
                }

                if (button == null)
                {
                    UIMultiStateButton snappingToggle = Global.UI.FindComponent<UIMultiStateButton>("SnappingToggle", parentPanel);
                    if (snappingToggle != null)
                    {
                        button = Global.UI.FindComponent<UIMultiStateButton>(ButtonName);
                        if (button == null)
                        {
                            button = parentPanel.AddUIComponent<UIMultiStateButton>();
                            button.name = ButtonName;
                            button.tooltip = ToolTip;
                            button.size = new Vector2(snappingToggle.size.x, snappingToggle.size.y);
                            button.absolutePosition = new Vector3(snappingToggle.absolutePosition.x - 6, snappingToggle.absolutePosition.y - 38, snappingToggle.absolutePosition.z);
                            button.playAudioEvents = true;
                            button.text = "SL";

                            Log.Debug(this, "Initialize", "Button created", parentPanel.name, button.name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "Initialize", ex);
            }
        }

        /// <summary>
        /// Reinitialize this instance.
        /// </summary>
        public void ReInitialize()
        {
            DeInitialize();
            Initialize();
        }
    }
}