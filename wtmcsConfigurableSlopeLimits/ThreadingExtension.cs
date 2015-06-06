using ICities;
using System;
using ColossalFramework.UI;
using System.Collections.Generic;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Mod stuff doer.
    /// </summary>
    public class ThreadingExtension : ThreadingExtensionBase
    {
        //// <summary>
        //// The active tool.
        //// </summary>
        //private Tool ActiveTool = Tool.None;

        /// <summary>
        /// Wether to create buttons on update.
        /// </summary>
        private bool createButtonsOnUpdate = true;

        /// <summary>
        /// The doer is broken.
        /// </summary>
        private bool isBroken = false;

        private static readonly string[] buttonParents = 
            {
                "RoadsOptionPanel(RoadsPanel)",
                "PathsOptionPanel(BeautificationPanel)",
                "TracksOptionPanel(PublicTransportPanel)",
                "TunnelsOptionPanel(PublicTransportPanel)",
                "RoadsOptionPanel(PublicTransportPanel)"
            };

        private Dictionary<string, ToolButton> toolButtons = new Dictionary<string, ToolButton>();

        /// <summary>
        /// Tools.
        /// </summary>
        private enum Tool
        {
            None = 0,
            Roads = 1
        }

        /// <summary>
        /// Gets a value indicating whether all buttons have been created.
        /// </summary>
        /// <value>
        ///   <c>true</c> if all buttons have been created; otherwise, <c>false</c>.
        /// </value>
        private bool ButtonsCreated
        {
            get
            {
                try
                {
                    foreach (string parent in buttonParents)
                    {
                        if (!toolButtons.ContainsKey(parent))
                        {
                            return false;
                        }
                    }
                }
                catch 
                {
                    isBroken = true;
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Called when doer is created.
        /// </summary>
        /// <param name="threading">The threading.</param>
        public override void OnCreated(IThreading threading)
        {
            Log.Debug(this, "OnCreated", "Begin");

            try
            {
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnCreated", ex);
                isBroken = true;
            }
            finally
            {
                Log.Debug(this, "OnCreated", "Base");
                base.OnCreated(threading);
            }

            Log.Debug(this, "OnCreated", "End");
        }

        /// <summary>
        /// Called when doer is released.
        /// </summary>
        public override void OnReleased()
        {
            Log.Debug(this, "OnReleased", "Begin");

            try
            {
                DeInitialize();
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnReleased", ex);
                isBroken = true;
            }
            finally
            {
                Log.Debug(this, "OnReleased", "Base");
                base.OnReleased();
            }

            Log.Debug(this, "OnReleased", "End");
        }

        private float updateTimeCheck = 0;

        /// <summary>
        /// Called on update.
        /// </summary>
        /// <param name="realTimeDelta">The real time delta.</param>
        /// <param name="simulationTimeDelta">The simulation time delta.</param>
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            base.OnUpdate(realTimeDelta, simulationTimeDelta);

            try
            {
                if (createButtonsOnUpdate && !isBroken)
                {
                    updateTimeCheck += realTimeDelta;

                    if (updateTimeCheck > 1.25)
                    {
                        updateTimeCheck = 0;

                        if (CreateButtons())
                        {
                            createButtonsOnUpdate = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnUpdate", ex);
                isBroken = true;
            }
        }

        /// <summary>
        /// Deinitialize doer.
        /// </summary>
        protected void DeInitialize()
        {
            Log.Debug(this, "DeInitialize", "Begin");

            try
            {
                DisposeToolButtons();
                Global.DisposeSettingsPanel();
                Global.DisposeUI();
            }
            catch (Exception ex)
            {
                Log.Error(this, "DeInitialize", ex);
                isBroken = true;
            }

            Log.Debug(this, "DeInitialize", "End");
        }

        //        if (ActiveTool == Tool.None)
        //        {
        //            return;
        //        }
        /// <summary>
        /// Disposes the tool button instance.
        /// </summary>
        protected void DisposeToolButtons()
        {
            Log.Debug(this, "DisposeToolButtons", "Begin");

            try
            {
                foreach (ToolButton button in toolButtons.Values)
                {
                    button.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "DisposeToolButtons", ex);
                isBroken = true;
            }
            finally
            {
                toolButtons.Clear();
                createButtonsOnUpdate = true;
            }

            Log.Debug(this, "DisposeToolButtons", "End");
        }

        /// <summary>
        /// Creates the buttons.
        /// </summary>
        /// <returns><c>true</c> if all buttons have been created; otherwise, <c>false</c>.</returns>
        private bool CreateButtons()
        {
            try
            {
                foreach (string parentName in buttonParents)
                {
                    if (!toolButtons.ContainsKey(parentName))
                    {
                        UIComponent parentComponent = Global.UI.Components[parentName];
                        if (parentComponent != null)
                        {
                            ToolButton button = null;
                            UIMultiStateButton snappingToggle = Global.UI.FindComponent<UIMultiStateButton>("SnappingToggle", parentComponent);

                            if (snappingToggle == null)
                            {
                                Log.Debug(this, "CreateButtons", "No Snap Toggle", parentName);
                            }
                            else
                            {
                                Log.Debug(this, "CreateButtons", parentName);
                                button = new ToolButton(parentComponent, snappingToggle);
                            }

                            toolButtons[parentName] = button;
                        }
                    }
                }

                return ButtonsCreated;
            }
            catch (Exception ex)
            {
                Log.Error(this, "CreateButtun", ex);
                isBroken = true;
                return false;
            }
        }
    }
}