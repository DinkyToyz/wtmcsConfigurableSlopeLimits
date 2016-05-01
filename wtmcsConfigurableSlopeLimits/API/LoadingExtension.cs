using System;
using ICities;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Mod stuff loader.
    /// </summary>
    public class LoadingExtension : LoadingExtensionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingExtension"/> class.
        /// </summary>
        public LoadingExtension()
            : base()
        {
            Log.Debug(this, "Constructed");
        }

        /// <summary>
        /// Called when mod is created.
        /// </summary>
        /// <param name="loading">The loading.</param>
        public override void OnCreated(ILoading loading)
        {
            Log.Debug(this, "OnCreated", "Begin");

            try
            {
                if (Log.LogToFile)
                {
                    Log.BufferFileWrites = true;
                }

                Global.InitializeLimits();
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnCreated", ex);
            }
            finally
            {
                Log.Debug(this, "OnCreated", "Base");
                base.OnCreated(loading);

                if (Log.LogToFile)
                {
                    Log.BufferFileWrites = false;
                }
            }

            Log.Debug(this, "OnCreated", "End");
        }

        /// <summary>
        /// Called when map (etc) is loaded.
        /// </summary>
        /// <param name="mode">The load mode.</param>
        public override void OnLevelLoaded(LoadMode mode)
        {
            Log.Debug(this, "OnLevelLoaded", "Begin");

            try
            {
                if (Log.LogToFile)
                {
                    Log.BufferFileWrites = true;
                }

                Global.SetLimits(Limits.Groups.Custom);
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnLevelLoaded", ex);
            }
            finally
            {
                Log.Debug(this, "OnLevelLoaded", "Base");
                base.OnLevelLoaded(mode);

                if (Log.LogToFile)
                {
                    Log.BufferFileWrites = false;
                }
            }

            Log.Debug(this, "OnLevelLoaded", "End");
        }

        /// <summary>
        /// Called when map (etc) unloads.
        /// </summary>
        public override void OnLevelUnloading()
        {
            Log.Debug(this, "OnLevelUnloading", "Begin");

            try
            {
                if (Log.LogToFile)
                {
                    Log.BufferFileWrites = true;
                }

                Global.RestoreLimits();
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnLevelUnloading", ex);
            }
            finally
            {
                Log.Debug(this, "OnLevelUnloading", "Base");
                base.OnLevelUnloading();

                if (Log.LogToFile)
                {
                    Log.BufferFileWrites = false;
                }
            }

            Log.Debug(this, "OnLevelUnloading", "End");
        }

        /// <summary>
        /// Called when mod is released.
        /// </summary>
        public override void OnReleased()
        {
            Log.Debug(this, "OnReleased", "Begin");

            try
            {
                if (Log.LogToFile)
                {
                    Log.BufferFileWrites = true;
                }

                Global.DisposeLimits();
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnReleased", ex);
            }
            finally
            {
                Log.Debug(this, "OnReleased", "Base");
                base.OnReleased();

                if (Log.LogToFile)
                {
                    Log.BufferFileWrites = false;
                }
            }

            Log.Debug(this, "OnReleased", "End");
        }
    }
}