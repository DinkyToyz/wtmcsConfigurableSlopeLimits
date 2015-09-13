using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Mod stuff doer.
    /// </summary>
    public class ThreadingExtension : ThreadingExtensionBase
    {
        /// <summary>
        /// The button parents.
        /// </summary>
        private static readonly string[] ButtonParents =
            {
                "RoadsOptionPanel(RoadsPanel)",
                "PathsOptionPanel(BeautificationPanel)",
                "TracksOptionPanel(PublicTransportPanel)",
                "TunnelsOptionPanel(PublicTransportPanel)",
                "RoadsOptionPanel(PublicTransportPanel)"
            };

        /// <summary>
        /// Whether to create buttons on update.
        /// </summary>
        private bool createButtonsOnUpdate = true;

        /// <summary>
        /// The doer is broken.
        /// </summary>
        private bool isBroken = false;

        /// <summary>
        /// The tool buttons.
        /// </summary>
        private Dictionary<string, ToolButton> toolButtons = new Dictionary<string, ToolButton>();

        /// <summary>
        /// The update time check.
        /// </summary>
        private float updateTimeCheck = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadingExtension"/> class.
        /// </summary>
        public ThreadingExtension()
            : base()
        {
            foreach (string parentName in ButtonParents)
            {
                Global.UI.Components.Add(parentName, typeof(UIPanel));
                Global.UI.Components.Add("SnappingToggle", typeof(UIMultiStateButton), parentName);
            }

            Log.Debug(this, "Constructed");
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
                    foreach (string parent in ButtonParents)
                    {
                        if (!this.toolButtons.ContainsKey(parent))
                        {
                            return false;
                        }
                    }
                }
                catch
                {
                    this.isBroken = true;
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
                this.isBroken = true;
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
                this.DeInitialize();
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnReleased", ex);
                this.isBroken = true;
            }
            finally
            {
                Log.Debug(this, "OnReleased", "Base");
                base.OnReleased();
            }

            Log.Debug(this, "OnReleased", "End");
        }

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
                if (this.createButtonsOnUpdate && !this.isBroken && Global.Limits != null && Global.Limits.IsUsable)
                {
                    this.updateTimeCheck += realTimeDelta;

                    if (this.updateTimeCheck > 1.37)
                    {
                        if (this.CreateButtons())
                        {
                            Log.Debug(this, "OnUpdate", "All Buttons Created");
                            this.createButtonsOnUpdate = false;
                        }

                        this.updateTimeCheck = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnUpdate", ex);
                this.isBroken = true;
            }
        }

        /// <summary>
        /// Uninitialize doer.
        /// </summary>
        protected void DeInitialize()
        {
            Log.Debug(this, "DeInitialize", "Begin");

            try
            {
                this.DisposeToolButtons();
                Global.DisposeUI();
            }
            catch (Exception ex)
            {
                Log.Error(this, "DeInitialize", ex);
                this.isBroken = true;
            }

            Log.Debug(this, "DeInitialize", "End");
        }

        /// <summary>
        /// Disposes the tool button instance.
        /// </summary>
        protected void DisposeToolButtons()
        {
            Log.Debug(this, "DisposeToolButtons", "Begin");

            try
            {
                foreach (ToolButton button in this.toolButtons.Values)
                {
                    button.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "DisposeToolButtons", ex);
                this.isBroken = true;
            }
            finally
            {
                this.toolButtons.Clear();
                this.createButtonsOnUpdate = true;
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
                if (Log.LogToFile) Log.BufferFileWrites = true;

                ////if (Log.LogToFile) Log.Debug(this, "CreateButtons", "Begin");

                ////if (Log.LogToFile) Log.Debug(this, "CreateButtons", "FindComponents");
                Global.UI.FindComponents();
                foreach (string parentName in ButtonParents)
                {
                    if (!this.toolButtons.ContainsKey(parentName))
                    {
                        ////if (Log.LogToFile) Log.Debug(this, "CreateButtons", "Components[parentName]", parentName);
                        UIComponent parentComponent = Global.UI.Components[parentName];
                        if (parentComponent != null)
                        {
                            ////if (Log.LogToFile) Log.Debug(this, "CreateButtons", "Component[parentName/SnappingToggle]", parentName);
                            UIMultiStateButton snappingToggle = (UIMultiStateButton)Global.UI.Components[parentName + "/SnappingToggle"];

                            if (snappingToggle == null)
                            {
                                Log.Debug(this, "CreateButtons", "No Snap Toggle", parentName);
                                this.toolButtons[parentName] = null;
                            }
                            else
                            {
                                Log.Debug(this, "CreateButtons", parentName);
                                this.toolButtons[parentName] = new ToolButton(parentComponent, snappingToggle);
                            }
                        }
                    }
                }

                ////if (Log.LogToFile) Log.Debug(this, "CreateButtons", "End");

                return this.ButtonsCreated;
            }
            catch (Exception ex)
            {
                Log.Error(this, "CreateButtun", ex);
                this.isBroken = true;
                return false;
            }
            finally
            {
                if (Log.LogToFile) Log.BufferFileWrites = false;
            }
        }
    }
}
