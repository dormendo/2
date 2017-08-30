using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Maxima.BookmakerPrototype.Log.Configuration;

namespace Maxima.BookmakerPrototype.Log
{
    internal class NcfLogEngineProvider
    {
        [ThreadStatic]
        private static StringBuilder strBuilder;

        private Dictionary<string, IFileWriter> _writers = new Dictionary<string, IFileWriter>();
        private Dictionary<string, ILogEngine> _loggers = new Dictionary<string, ILogEngine>();

        internal static StringBuilder AcquireStringBuilder()
        {
            if (strBuilder == null)
            {
                strBuilder = new StringBuilder(16000);
            }
            else
            {
                strBuilder.Clear();
            }

            return strBuilder;
        }

        internal void Initialize()
        {
            LogEngineConfigurationSection section = LogEngineConfigurationSection.GetSection();
            
            if (section != null && section.Loggers != null)
            {
                foreach (LoggerElement loggerEl in section.Loggers)
                {
                    string targetName = loggerEl.Target;
                    FileElement fileEl = null;

                    if (targetName != null && section.Files != null)
                    {
                        fileEl = section.Files[targetName];
                        if (fileEl == null)
                        {
                            continue;
                        }
                    }

                    LogLevel minLevel = GetLogLevelFromConfiguration(loggerEl.MinLevel);
                    IFileWriter fileWriter;
                    if (!this._writers.TryGetValue(targetName, out fileWriter))
                    {
                        NcfLoggerMode writeMode = GetWriteModeFromConfiguration(fileEl.Mode);

                        if (writeMode == NcfLoggerMode.Immediate)
                        {
                            fileWriter = new ImmediateFileWriter(fileEl.FileName);
                        }
                        else
                        {
                            fileWriter = new OptimizedFileWriter(fileEl.FileName);
                        }

                        this._writers.Add(targetName, fileWriter);
                    }

                    NcfLogEngine logger = new NcfLogEngine(minLevel, fileWriter);
                    this._loggers.Add(loggerEl.Name, logger);
                }
            }
        }

        internal ILogEngine GetLogger(string logConfiguration)
        {
            ILogEngine engine;
            if (this._loggers.TryGetValue(logConfiguration, out engine))
            {
                engine.Initialize();
            }

            return engine;
        }

        private static LogLevel GetLogLevelFromConfiguration(string levelStr)
        {
            if (string.IsNullOrWhiteSpace(levelStr))
            {
                return LogLevel.Trace;
            }

            levelStr = levelStr.Trim();

            if (string.Equals(levelStr, "debug", StringComparison.OrdinalIgnoreCase))
            {
                return LogLevel.Debug;
            }
            else if (string.Equals(levelStr, "info", StringComparison.OrdinalIgnoreCase))
            {
                return LogLevel.Info;
            }
            else if (string.Equals(levelStr, "warning", StringComparison.OrdinalIgnoreCase))
            {
                return LogLevel.Warning;
            }
            else if (string.Equals(levelStr, "error", StringComparison.OrdinalIgnoreCase))
            {
                return LogLevel.Error;
            }
            else if (string.Equals(levelStr, "fatal", StringComparison.OrdinalIgnoreCase))
            {
                return LogLevel.Fatal;
            }

            return LogLevel.Trace;
        }

        private static NcfLoggerMode GetWriteModeFromConfiguration(string modeStr)
        {
            if (string.IsNullOrWhiteSpace(modeStr))
            {
                return NcfLoggerMode.Immediate;
            }

            modeStr = modeStr.Trim();

            if (string.Compare(modeStr, "optimized", true) == 0 || string.Compare(modeStr, "asynchronous", true) == 0)
            {
                return NcfLoggerMode.Optimized;
            }

            return NcfLoggerMode.Immediate;
        }
    }
}