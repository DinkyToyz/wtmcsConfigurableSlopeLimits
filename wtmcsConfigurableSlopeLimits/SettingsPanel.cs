using ColossalFramework.UI;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Panel with mod settings.
    /// </summary>
    public class SettingsPanel : UIPanel
    {
        /// <summary>
        /// The title label.
        /// </summary>
        private UILabel title;

        /// <summary>
        /// Called on awake of this instance (whatever that means).
        /// </summary>
        public override void Awake()
        {
            Log.Debug(this, "Awake", "Begin");
            isInteractive = true;
            enabled = true;

            width = 250;
            height = 350;

            title = this.AddUIComponent<UILabel>();

            this.eventKeyPress += SettingsPanel_eventKeyPress;
            Log.Debug(this, "Awake", "Base");
            base.Awake();

            Log.Debug(this, "Awake", "End");
        }

        void SettingsPanel_eventKeyPress(UIComponent component, UIKeyEventParameter eventParam)
        {
            if (eventParam.keycode == KeyCode.Escape)
            {
                Global.CloseSettingsPanel();
            }
        }

        /// <summary>
        /// Called at start of this instance (whatever that means).
        /// </summary>
        public override void Start()
        {
            Log.Debug(this, "Start", "Base");
            base.Start();

            Log.Debug(this, "Start", "Begin");

            relativePosition = new Vector3(10.48f, 80f);
            backgroundSprite = "MenuPanel";
            //color = new Color32(75, 75, 135, 255);
            //this.name = Library.Name + "SettingsPanel";

            title.text = "Slope Limits";
            title.padding = new RectOffset(5, 5, 5, 5);
            title.relativePosition = new Vector3((this.width - title.width) / 2, 0);

            Log.Debug(this, "Start", "End");
        }
    }
}