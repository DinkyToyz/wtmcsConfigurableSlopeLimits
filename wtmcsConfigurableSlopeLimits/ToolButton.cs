using ColossalFramework.UI;
using System;
using UnityEngine;
using System.Text.RegularExpressions;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    internal class ToolButton : IDisposable
    {
        /// <summary>
        /// The button name.
        /// </summary>
        public readonly string ButtonName;

        /// <summary>
        /// The parent panel.
        /// </summary>
        public readonly UIComponent Parent;

        /// <summary>
        /// The tool tip
        /// </summary>
        public readonly string ToolTip;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolButton"/> class.
        /// </summary>
        public ToolButton(UIComponent parent)
        {
            Log.Debug(this, "Construct");

            if (parent == null)
            {
                throw new NullReferenceException("parent == null");
            }

            this.Button = null;
            this.Parent = parent;
            this.ButtonName = Library.Name + ("ToolButton" + parent.name).ASCIICapitals();
            this.ToolTip = "Toggle slope limits.\nNothing for menu.";

            Initialize();

            Log.Debug(this, "Constructed");
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ToolButton"/> class.
        /// </summary>
        ~ToolButton()
        {
            DeInitialize();
        }

        /// <summary>
        /// The button.
        /// </summary>
        public UIMultiStateButton Button { get; private set; }

        /// <summary>
        /// Deinitialize this instance.
        /// </summary>
        public void DeInitialize()
        {
            if (Button == null)
            {
                return;
            }

            Log.Debug(this, "DeInitialize", "Begin");

            try
            {
                if (Parent != null)
                {
                    Log.Debug(this, "DeInitialize", "Remove");

                    Parent.RemoveUIComponent(Button);
                }

                GameObject.Destroy(Button);
                Button = null;
            }
            catch (Exception ex)
            {
                Log.Error(this, "DeInitialize", ex);
            }

            Log.Debug(this, "DeInitialize", "End");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Dispose()
        {
            Log.Debug(this, "Dispose", "Begin");

            DeInitialize();

            Log.Debug(this, "Dispose", "End");
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            Log.Debug(this, "Initialize", "Begin");

            if (Parent == null)
            {
                throw new NullReferenceException("Parent == null");
            }

            if (Button == null)
            {
                Button = Global.UI.FindComponent<UIMultiStateButton>(ButtonName, Parent);
            }

            if (Button != null)
            {
                return;
            }

            UIMultiStateButton snappingToggle = Global.UI.FindComponent<UIMultiStateButton>("SnappingToggle", Parent);
            if (snappingToggle == null)
            {
                throw new NullReferenceException("snappingToggle == null");
            }

            Log.Debug(this, "Initialize", "Create", Parent.name, ButtonName);

            Button = Parent.AddUIComponent<UIMultiStateButton>();
            Button.Hide();
            Button.name = ButtonName;
            Button.tooltip = ToolTip;
            Button.size = new Vector2(snappingToggle.size.x, snappingToggle.size.y);
            Button.absolutePosition = new Vector3(snappingToggle.absolutePosition.x - 6, snappingToggle.absolutePosition.y + 38, snappingToggle.absolutePosition.z);
            Button.playAudioEvents = true;
            Button.text = "SL";

            Log.Debug(this, "Initialize", "SnappingToggle", snappingToggle.activeStateIndex);
            Log.Debug(this, "Initialize", "SnappingToggle", snappingToggle.ActiveStatesCount());
            Log.Debug(this, "Initialize", "SnappingToggle", snappingToggle.state);
            Log.Debug(this, "Initialize", "SnappingToggle", Button.activeStateIndex);
            Log.Debug(this, "Initialize", "SnappingToggle", Button.ActiveStatesCount());
            Log.Debug(this, "Initialize", "SnappingToggle", Button.state);

            Log.Debug(this, "Initialize", "Button Created", Parent.name, Button.name);

            Log.Debug(this, "Initialize", "End");
        }

        /// <summary>
        /// Shows this instance.
        /// </summary>
        public void Show()
        {
            Log.Debug(this, "Show");
            Button.Show();
        }

        /// <summary>
        /// Hides this instance.
        /// </summary>
        public void Hide()
        {
            Log.Debug(this, "Hide");
            Button.Hide();
        }

        public bool IsVisible 
        {
            get
            {
                return Button.isVisible;
            }
        }
    }
}