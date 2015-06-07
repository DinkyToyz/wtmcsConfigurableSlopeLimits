using ColossalFramework.UI;
using System;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Global objects.
    /// </summary>
    internal static class Global
    {
        public static SettingsPanel settingsPanel = null;

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

        public static SettingsPanel SettingsPanel
        {
            get
            {
                CreateSettingsPanel();

                return settingsPanel;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the settings panel is visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if the settings panel is visible; otherwise, <c>false</c>.
        /// </value>
        public static bool SettingsPanelIsVisible
        {
            get
            {
                return (settingsPanel != null && settingsPanel.isVisible);
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
        /// Closes the settings panel.
        /// </summary>
        public static void CloseSettingsPanel()
        {
            if (settingsPanel == null || !settingsPanel.isVisible)
            {
                return;
            }

            Log.Debug(typeof(Global), "CloseSettingsPanel", "Begin");

            try
            {
                settingsPanel.isVisible = false;
                settingsPanel.Hide();
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Global), "CloseSettingsPanel", ex);
            }

            Log.Debug(typeof(Global), "CloseSettingsPanel", "End");
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
        /// Disposes the settings panel.
        /// </summary>
        public static void DisposeSettingsPanel()
        {
            if (settingsPanel != null)
            {
                Log.Debug(typeof(Global), "DisposeSettingsPanel", "Begin");

                try
                {
                    UIView.Destroy(settingsPanel);
                }
                catch (Exception ex)
                {
                    Log.Error(typeof(Global), "DisposeSettingsPanel", ex);
                }
                finally
                {
                    settingsPanel = null;
                }

                Log.Debug(typeof(Global), "DisposeSettingsPanel", "End");
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

        /// <summary>
        /// Shows the settings panel.
        /// </summary>
        public static void ShowSettingsPanel()
        {
            Log.Debug(typeof(Global), "ShowSettingsPanel", "Begin");

            CreateSettingsPanel();
            try
            {
                if (!settingsPanel.isVisible)
                {
                    settingsPanel.isVisible = true;
                    settingsPanel.Show();
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Global), "ShowSettingsPanel", ex);
            }

            Log.Debug(typeof(Global), "ShowSettingsPanel", "End");
        }

        /// <summary>
        /// Creates the settings panel.
        /// </summary>
        private static void CreateSettingsPanel()
        {
            if (settingsPanel != null)
            {
                return;
            }

            Log.Debug(typeof(Global), "CreateSettingsPanel", "Begin");

            try
            {
                UIView view = UIView.GetAView();

                settingsPanel = (SettingsPanel)view.AddUIComponent(typeof(SettingsPanel));

                settingsPanel.transform.parent = view.transform;
                settingsPanel.Hide();
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Global), "CreateSettingsPanel", ex);
            }

            Log.Debug(typeof(Global), "CreateSettingsPanel", "End");
        }
    }
}