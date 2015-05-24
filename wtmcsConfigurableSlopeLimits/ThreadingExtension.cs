using ICities;
using System;

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

        /// <summary>
        /// The tool button.
        /// </summary>
        private ToolButton roadsToolButton = null;

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
                return (roadsToolButton != null);
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
                    if (CreateButtons())
                    {
                        createButtonsOnUpdate = false;
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
                if (roadsToolButton != null)
                {
                    roadsToolButton.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "DisposeToolButtons", ex);
                isBroken = true;
            }
            finally
            {
                roadsToolButton = null;
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
                if (roadsToolButton == null && Global.UI.RoadsPanel != null && Global.UI.RoadsOptionPanel != null)
                {
                    Log.Debug(this, "CreateButtons", "RoadsToolButton");
                    roadsToolButton = new ToolButton(Global.UI.RoadsOptionPanel);
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