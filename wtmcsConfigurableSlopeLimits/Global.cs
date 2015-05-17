namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Global objects.
    /// </summary>
    internal static class Global
    {
        /// <summary>
        /// The UI instance.
        /// </summary>
        public static UI uiInstance = null;

        /// <summary>
        /// The settings instance.
        /// </summary>
        private static Settings settingsInstance = null;

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
                uiInstance.DeInitialize();
            }
        }

        /// <summary>
        /// Disposes the UI instance.
        /// </summary>
        public static void DisposeUI()
        {
            if (uiInstance != null)
            {
                uiInstance.DeInitialize();
                uiInstance = null;
            }
        }
    }
}