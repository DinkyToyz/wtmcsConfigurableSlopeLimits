using ColossalFramework.Plugins;
using UnityEngine;
using System;
using System.Text;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// Log helper.
    /// </summary>
    public class Log
    {
        /// <summary>
        /// The log level.
        /// </summary>
        public PluginManager.MessageType Level = 
            #if DEBUG
            PluginManager.MessageType.Message;
            #else
            PluginManager.MessageType.Warning;
            #endif

        /// <summary>
        /// Outputs the specified message.
        /// </summary>
        /// <param name="level">The message level.</param>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceBlock">The source block.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        protected static void Output(PluginManager.MessageType level, object sourceObject, string sourceBlock, string message, Exception exception = null)
        {
            try
            {
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

                if (!String.IsNullOrEmpty(message))
                {
                    if (msg.Length > 0)
                    {
                        msg.Append(' ');
                    }
                    msg.Append(message);
                }

                if (exception != null)
                {
                    if (msg.Length > 0)
                    {
                        msg.Append(' ');
                    }
                    msg.Append(" [").Append(exception.GetType().Name).Append("] ").Append(exception.Message);
                }

                if (msg.Length == 0)
                {
                    return;
                }

                msg.Insert(0, "] ").Insert(0, Assembly.Name).Insert(0, "[");

                if (level == PluginManager.MessageType.Message || level == PluginManager.MessageType.Error ||
                   (level == PluginManager.MessageType.Warning && level != PluginManager.MessageType.Message))
                {
                    try
                    {
                        DebugOutputPanel.AddMessage(level, msg.ToString());
                    }
                    catch { }
                }

                if (exception != null)
                {
                    msg.Append("\r\n").Append(exception.StackTrace).Append("\r\n");
                    while (exception.InnerException != null)
                    {
                        exception = exception.InnerException;
                        msg.Append("\r\nInner: [").Append(exception.GetType().Name).Append("] ").Append(exception.Message).Append("\r\n").Append(exception.StackTrace).Append("\r\n");
                    }
                }
                try
                {

                    switch (level)
                    {
                        case PluginManager.MessageType.Message:
                            if (level == PluginManager.MessageType.Message)
                            {
                                msg.Insert(0, "Info: ");
                                Debug.Log(msg.ToString());
                            }
                            break;

                        case PluginManager.MessageType.Warning:
                            if (level == PluginManager.MessageType.Message || level == PluginManager.MessageType.Warning)
                            {
                                msg.Insert(0, "Warning: ");
                                Debug.LogWarning(msg.ToString());
                            }
                            break;

                        case PluginManager.MessageType.Error:
                            msg.Insert(0, "Error: ");
                            Debug.LogError(msg.ToString());
                            break;
                    }
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
        /// <param name="message">The message.</param>
        public static void Warning(object sourceObject, string sourceBlock, string message = null)
        {
            Output(PluginManager.MessageType.Warning, sourceObject, sourceBlock, message);
        }

        /// <summary>
        /// Outputs the specified error message.
        /// </summary>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceBlock">The source block.</param>
        /// <param name="exception">The exception.</param>
        public static void Error(object sourceObject, string sourceBlock, Exception exception)
        {
            Output(PluginManager.MessageType.Error, sourceObject, sourceBlock, null, exception);
        }

        /// <summary>
        /// Outputs the specified error message.
        /// </summary>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceBlock">The source block.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public static void Error(object sourceObject, string sourceBlock, string message = null, Exception exception = null)
        {
            Output(PluginManager.MessageType.Error, sourceObject, sourceBlock, message, exception);
        }

        /// <summary>
        /// Informations the specified informational message.
        /// </summary>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceBlock">The source block.</param>
        /// <param name="message">The message.</param>
        public static void Info(object sourceObject, string sourceBlock, string message = null)
        {
            Output(PluginManager.MessageType.Message, sourceObject, sourceBlock, message);
        }

        /// <summary>
        /// Informations the specified informational message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void Info(string message)
        {
            Output(PluginManager.MessageType.Message, null, null, message);
        }
    }
}