using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maxima.BookmakerPrototype.Log
{
    internal class NLogEngine : ILogEngine
    {
        private NLog.Logger _logger;
        
        internal NLogEngine(string logConfiguration)
        {
            this._logger = NLog.LogManager.GetLogger(logConfiguration);
        }

        void ILogEngine.Initialize()
        {
        }

        public void Log(LogLevel level, string message, params object[] parameters)
        {
            this._logger.Log(ToNLogLevel(level), message, parameters);
        }

        public void LogException(LogLevel level, Exception exception, string message, params object[] parameters)
        {
            NLog.LogLevel nLogLevel = ToNLogLevel(level);
            if (this._logger.IsEnabled(nLogLevel))
            {
                string msg = parameters.Length > 0 ? string.Format(message, parameters) : message;
                this._logger.LogException(nLogLevel, msg, exception);
            }
        }

        void IDisposable.Dispose()
        {
        }

        /// <summary>
        /// Возвращает уровень журналирования, принятый в NLog, по уровню журналирования, принятом в системе
        /// </summary>
        /// <param name="logLevel">Уровень журналирования, принятый в системе</param>
        /// <returns>Уровень журналирования, принятый в NLog</returns>
        private static NLog.LogLevel ToNLogLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return NLog.LogLevel.Trace;
                case LogLevel.Debug:
                    return NLog.LogLevel.Debug;
                case LogLevel.Info:
                    return NLog.LogLevel.Info;
                case LogLevel.Warning:
                    return NLog.LogLevel.Warn;
                case LogLevel.Error:
                    return NLog.LogLevel.Error;
                default:
                    return NLog.LogLevel.Fatal;
            }
        }
    }
}
