using ColossalFramework.Plugins;
using System;
using System.IO;
using System.Text;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Log helper.
    /// </summary>
    internal static class Log
    {
        /// <summary>
        /// The log level.
        /// </summary>
        public static Level LogLevel = Level.Warning;

        /// <summary>
        /// True for logging to file.
        /// </summary>
        public static bool LogToFile = true;

        /// <summary>
        /// True when log file has been created.
        /// </summary>
        private static bool logFileCreated = false;

        /// <summary>
        /// Log levels.
        /// </summary>
        public enum Level
        {
            /// <summary>
            /// Log nothing.
            /// </summary>
            None = 0,

            /// <summary>
            /// Log errors.
            /// </summary>
            Error = 1,

            /// <summary>
            /// Log warnings.
            /// </summary>
            Warning = 2,

            /// <summary>
            /// Log informational messages.
            /// </summary>
            Info = 3,

            /// <summary>
            /// Log debug messages.
            /// </summary>
            Debug = 4,

            /// <summary>
            /// Log all messages.
            /// </summary>
            All = 5
        }

        /// <summary>
        /// Outputs the specified debugging message.
        /// </summary>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceBlock">The source block.</param>
        /// <param name="messages">The messages.</param>
        public static void Debug(object sourceObject, string sourceBlock, params object[] messages)
        {
            Output(Level.Debug, sourceObject, sourceBlock, null, messages);
        }

        /// <summary>
        /// Outputs the specified debugging message.
        /// </summary>
        /// <param name="messages">The messages.</param>
        public static void Debug(params object[] messages)
        {
            Output(Level.Debug, null, null, null, messages);
        }

        /// <summary>
        /// Outputs the specified error message.
        /// </summary>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceBlock">The source block.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messages">The messages.</param>
        public static void Error(object sourceObject, string sourceBlock, Exception exception, params object[] messages)
        {
            Output(Level.Error, sourceObject, sourceBlock, exception, messages);
        }

        /// <summary>
        /// Outputs the specified informational message.
        /// </summary>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceBlock">The source block.</param>
        /// <param name="messages">The messages.</param>
        public static void Info(object sourceObject, string sourceBlock, params object[] messages)
        {
            Output(Level.Info, sourceObject, sourceBlock, null, messages);
        }

        /// <summary>
        /// Outputs the specified informational message.
        /// </summary>
        /// <param name="messages">The messages.</param>
        public static void Info(params object[] messages)
        {
            Output(Level.Info, null, null, null, messages);
        }

        /// <summary>
        /// Comvert log level to message type.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        public static PluginManager.MessageType MessageType(this Level level)
        {
            switch (level)
            {
                case Level.None:
                case Level.Error:
                    return PluginManager.MessageType.Error;

                case Level.Warning:
                    return PluginManager.MessageType.Warning;

                default:
                    return PluginManager.MessageType.Message;
            }
        }

        /// <summary>
        /// Outputs the specified message.
        /// </summary>
        /// <param name="level">The message level.</param>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceBlock">The source block.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messages">The messages.</param>
        public static void Output(Level level, object sourceObject, string sourceBlock, Exception exception, params object[] messages)
        {
            if (level > LogLevel)
            {
                return;
            }

            try
            {
                DateTime now = DateTime.Now;

                StringBuilder msg = new StringBuilder();
                if (sourceObject != null)
                {
                    if (sourceObject is String || sourceObject is string || sourceObject is ValueType)
                    {
                        if (!String.IsNullOrEmpty(sourceObject.ToString()))
                        {
                            msg.Append('<').Append(sourceObject.ToString());
                        }
                    }
                    else if (sourceObject is Type)
                    {
                        msg.Append('<').Append(((Type)sourceObject).Name);
                    }
                    else
                    {
                        msg.Append('<').Append(sourceObject.GetType().Name);
                    }
                }
                if (!String.IsNullOrEmpty(sourceBlock))
                {
                    if (msg.Length == 0)
                    {
                        msg.Append('<');
                    }
                    else
                    {
                        msg.Append('.');
                    }
                    msg.Append(sourceBlock);
                }
                if (msg.Length > 0)
                {
                    msg.Append('>');
                }

                int mc = 0;
                for (int i = 0; i < messages.Length; i++)
                {
                    if (messages[i] == null)
                    {
                        continue;
                    }

                    string message = (messages[i] is string) ? (string)messages[i] : messages[i].ToString();
                    if (message == null)
                    {
                        continue;
                    }

                    if (mc > 0)
                    {
                        msg.Append("; ");
                    }
                    else if (msg.Length > 0)
                    {
                        msg.Append(' ');
                    }

                    msg.Append(message.Trim());
                    mc++;
                }

                if (exception != null)
                {
                    if (msg.Length > 0)
                    {
                        msg.Append(' ');
                    }
                    msg.Append("[").Append(exception.GetType().Name).Append("] ").Append(exception.Message.Trim());
                }

                if (msg.Length == 0)
                {
                    return;
                }

                msg.Insert(0, "] ").Insert(0, Library.Name).Insert(0, "[");

                try
                {
                    DebugOutputPanel.AddMessage(level.MessageType(), msg.CleanNewLines());
                }
                catch { }

                if (exception != null)
                {
                    msg.Append("\n").Append(exception.StackTrace).Append("\n");
                    while (exception.InnerException != null)
                    {
                        exception = exception.InnerException;
                        msg.Append("\nInner: [").Append(exception.GetType().Name).Append("] ").Append(exception.Message).Append("\n").Append(exception.StackTrace).Append("\n");
                    }
                }

                try
                {
                    switch (level)
                    {
                        case Level.None:
                            msg.Insert(0, "Critical: ");
                            UnityEngine.Debug.LogError(msg.CleanNewLines());
                            break;

                        case Level.Error:
                            msg.Insert(0, "Error:    ");
                            UnityEngine.Debug.LogError(msg.CleanNewLines());
                            break;

                        case Level.Warning:
                            msg.Insert(0, "Warning:  ");
                            UnityEngine.Debug.LogWarning(msg.CleanNewLines());
                            break;

                        case Level.Info:
                            msg.Insert(0, "Info:     ");
                            UnityEngine.Debug.Log(msg.CleanNewLines());
                            break;

                        default:
                            msg.Insert(0, "Debug:    ");
                            UnityEngine.Debug.Log(msg.CleanNewLines());
                            break;
                    }
                }
                catch { }

                try
                {
                    using (StreamWriter logFile = new StreamWriter(FileSystem.FilePathName(".log"), logFileCreated))
                    {
                        msg.Insert(0, ' ').Insert(0, now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        msg.Append("\n");

                        logFile.Write(msg.ConformNewlines());
                        logFile.Close();
                    }

                    logFileCreated = true;
                }
                catch { }
            }
            catch { }
        }

        /// <summary>
        /// Outputs the specified warning message.
        /// </summary>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceBlock">The source block.</param>
        /// <param name="messages">The messages.</param>
        public static void Warning(object sourceObject, string sourceBlock, params object[] messages)
        {
            Output(Level.Warning, sourceObject, sourceBlock, null, messages);
        }
    }
}