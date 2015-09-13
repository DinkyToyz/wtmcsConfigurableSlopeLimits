using ColossalFramework.UI;
using System;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// A tool button for interaction.
    /// </summary>
    internal class ToolButton : IDisposable
    {
        /// <summary>
        /// The button name.
        /// </summary>
        public readonly string ButtonName;

        /// <summary>
        /// The parent panel.
        /// </summary>
        public readonly UIComponent Parent = null;

        /// <summary>
        /// The snapping toggle button.
        /// </summary>
        public readonly UIMultiStateButton SnappingToggle;

        /// <summary>
        /// The tool tip.
        /// </summary>
        public readonly string ToolTip;

        /// <summary>
        /// The sprite image names.
        /// </summary>
        private static readonly string[] SpriteImageNames =
        {
            "Base",
            "BaseFocused",
            "BaseHovered",
            "BasePressed",
            "BaseDisabled",
            "GradeSymbol",
            "GradeSymbolDisabled"
        };

        /// <summary>
        /// The sprite image size.
        /// </summary>
        private static readonly int SpriteImageSize = 36;

        /// <summary>
        /// The sprite resource name.
        /// </summary>
        private static readonly string SpriteResourceName = "ConfigurableSlopeLimitsButton.png";

        /// <summary>
        /// The sprite sets.
        /// </summary>
        private static UI.MultiStateButtonSpriteSetsList spriteSets = new UI.MultiStateButtonSpriteSetsList(
            disabled: new UI.MultiStateButtonSpriteSets(
                background: new UI.MultiStateButtonSpriteSet(
                    disabled: "Base",
                    focused: "Base",
                    hovered: "BaseHovered",
                    normal: "Base",
                    pressed: "Base"),
                foreground: new UI.MultiStateButtonSpriteSet(
                    disabled: "GradeSymbolDisabled",
                    focused: "GradeSymbolDisabled",
                    hovered: "GradeSymbolDisabled",
                    normal: "GradeSymbolDisabled",
                    pressed: "GradeSymbolDisabled")),
            enabled: new UI.MultiStateButtonSpriteSets(
                background: new UI.MultiStateButtonSpriteSet(
                    disabled: "BaseFocused",
                    focused: "BaseFocused",
                    hovered: "BaseFocused",
                    normal: "BaseFocused",
                    pressed: "BaseFocused"),
                foreground: new UI.MultiStateButtonSpriteSet(
                    disabled: "GradeSymbol",
                    focused: "GradeSymbol",
                    hovered: "GradeSymbol",
                    normal: "GradeSymbol",
                    pressed: "GradeSymbol")));

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolButton" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="snappingToggle">The snapping toggle button.</param>
        /// <exception cref="System.NullReferenceException">
        /// Null parent
        /// or
        /// Null snapping toggle button.
        /// </exception>
        public ToolButton(UIComponent parent, UIMultiStateButton snappingToggle)
        {
            Log.Debug(this, this.ParentName, "Construct");

            if (parent == null)
            {
                throw new NullReferenceException("Null parent");
            }

            if (snappingToggle == null)
            {
                throw new NullReferenceException("Null snapping toggle button");
            }

            this.Button = null;
            this.Parent = parent;
            this.SnappingToggle = snappingToggle;

            this.ButtonName = Library.Name + ("(ToolButton(" + parent.name + "))").CompactName();
            this.ToolTip = "Toggle slope limits.";

            this.Initialize();

            Log.Debug(this, this.ParentName, "Constructed");
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ToolButton"/> class.
        /// </summary>
        ~ToolButton()
        {
            this.DeInitialize();
        }

        /// <summary>
        /// Gets the button control.
        /// </summary>
        /// <value>
        /// The button control.
        /// </value>
        public UIMultiStateButton Button { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible
        {
            get
            {
                return this.Button.isVisible;
            }
        }

        /// <summary>
        /// Gets the name of the parent component.
        /// </summary>
        /// <value>
        /// The name of the parent component.
        /// </value>
        private string ParentName
        {
            get
            {
                return (this.Parent == null) ? "~" : this.Parent.name;
            }
        }

        /// <summary>
        /// Uninitialize this instance.
        /// </summary>
        public void DeInitialize()
        {
            if (this.Button == null)
            {
                return;
            }

            Log.Debug(this, this.ParentName, "DeInitialize", "Begin");

            try
            {
                if (this.Parent != null)
                {
                    Log.Debug(this, this.ParentName, "DeInitialize", "Remove");

                    this.Parent.RemoveUIComponent(this.Button);
                }

                GameObject.Destroy(this.Button);
                this.Button = null;
            }
            catch (Exception ex)
            {
                Log.Error(this, "DeInitialize", ex, this.ParentName);
            }

            Log.Debug(this, this.ParentName, "DeInitialize", "End");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Log.Debug(this, this.ParentName, "Dispose", "Begin");

            this.DeInitialize();

            Log.Debug(this, this.ParentName, "Dispose", "End");
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            Log.Debug(this, this.ParentName, "Initialize", "Begin");

            if (this.Parent == null)
            {
                throw new NullReferenceException("Parent == null");
            }

            if (this.Button == null)
            {
                this.Button = Global.UI.FindComponent<UIMultiStateButton>(this.ButtonName, this.Parent);
            }

            if (this.Button != null)
            {
                return;
            }

            Log.Debug(this, this.ParentName, "Initialize", "Create", this.Parent.name, this.ButtonName);

            this.Button = this.Parent.AddUIComponent<UIMultiStateButton>();
            this.Button.name = this.ButtonName;
            this.Button.tooltip = this.ToolTip;
            this.Button.size = new Vector2(this.SnappingToggle.size.x, this.SnappingToggle.size.y);
            this.Button.absolutePosition = new Vector3(this.SnappingToggle.absolutePosition.x, this.SnappingToggle.absolutePosition.y + 38, this.SnappingToggle.absolutePosition.z);
            this.Button.playAudioEvents = true;
            this.Button.activeStateIndex = (Global.LimitsGroup == Limits.Groups.Custom) ? 1 : 0;
            this.Button.spritePadding = new RectOffset();
            this.Button.atlas = Global.UI.CreateAtlas(this.ButtonName + "_Atlas", SpriteResourceName, SpriteImageNames, SpriteImageSize, this.SnappingToggle.atlas.material);
            spriteSets.Apply(this.Button);

            this.Button.eventActiveStateIndexChanged += this.Button_eventActiveStateIndexChanged;
            this.Button.eventVisibilityChanged += this.Button_eventVisibilityChanged;

            Log.Debug(this, this.ParentName, "Initialize", "Button Created", this.Parent.name, this.Button.name);

            Log.Debug(this, this.ParentName, "Initialize", "End");
        }

        /// <summary>
        /// Called when the buttons active state index is changed.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="value">The value.</param>
        private void Button_eventActiveStateIndexChanged(UIComponent component, int value)
        {
            Log.Debug(this, this.ParentName, "Button_eventActiveStateIndexChanged", component, value);

            switch (value)
            {
                // Disable limits.
                case 0:
                    if (Global.Limits.Group != Limits.Groups.Disabled)
                    {
                        Log.Debug(this, this.ParentName, "Button_eventActiveStateIndexChanged", "Global.SetLimits", "Limits.Groups.Disabled");
                        Global.SetLimits(Limits.Groups.Disabled);
                    }
                    break;

                // Enable limits.
                case 1:
                    if (Global.Limits.Group != Limits.Groups.Custom)
                    {
                        Log.Debug(this, this.ParentName, "Button_eventActiveStateIndexChanged", "Global.SetLimits", "Limits.Groups.Custom");
                        Global.SetLimits(Limits.Groups.Custom);
                    }
                    break;
            }
        }

        /// <summary>
        /// Called when the buttons visibility is changed.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="value">If set to <c>true</c> the button was made visible, otherwise invisible.</param>
        private void Button_eventVisibilityChanged(UIComponent component, bool value)
        {
            if (value)
            {
                int state = (Global.LimitsGroup == Limits.Groups.Custom) ? 1 : 0;

                if (state != this.Button.activeStateIndex)
                {
                    this.Button.activeStateIndex = state;
                }
            }
        }
    }
}
