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
        public readonly UIComponent Parent;

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
            Button.name = ButtonName;
            Button.tooltip = ToolTip;
            Button.size = new Vector2(snappingToggle.size.x, snappingToggle.size.y);
            Button.absolutePosition = new Vector3(snappingToggle.absolutePosition.x, snappingToggle.absolutePosition.y + 38, snappingToggle.absolutePosition.z);
            Button.playAudioEvents = true;
            Button.activeStateIndex = 1;
            Button.spritePadding = new RectOffset();
            Button.atlas = Global.UI.CreateAtlas(ButtonName + "_Atlas", spriteResourceName, spriteImageNames, spriteImageSize, snappingToggle.atlas.material);
            spriteSets.Apply(Button);

            Button.eventActiveStateIndexChanged += Button_eventActiveStateIndexChanged;

            Button.eventMouseUp += Button_eventMouseUp;

            Log.Debug(this, "Initialize", "SnappingToggle.activeStateIndex", snappingToggle.activeStateIndex);
            Log.Debug(this, "Initialize", "SnappingToggle.ActiveStatesCount", snappingToggle.ActiveStatesCount());
            Log.Debug(this, "Initialize", "SnappingToggle.state", snappingToggle.state);
            Log.Debug(this, "Initialize", "Button.activeStateIndex", Button.activeStateIndex);
            Log.Debug(this, "Initialize", "Button.ActiveStatesCount", Button.ActiveStatesCount());
            Log.Debug(this, "Initialize", "Button.state", Button.state);

            Log.Debug(this, "Initialize", "Button Created", Parent.name, Button.name);

            Log.Debug(this, "Initialize", "End");
        }

        /// <summary>
        /// Called when the buttons active state index is changed.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="value">The value.</param>
        private void Button_eventActiveStateIndexChanged(UIComponent component, int value)
        {
            Log.Debug(this, "Button_eventActiveStateIndexChanged", component, value);

            switch (value)
            {
                // Disable limits.
                case 0:
                    Log.Debug(this, "Button_eventActiveStateIndexChanged", "Global.SetLimits", "Limits.Groups.Disabled");
                    Global.SetLimits(Limits.Groups.Disabled);
                    break;

                // Enable limits.
                case 1:
                    Log.Debug(this, "Button_eventActiveStateIndexChanged", "Global.SetLimits", "Limits.Groups.Custom");
                    Global.SetLimits(Limits.Groups.Custom);
                    break;
            }
        }

        /// <summary>
        /// Called when mouse button let up from button.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="eventParam">The event parameter.</param>
        private void Button_eventMouseUp(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (eventParam.buttons == UIMouseButton.Right)
            {
                Log.Debug(this, "Button_eventMouseUp", component, eventParam.buttons, eventParam.clicks);
            }
        }
    }
}