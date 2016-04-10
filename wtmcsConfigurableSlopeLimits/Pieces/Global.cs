using System;
using System.Threading;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Global objects.
    /// </summary>
    internal static class Global
    {
        /// <summary>
        /// The button position needs to be updated.
        /// </summary>
        public static bool ButtonPositionUpdateNeeded = false;

        /// <summary>
        /// The net limit needs to be updated.
        /// </summary>
        public static bool LimitUpdateNeeded = false;

        /// <summary>
        /// The data lock instance.
        /// </summary>
        private static readonly object LockInstance = new object();

        /// <summary>
        /// The limits instance.
        /// </summary>
        private static Limits limitsInstance = null;

        /// <summary>
        /// The net names map instance.
        /// </summary>
        private static NetNameMap netNamesInstance = null;

        /// <summary>
        /// The settings instance.
        /// </summary>
        private static Settings settingsInstance = null;

        /// <summary>
        /// The UI instance.
        /// </summary>
        private static UI uiInstance = null;

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
        /// Gets the net names map.
        /// </summary>
        /// <value>
        /// The net names map.
        /// </value>
        public static NetNameMap NetNames
        {
            get
            {
                if (netNamesInstance == null)
                {
                    netNamesInstance = new NetNameMap();
                }

                return netNamesInstance;
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
        /// Uninitialize the UI instance.
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
                    limitsInstance.RestoreLimits();
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
        /// Locks the data.
        /// </summary>
        /// <returns>Tue when locked.</returns>
        public static bool Lock()
        {
            Monitor.Enter(LockInstance);
            return true;
        }

        /// <summary>
        /// Release the data.
        /// </summary>
        /// <param name="locked">Set to <c>true</c> if locked.</param>
        public static void Release(bool locked = true)
        {
            if (locked)
            {
                try
                {
                    Monitor.Exit(LockInstance);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Sets the limits for the current group.
        /// </summary>
        public static void ReSetLimits()
        {
            try
            {
                if (limitsInstance != null && Global.Limits.IsUsable)
                {
                    switch (Limits.Group)
                    {
                        case Limits.Groups.Original:
                            Limits.RestoreLimits(true);
                            break;

                        default:
                            Limits.SetLimits(Limits.Group, true);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Global), "ReSetLimits", ex);
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
                    limitsInstance.RestoreLimits();
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
        /// <param name="setToGroup">The group to set to.</param>
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