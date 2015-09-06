using System;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Global objects.
    /// </summary>
    internal static class Global
    {
        ///// <summary>
        ///// The settings panel.
        ///// </summary>
        //public static SettingsPanel settingsPanel = null;

        /// <summary>
        /// The UI instance.
        /// </summary>
        public static UI uiInstance = null;

        /// <summary>
        /// The limits instance.
        /// </summary>
        private static Limits limitsInstance = null;

        /// <summary>
        /// The settings instance.
        /// </summary>
        private static Settings settingsInstance = null;

        /// <summary>
        /// Gets the limits.
        /// </summary>
        /// <value>
        /// The limits.
        /// </value>
        public static Limits Limits
        {
            get
            {
                if (limitsInstance == null)
                {
                    limitsInstance = new Limits();
                }

                return limitsInstance;
            }
        }

        /// <summary>
        /// Gets the limits group.
        /// </summary>
        /// <value>
        /// The limits group.
        /// </value>
        public static Limits.Groups LimitsGroup
        {
            get
            {
                try
                {
                    Limits.Initialize();
                    return Limits.Group;
                }
                catch (Exception ex)
                {
                    Log.Error(typeof(Global), "GetLimits", ex);
                    return Limits.Groups.Original;
                }
            }
        }

        /// <summary>
        /// Gets the current settings.
        /// </summary>
        /// <value>
        /// The current settings.
        /// </value>
        public static Settings Settings
        {
            get
            {
                if (settingsInstance == null)
                {
                    settingsInstance = ConfigurableSlopeLimits.Settings.Load();
                }

                return settingsInstance;
            }
        }

        /// <summary>
        /// Gets the UI instance.
        /// </summary>
        /// <value>
        /// The UI instance.
        /// </value>
        public static UI UI
        {
            get
            {
                if (uiInstance == null)
                {
                    uiInstance = new UI();
                }

                return uiInstance;
            }
        }

        /// <summary>
        /// Deinitialize the UI instance.
        /// </summary>
        public static void DeInitializeUI()
        {
            if (uiInstance != null)
            {
                try
                {
                    uiInstance.DeInitialize();
                }
                catch (Exception ex)
                {
                    Log.Error(typeof(Global), "DeInitializeUI", ex);
                }
            }
        }

        /// <summary>
        /// Disposes the limits.
        /// </summary>
        public static void DisposeLimits()
        {
            if (limitsInstance != null)
            {
                try
                {
                    limitsInstance.Restore();
                }
                catch (Exception ex)
                {
                    Log.Error(typeof(Global), "DisposeLimits", ex);
                }
                finally
                {
                    limitsInstance = null;
                }
            }
        }

        /// <summary>
        /// Disposes the UI instance.
        /// </summary>
        public static void DisposeUI()
        {
            if (uiInstance != null)
            {
                try
                {
                    uiInstance.DeInitialize();
                }
                catch (Exception ex)
                {
                    Log.Error(typeof(Global), "DisposeUI", ex);
                }
                finally
                {
                    uiInstance = null;
                }
            }
        }

        /// <summary>
        /// Initializes the limits.
        /// </summary>
        public static void InitializeLimits()
        {
            try
            {
                Limits.Initialize();
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Global), "InitializeLimits", ex);
            }
        }

        /// <summary>
        /// Restores the limits.
        /// </summary>
        public static void RestoreLimits()
        {
            if (limitsInstance != null)
            {
                try
                {
                    limitsInstance.Restore();
                }
                catch (Exception ex)
                {
                    Log.Error(typeof(Global), "RestoreLimits", ex);
                }
            }
        }

        /// <summary>
        /// Sets the limits.
        /// </summary>
        public static void SetLimits(Limits.Groups setToGroup)
        {
            try
            {
                Limits.Initialize();
                Limits.SetLimits(setToGroup);
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Global), "SetLimits", ex);
            }
        }
    }
}