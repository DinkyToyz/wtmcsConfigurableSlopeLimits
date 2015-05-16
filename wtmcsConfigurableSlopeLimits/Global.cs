namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
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
        /// The tool button instance.
        /// </summary>
        private static ToolButton toolButtonInstance = null;

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
        /// Gets the tool button instance.
        /// </summary>
        /// <value>
        /// The tool button instance.
        /// </value>
        public static ToolButton ToolButton
        {
            get
            {
                if (toolButtonInstance == null)
                {
                    toolButtonInstance = new ToolButton();
                }

                return toolButtonInstance;
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
        /// Deinitialize the tool button instance.
        /// </summary>
        public static void DeInitializeToolButton()
        {
            if (toolButtonInstance != null)
            {
                toolButtonInstance.DeInitialize();
            }
        }

        /// <summary>
        /// Disposes the tool button instance.
        /// </summary>
        public static void DisposeToolButton()
        {
            if (toolButtonInstance != null)
            {
                toolButtonInstance.DeInitialize();
                toolButtonInstance = null;
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
        /// Reinitialize the tool button instance.
        /// </summary>
        public static void ReInitializeToolButton()
        {
            ToolButton.ReInitialize();
        }
    }
}