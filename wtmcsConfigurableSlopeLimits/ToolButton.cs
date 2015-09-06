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
        /// The tool tip
        /// </summary>
        public readonly string ToolTip;

        /// <summary>
        /// The sprite image names.
        /// </summary>
        private static readonly string[] spriteImageNames =
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
        private static readonly int spriteImageSize = 36;

        /// <summary>
        /// The sprite resource name.
        /// </summary>
        private static readonly string spriteResourceName = "ConfigurableSlopeLimitsButton.png";

        /// <summary>
        /// The sprite sets.
        /// </summary>
        private static UI.MultiStateButtonSpriteSetsList spriteSets = new UI.MultiStateButtonSpriteSetsList
            (
                disabled: new UI.MultiStateButtonSpriteSets
                    (
                        background: new UI.MultiStateButtonSpriteSet
                            (
                                disabled: "Base",
                                focused: "Base",
                                hovered: "BaseHovered",
                                normal: "Base",
                                pressed: "Base"
                            ),
                        foreground: new UI.MultiStateButtonSpriteSet
                            (
                                disabled: "GradeSymbolDisabled",
                                focused: "GradeSymbolDisabled",
                                hovered: "GradeSymbolDisabled",
                                normal: "GradeSymbolDisabled",
                                pressed: "GradeSymbolDisabled"
                            )
                    ),
                enabled: new UI.MultiStateButtonSpriteSets
                    (
                        background: new UI.MultiStateButtonSpriteSet
                            (
                                disabled: "BaseFocused",
                                focused: "BaseFocused",
                                hovered: "BaseFocused",
                                normal: "BaseFocused",
                                pressed: "BaseFocused"
                            ),
                        foreground: new UI.MultiStateButtonSpriteSet
                            (
                                disabled: "GradeSymbol",
                                focused: "GradeSymbol",
                                hovered: "GradeSymbol",
                                normal: "GradeSymbol",
                                pressed: "GradeSymbol"
                            )
                    )
            );

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolButton"/> class.
        /// </summary>
        public ToolButton(UIComponent parent, UIMultiStateButton snappingToggle)
        {
            Log.Debug(this, parentName, "Construct");

            if (parent == null)
            {
                throw new NullReferenceException("parent == null");
            }

            if (snappingToggle == null)
            {
                throw new NullReferenceException("snappingToggle == null");
            }

            this.Button = null;
            this.Parent = parent;
            this.SnappingToggle = snappingToggle;

            this.ButtonName = Library.Name + ("(ToolButton(" + parent.name + "))").CompactName();
            this.ToolTip = "Toggle slope limits.";

            Initialize();

            Log.Debug(this, parentName, "Constructed");
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
        /// Gets a value indicating whether this instance is visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible
        {
            get
            {
                return Button.isVisible;
            }
        }

        /// <summary>
        /// Gets the name of the parent component.
        /// </summary>
        /// <value>
        /// The name of the parent component.
        /// </value>
        private string parentName
        {
            get
            {
                return (Parent == null) ? "~" : Parent.name;
            }
        }

        /// <summary>
        /// Deinitialize this instance.
        /// </summary>
        public void DeInitialize()
        {
            if (Button == null)
            {
                return;
            }

            Log.Debug(this, parentName, "DeInitialize", "Begin");

            try
            {
                if (Parent != null)
                {
                    Log.Debug(this, parentName, "DeInitialize", "Remove");

                    Parent.RemoveUIComponent(Button);
                }

                GameObject.Destroy(Button);
                Button = null;
            }
            catch (Exception ex)
            {
                Log.Error(this, "DeInitialize", ex, parentName);
            }

            Log.Debug(this, parentName, "DeInitialize", "End");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Dispose()
        {
            Log.Debug(this, parentName, "Dispose", "Begin");

            DeInitialize();

            Log.Debug(this, parentName, "Dispose", "End");
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            Log.Debug(this, parentName, "Initialize", "Begin");

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

            Log.Debug(this, parentName, "Initialize", "Create", Parent.name, ButtonName);

            Button = Parent.AddUIComponent<UIMultiStateButton>();
            Button.name = ButtonName;
            Button.tooltip = ToolTip;
            Button.size = new Vector2(SnappingToggle.size.x, SnappingToggle.size.y);
            Button.absolutePosition = new Vector3(SnappingToggle.absolutePosition.x, SnappingToggle.absolutePosition.y + 38, SnappingToggle.absolutePosition.z);
            Button.playAudioEvents = true;
            Button.activeStateIndex = (Global.LimitsGroup == Limits.Groups.Custom) ? 1 : 0;
            Button.spritePadding = new RectOffset();
            Button.atlas = Global.UI.CreateAtlas(ButtonName + "_Atlas", spriteResourceName, spriteImageNames, spriteImageSize, SnappingToggle.atlas.material);
            spriteSets.Apply(Button);

            Button.eventActiveStateIndexChanged += Button_eventActiveStateIndexChanged;
            //Button.eventMouseUp += Button_eventMouseUp;
            Button.eventVisibilityChanged += Button_eventVisibilityChanged;

            Log.Debug(this, parentName, "Initialize", "Button Created", Parent.name, Button.name);

            Log.Debug(this, parentName, "Initialize", "End");
        }

        /// <summary>
        /// Called when the buttons active state index is changed.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="value">The value.</param>
        private void Button_eventActiveStateIndexChanged(UIComponent component, int value)
        {
            Log.Debug(this, parentName, "Button_eventActiveStateIndexChanged", component, value);

            switch (value)
            {
                // Disable limits.
                case 0:
                    if (Global.Limits.Group != Limits.Groups.Disabled)
                    {
                        Log.Debug(this, parentName, "Button_eventActiveStateIndexChanged", "Global.SetLimits", "Limits.Groups.Disabled");
                        Global.SetLimits(Limits.Groups.Disabled);
                    }
                    break;

                // Enable limits.
                case 1:
                    if (Global.Limits.Group != Limits.Groups.Custom)
                    {
                        Log.Debug(this, parentName, "Button_eventActiveStateIndexChanged", "Global.SetLimits", "Limits.Groups.Custom");
                        Global.SetLimits(Limits.Groups.Custom);
                    }
                    break;
            }
        }

        /// <summary>
        /// Called when the buttons visibility is changed.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="value">if set to <c>true</c> the button was made visible, otherwise invisible.</param>
        private void Button_eventVisibilityChanged(UIComponent component, bool value)
        {
            if (value)
            {
                int state = (Global.LimitsGroup == Limits.Groups.Custom) ? 1 : 0;

                if (state != Button.activeStateIndex)
                {
                    Button.activeStateIndex = state;
                }

                //Global.CloseSettingsPanel();
            }
        }
    }
}