using ICities;
using System;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Mod stuff doer.
    /// </summary>
    public class ThreadingExtension : ThreadingExtensionBase
    {
        /// <summary>
        /// The active tool.
        /// </summary>
        private Tool ActiveTool = Tool.None;

        /// <summary>
        /// The doer is broken.
        /// </summary>
        private bool isBroken = false;

        /// <summary>
        /// The tool button.
        /// </summary>
        private ToolButton RoadsToolButton = null;

        /// <summary>
        /// Tools.
        /// </summary>
        private enum Tool
        {
            None = 0,
            Roads = 1
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
                if (isBroken)
                {
                    return;
                }

                if (Global.UI.RoadsToolIsVisible)
                {
                    if (RoadsToolButton == null)
                    {
                        Log.Debug(this, "OnUpdate", "New RoadsToolButton");
                        RoadsToolButton = new ToolButton(Global.UI.RoadsOptionPanel);
                    }

                    if (ActiveTool != Tool.Roads)
                    {
                        HideButtons();
                        ActiveTool = Tool.Roads;
                    }

                    if (!RoadsToolButton.IsVisible)
                    {
                        RoadsToolButton.Show();
                    }

                    return;
                }

                if (ActiveTool == Tool.None)
                {
                    return;
                }

                Log.Debug(this, "OnUpdate", "No Tool");
                HideButtons();
                ActiveTool = Tool.None;
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
                Global.DisposeUI();
            }
            catch (Exception ex)
            {
                Log.Error(this, "DeInitialize", ex);
                isBroken = true;
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
                if (RoadsToolButton != null)
                {
                    RoadsToolButton.Dispose();
                    RoadsToolButton = null;
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "DisposeToolButtons", ex);
                isBroken = true;
            }

            Log.Debug(this, "DisposeToolButtons", "End");
        }

        /// <summary>
        /// Hides the buttons.
        /// </summary>
        protected void HideButtons()
        {
            Log.Debug(this, "HideButtons", "Begin");

            try
            {
                if (RoadsToolButton != null && RoadsToolButton.IsVisible)
                {
                    RoadsToolButton.Hide();
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "HideButtons", ex);
                isBroken = true;
            }

            Log.Debug(this, "HideButtons", "End");
        }
    }
}