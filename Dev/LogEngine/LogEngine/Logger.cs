using System;

namespace LogEngine
{
    /// <summary>
    /// Обеспечивает работу с журналами
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Коллекция компонентов журналирования
        /// </summary>
        private static readonly ILogEngine[] Loggers = new ILogEngine[3];

        private static readonly NcfLogEngineProvider _le;

        static Logger()
        {
            _le = new NcfLogEngineProvider();
            _le.Initialize();
        }

        /// <summary>
        /// Регистрирует конфигурацию логирования. Системный журнал регистрируется автоматически и не требует дополнительных действий
        /// </summary>
        /// <param name="type">Тип журнала</param>
        /// <param name="logConfiguration">Название конфигурации</param>
        public static void RegisterLog(LogType type, string logConfiguration)
        {
            ILogEngine logger = _le.GetLogger(logConfiguration);
            if (logger == null)
            {
                logger = new NLogEngine(logConfiguration);
            }

            Loggers[(int)type] = logger;
        }

        /// <summary>
        /// Записывает в журнал сообщение. Уровень Trace (1)
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="type">Тип журнала</param>
        public static void Trace(string message, LogType type = LogType.Operations)
        {
            Log(type, LogLevel.Trace, message);
        }

        /// <summary>
        /// Записывает в журнал исключение. Уровень Trace (1)
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="exception">Исключение</param>
        /// <param name="type">Тип журнала</param>
        public static void TraceException(string message, Exception exception, LogType type = LogType.Operations)
        {
            LogException(type, LogLevel.Trace, exception, message);
        }

        /// <summary>
        /// Записывает в журнал сообщение. Уровень Debug (2)
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="type">Тип журнала</param>
        public static void Debug(string message, LogType type = LogType.Operations)
        {
            Log(type, LogLevel.Debug, message);
        }

        /// <summary>
        /// Записывает в журнал исключение. Уровень Debug (2)
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="exception">Исключение</param>
        /// <param name="type">Тип журнала</param>
        public static void DebugException(string message, Exception exception, LogType type = LogType.Operations)
        {
            LogException(type, LogLevel.Debug, exception, message);
        }

        /// <summary>
        /// Записывает в журнал сообщение. Уровень Info (3)
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="type">Тип журнала</param>
        public static void Info(string message, LogType type = LogType.Operations)
        {
            Log(type, LogLevel.Info, message);
        }

        /// <summary>
        /// Записывает в журнал исключение. Уровень Info (3)
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="exception">Исключение</param>
        /// <param name="type">Тип журнала</param>
        public static void InfoException(string message, Exception exception, LogType type = LogType.Operations)
        {
            LogException(type, LogLevel.Info, exception, message);
        }

        /// <summary>
        /// Записывает в журнал сообщение. Уровень Warning (4)
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="type">Тип журнала</param>
        public static void Warning(string message, LogType type = LogType.Operations)
        {
            Log(type, LogLevel.Warning, message);
        }

        /// <summary>
        /// Записывает в журнал исключение. Уровень Warning (4)
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="exception">Исключение</param>
        /// <param name="type">Тип журнала</param>
        public static void WarningException(string message, Exception exception, LogType type = LogType.Operations)
        {
            LogException(type, LogLevel.Warning, exception, message);
        }

        /// <summary>
        /// Записывает в журнал сообщение. Уровень Error (5)
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="type">Тип журнала</param>
        /// <summary>
        public static void Error(string message, LogType type = LogType.Operations)
        {
            Log(type, LogLevel.Error, message);
        }

        /// <summary>
        /// Записывает в журнал исключение. Уровень Error (5)
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="exception">Исключение</param>
        /// <param name="type">Тип журнала</param>
        public static void ErrorException(string message, Exception exception, LogType type = LogType.Operations)
        {
            LogException(type, LogLevel.Error, exception, message);
        }

        /// <summary>
        /// Записывает в журнал сообщение. Уровень Fatal (6)
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="type">Тип журнала</param>
        public static void Fatal(string message, LogType type = LogType.Operations)
        {
            Log(type, LogLevel.Fatal, message);
        }

        /// <summary>
        /// Записывает в журнал исключение. Уровень Fatal (6)
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="exception">Исключение</param>
        /// <param name="type">Тип журнала</param>
        public static void FatalException(string message, Exception exception, LogType type = LogType.Operations)
        {
            LogException(type, LogLevel.Fatal, exception, message);
        }

        /// <summary>
        /// Записывает в журнал сообщение
        /// </summary>
        /// <param name="type">Тип журнала</param>
        /// <param name="level">Уровень сообщения</param>
        /// <param name="message">Сообщение</param>
        /// <param name="parameters">Параметры формирования сообщения</param>
        private static void Log(LogType type, LogLevel level, string message, params object[] parameters)
        {
            try
            {
                ILogEngine logger = GetLogger(type);
                if (logger == null)
                {
                    return;
                }

                logger.Log(level, message, parameters);
            }
            catch (Exception le)
            {
                if (type != LogType.System)
                {
                    Logger.LogException(LogType.System, LogLevel.Error, le, "Ошибка при записи в журнал {0}", type);
                }

                throw;
            }
        }

        /// <summary>
        /// Записывает в журнал сообщение об исключении
        /// </summary>
        /// <param name="type">Тип журнала</param>
        /// <param name="level">Уровень сообщения</param>
        /// <param name="exception">Исключение</param>
        /// <param name="message">Сообщение</param>
        /// <param name="parameters">Параметры формирования сообщения</param>
        private static void LogException(LogType type, LogLevel level, Exception exception, string message, params object[] parameters)
        {
            try
            {
                ILogEngine logger = GetLogger(type);
                if (logger == null)
                {
                    return;
                }

                logger.LogException(level, exception, message, parameters);
            }
            catch (Exception le)
            {
                if (type != LogType.System)
                {
                    Logger.LogException(LogType.System, LogLevel.Error, le, "Ошибка при записи в журнал {0}", type);
                }

                throw;
            }
        }

        /// <summary>
        /// Возвращает компонент журналирования по типу журнала
        /// </summary>
        /// <param name="type">Тип журнала</param>
        /// <returns>Компонент журналирования</returns>
        private static ILogEngine GetLogger(LogType type)
        {
            return Loggers[(int)type];
        }

        public static void Stop()
        {
            foreach (ILogEngine engine in Loggers)
            {
                if (engine != null)
                {
                    engine.Dispose();
                }
            }
        }
    }
}
