using ColossalFramework;
using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

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
        /// The file buffer.
        /// </summary>
        private static List<string> fileBuffer = null;

        /// <summary>
        /// True when buffering file writes.
        /// </summary>
        private static bool fileBuffering = false;

        /// <summary>
        /// True when log file has been created.
        /// </summary>
        private static bool logFileCreated = false;

        /// <summary>
        /// Initializes static members of the <see cref="Log"/> class.
        /// </summary>
        static Log()
        {
            if (Library.IsDebugBuild || FileSystem.Exists(".debug"))
            {
                Log.LogLevel = Log.Level.All;
                Log.LogToFile = true;
            }
            else
            {
                Log.LogLevel = Log.Level.Warning;
                Log.LogToFile = false;
            }

            Log.LogALot = Log.LogToFile && (Library.IsDebugBuild || FileSystem.Exists(".debug.dev"));

            try
            {
                AssemblyName name = Assembly.GetExecutingAssembly().GetName();
                Output(Level.None, null, null, null, Library.Build);
                Output(Level.None, null, null, null, "Cities Skylines", BuildConfig.applicationVersionFull, GetDLCString());
            }
            catch
            {
            }
        }

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
        /// Gets or sets a value indicating whether to buffer file writes.
        /// </summary>
        /// <value>
        ///   <c>true</c> if buffering file writes; otherwise, <c>false</c>.
        /// </value>
        public static bool BufferFileWrites
        {
            get
            {
                return fileBuffering;
            }

            set
            {
                if (value != fileBuffering)
                {
                    try
                    {
                        if (value)
                        {
                            if (LogToFile && fileBuffer == null)
                            {
                                fileBuffer = new List<string>();
                            }
                        }
                        else
                        {
                            if (fileBuffer != null && fileBuffer.Count > 0)
                            {
                                if (LogToFile)
                                {
                                    try
                                    {
                                        using (StreamWriter logFile = OpenLogFile())
                                        {
                                            logFile.Write(String.Join("", fileBuffer.ToArray()).ConformNewlines());
                                            logFile.Close();
                                        }

                                        logFileCreated = true;
                                    }
                                    catch
                                    {
                                    }
                                }

                                fileBuffer.Clear();
                            }
                        }

                        fileBuffering = value;
                    }
                    catch
                    {
                        fileBuffering = false;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether to log a lot or not.
        /// </summary>
        /// <value>
        ///   <c>True</c> if logging a lot; otherwise, <c>false</c>.
        /// </value>
        public static bool LogALot
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether to log to file.
        /// </summary>
        /// <value>
        ///   <c>True</c> logging to file; otherwise, <c>false</c>.
        /// </value>
        public static bool LogToFile
        {
            get;
            private set;
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
        /// Combines the messages in a string.
        /// </summary>
        /// <param name="messages">The messages.</param>
        /// <returns>
        /// The string.
        /// </returns>
        public static string ListString(IEnumerable<object> messages)
        {
            StringBuilder msg = new StringBuilder();
            int mc = 0;

            AddMessages(ref msg, ref mc, messages);

            return (mc == 0) ? (string)null : msg.ToString();
        }

        /// <summary>
        /// Combines the messages in a string.
        /// </summary>
        /// <param name="messages">The messages.</param>
        /// <returns>The string.</returns>
        public static string MessageString(params object[] messages)
        {
            StringBuilder msg = new StringBuilder();
            int mc = 0;

            AddMessages(ref msg, ref mc, messages);

            return (mc == 0) ? (string)null : msg.ToString();
        }

        /// <summary>
        /// Convert log level to message type.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>The message type.</returns>
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
        /// Do nothing (except trigger the class constructor unless it has run already).
        /// </summary>
        public static void NoOp()
        {
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
                AddMessages(ref msg, ref mc, messages);

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

                if (level != Level.None && level <= Level.Warning)
                {
                    try
                    {
                        DebugOutputPanel.AddMessage(level.MessageType(), msg.CleanNewLines());
                    }
                    catch
                    {
                    }
                }

                if (exception != null)
                {
                    msg.Append("\n").Append(exception.StackTrace).Append("\n");
                    while (exception.InnerException != null)
                    {
                        exception = exception.InnerException;
                        msg.Append("\nInner: [").Append(exception.GetType().Name).Append("] ").Append(exception.Message).Append("\n").Append(exception.StackTrace).Append("\n");
                    }
                }

                switch (level)
                {
                    case Level.Info:
                        msg.Insert(0, "Info:    ");
                        break;

                    case Level.Warning:
                        msg.Insert(0, "Warning: ");
                        break;

                    case Level.Error:
                        msg.Insert(0, "Error:   ");
                        break;

                    case Level.None:
                    case Level.All:
                        msg.Insert(0, "         ");
                        break;

                    default:
                        msg.Insert(0, "Debug:   ");
                        break;
                }

                if (level != Level.None && level != Level.All && (level < Level.Debug || !LogToFile))
                {
                    try
                    {
                        UnityEngine.Debug.Log(msg.CleanNewLines());
                    }
                    catch
                    {
                    }
                }

                if (LogToFile)
                {
                    try
                    {
                        msg.Insert(0, ' ').Insert(0, now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        msg.Append("\n");

                        if (fileBuffering && fileBuffer != null)
                        {
                            fileBuffer.Add(msg.ToString());
                        }
                        else
                        {
                            using (StreamWriter logFile = OpenLogFile())
                            {
                                logFile.Write(msg.ConformNewlines());
                                logFile.Close();
                            }

                            logFileCreated = true;
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
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

        /// <summary>
        /// Adds the messages to the string builder.
        /// </summary>
        /// <param name="stringBuilder">The string builder.</param>
        /// <param name="messageCounter">The message counter.</param>
        /// <param name="messages">The messages.</param>
        private static void AddMessages(ref StringBuilder stringBuilder, ref int messageCounter, IEnumerable<object> messages)
        {
            foreach (object msg in messages)
            {
                if (msg == null)
                {
                    continue;
                }

                string message = (msg is string) ? (string)msg : msg.ToString();
                if (message == null)
                {
                    continue;
                }

                if (messageCounter > 0)
                {
                    stringBuilder.Append("; ");
                }
                else if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(' ');
                }

                stringBuilder.Append(message.Trim());
                messageCounter++;
            }
        }

        /// <summary>
        /// Gets the DLC string.
        /// </summary>
        /// <value>
        /// The DLC string.
        /// </value>
        private static string GetDLCString()
        {
            return String.Join(
                ", ",
                ((IEnumerable<SteamHelper.DLC>)Enum.GetValues(typeof(SteamHelper.DLC)))
                .Where(dlc => dlc != SteamHelper.DLC.None && SteamHelper.IsDLCOwned(dlc))
                .Select(dlc => dlc.ToString())
                .ToArray());
        }

        /// <summary>
        /// Gets the mod string.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> return only enabled mods, otherwise return only disabled mods.</param>
        /// <returns>A comma separated list of mod names.</returns>
        private static string GetModString(bool enabled)
        {
            return String.Join(", ", Singleton<PluginManager>.instance.GetPluginsInfo().Where(pi => pi.isEnabled).Select(pi => pi.name).ToArray());
        }

        /// <summary>
        /// Opens the log file.
        /// </summary>
        /// <returns>The open log file writer.</returns>
        private static StreamWriter OpenLogFile()
        {
            string filePathName = FileSystem.FilePathName(".log");
            string filePath = Path.GetDirectoryName(filePathName);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            return new StreamWriter(filePathName, logFileCreated);
        }

        /// <summary>
        /// Named info list for log lines.
        /// </summary>
        public class InfoList
        {
            /// <summary>
            /// The string escape regex.
            /// </summary>
            private static Regex escapeRex = new Regex("([;^\"])");

            /// <summary>
            /// The information list.
            /// </summary>
            private StringBuilder info = new StringBuilder();

            /// <summary>
            /// Adds the info to the list.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="data">The data.</param>
            public void Add(string name, params object[] data)
            {
                if (data.Length == 0)
                {
                    this.AddNameOrSeparator(name);
                    return;
                }

                int dc = 0;

                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == null)
                    {
                        continue;
                    }

                    if (data[i] is IEnumerable<string>)
                    {
                        bool sa = false;
                        foreach (string str in (IEnumerable<string>)data[i])
                        {
                            if (str == null)
                            {
                                continue;
                            }

                            if (!sa)
                            {
                                this.AddNameOrSeparator(name, dc);
                                sa = true;
                            }

                            this.info.Append(escapeRex.Replace(str.Trim(), "^$1"));
                        }

                        if (!sa)
                        {
                            continue;
                        }
                    }
                    else if (data[i] is string)
                    {
                        this.AddNameOrSeparator(name, dc);
                        this.info.Append(escapeRex.Replace(((string)data[i]).Trim(), "^$1"));
                    }
                    else if (data[i] is float)
                    {
                        this.AddNameOrSeparator(name, dc);
                        this.info.Append(((float)data[i]).ToString("#,0.##", CultureInfo.InvariantCulture));
                    }
                    else if (data[i] is int || data[i] is Int16 || data[i] is Int32 || data[i] is Int64 || data[i] is short || data[i] is byte ||
                             data[i] is uint || data[i] is UInt16 || data[i] is UInt32 || data[i] is UInt64 || data[i] is ushort)
                    {
                        this.AddNameOrSeparator(name, dc);
                        this.info.Append(data[i].ToString());
                    }
                    else
                    {
                        string text = data[i].ToString();
                        if (text == null)
                        {
                            continue;
                        }

                        this.AddNameOrSeparator(name, dc);
                        this.info.Append(escapeRex.Replace(text.Trim(), "^$1"));
                    }

                    dc++;
                }
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return this.info.ToString();
            }

            /// <summary>
            /// Adds the name or separator if it should.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="paramPos">The parameter position.</param>
            private void AddNameOrSeparator(string name, int paramPos = -1)
            {
                if (paramPos <= 0)
                {
                    if (this.info.Length > 0)
                    {
                        this.info.Append("; ");
                    }

                    this.info.Append(escapeRex.Replace(name, "^$1"));

                    if (paramPos == 0)
                    {
                        this.info.Append('=');
                    }
                }
                else
                {
                    this.info.Append(", ");
                }
            }
        }
    }
}