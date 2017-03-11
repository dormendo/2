using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace TestLog
{
    public class NLogLog : LogBase
    {
        #region LogLevel и LogType
        
        /// <summary>
        /// Тип журнала
        /// </summary>
        public enum LogType : int
        {
            /// <summary>
            /// Системный журнал, не связанный с действиями пользователя
            /// </summary>
            System = 0,

            /// <summary>
            /// Журнал операций
            /// </summary>
            Operations = 1
        }

        /// <summary>
        /// Уровни журналирования
        /// </summary>
        public enum LogLevel : byte
        {
            /// <summary>
            /// Трассировочная информация. Максимально информативный уровень журналирования
            /// </summary>
            Trace = 0,

            /// <summary>
            /// Отладочная информация. Второй по информативности уровень журналирования
            /// </summary>
            Debug = 1,

            /// <summary>
            /// Информационные сообщения. Третий по информативности уровень журналирования
            /// </summary>
            Info = 2,

            /// <summary>
            /// Предупреждения. Четвёртый по информативности уровень журналирования
            /// </summary>
            Warning = 3,

            /// <summary>
            /// Ошибки. Пятый по информативности уровень журналирования
            /// </summary>
            Error = 4,

            /// <summary>
            /// Ошибки, не позволяющие приложению корректно работать. Минимально информативный уровень журналирования
            /// </summary>
            Fatal = 5
        }

        #endregion
        
        #region Logger

        /// <summary>
        /// Обеспечивает работу с журналами
        /// </summary>
        private static class Logger
        {
            /// <summary>
            /// Коллекция компонентов журналирования
            /// </summary>
            private static readonly NLog.Logger[] Loggers = new NLog.Logger[1];

            /// <summary>
            /// Статический конструктор
            /// </summary>
            static Logger()
            {
                System.IO.File.Delete("nlog.log");
                RegisterLog2(LogType.System, "SystemDev");
            }

            /// <summary>
            /// Регистрирует конфигурацию логирования. Системный журнал регистрируется автоматически и не требует дополнительных действий
            /// </summary>
            /// <param name="type">Тип журнала</param>
            /// <param name="logConfiguration">Название конфигурации</param>
            public static void RegisterLog(LogType type, string logConfiguration)
            {
                if (type != LogType.System)
                {
                    RegisterLog2(type, logConfiguration);
                }
            }

            /// <summary>
            /// Записывает в журнал сообщение
            /// </summary>
            /// <param name="type">Тип журнала</param>
            /// <param name="level">Уровень сообщения</param>
            /// <param name="message">Сообщение</param>
            /// <param name="parameters">Параметры формирования сообщения</param>
            public static void Log(LogType type, LogLevel level, string message, params object[] parameters)
            {
                Log(type, ToNLogLevel(level), message, parameters);
            }

            /// <summary>
            /// Записывает в журнал сообщение об исключении
            /// </summary>
            /// <param name="type">Тип журнала</param>
            /// <param name="level">Уровень сообщения</param>
            /// <param name="exception">Исключение</param>
            /// <param name="message">Сообщение</param>
            /// <param name="parameters">Параметры формирования сообщения</param>
            public static void LogException(LogType type, LogLevel level, Exception exception, string message, params object[] parameters)
            {
                LogException(type, ToNLogLevel(level), exception, message, parameters);
            }

            /// <summary>
            /// Записывает в журнал сообщение. Уровень Trace (1)
            /// </summary>
            /// <param name="message">Сообщение</param>
            /// <param name="type">Тип журнала</param>
            public static void Trace(string message, LogType type = LogType.Operations)
            {
                Log(type, NLog.LogLevel.Trace, message);
            }

            /// <summary>
            /// Записывает в журнал исключение. Уровень Trace (1)
            /// </summary>
            /// <param name="message">Сообщение</param>
            /// <param name="exception">Исключение</param>
            /// <param name="type">Тип журнала</param>
            public static void TraceException(string message, Exception exception, LogType type = LogType.Operations)
            {
                LogException(type, NLog.LogLevel.Trace, exception, message);
            }

            /// <summary>
            /// Записывает в журнал сообщение. Уровень Debug (2)
            /// </summary>
            /// <param name="message">Сообщение</param>
            /// <param name="type">Тип журнала</param>
            public static void Debug(string message, LogType type = LogType.Operations)
            {
                Log(type, NLog.LogLevel.Debug, message);
            }

            /// <summary>
            /// Записывает в журнал исключение. Уровень Debug (2)
            /// </summary>
            /// <param name="message">Сообщение</param>
            /// <param name="exception">Исключение</param>
            /// <param name="type">Тип журнала</param>
            public static void DebugException(string message, Exception exception, LogType type = LogType.Operations)
            {
                LogException(type, NLog.LogLevel.Debug, exception, message);
            }

            /// <summary>
            /// Записывает в журнал сообщение. Уровень Info (3)
            /// </summary>
            /// <param name="message">Сообщение</param>
            /// <param name="type">Тип журнала</param>
            public static void Info(string message, LogType type = LogType.Operations)
            {
                Log(type, NLog.LogLevel.Info, message);
            }

            /// <summary>
            /// Записывает в журнал исключение. Уровень Info (3)
            /// </summary>
            /// <param name="message">Сообщение</param>
            /// <param name="exception">Исключение</param>
            /// <param name="type">Тип журнала</param>
            public static void InfoException(string message, Exception exception, LogType type = LogType.Operations)
            {
                LogException(type, NLog.LogLevel.Info, exception, message);
            }

            /// <summary>
            /// Записывает в журнал сообщение. Уровень Warning (4)
            /// </summary>
            /// <param name="message">Сообщение</param>
            /// <param name="type">Тип журнала</param>
            public static void Warning(string message, LogType type = LogType.Operations)
            {
                Log(type, NLog.LogLevel.Warn, message);
            }

            /// <summary>
            /// Записывает в журнал исключение. Уровень Warning (4)
            /// </summary>
            /// <param name="message">Сообщение</param>
            /// <param name="exception">Исключение</param>
            /// <param name="type">Тип журнала</param>
            public static void WarningException(string message, Exception exception, LogType type = LogType.Operations)
            {
                LogException(type, NLog.LogLevel.Warn, exception, message);
            }

            /// <summary>
            /// Записывает в журнал сообщение. Уровень Error (5)
            /// </summary>
            /// <param name="message">Сообщение</param>
            /// <param name="type">Тип журнала</param>
            /// <summary>
            public static void Error(string message, LogType type = LogType.Operations)
            {
                Log(type, NLog.LogLevel.Error, message);
            }

            /// <summary>
            /// Записывает в журнал исключение. Уровень Error (5)
            /// </summary>
            /// <param name="message">Сообщение</param>
            /// <param name="exception">Исключение</param>
            /// <param name="type">Тип журнала</param>
            public static void ErrorException(string message, Exception exception, LogType type = LogType.Operations)
            {
                LogException(type, NLog.LogLevel.Error, exception, message);
            }

            /// <summary>
            /// Записывает в журнал сообщение. Уровень Fatal (6)
            /// </summary>
            /// <param name="message">Сообщение</param>
            /// <param name="type">Тип журнала</param>
            public static void Fatal(string message, LogType type = LogType.Operations)
            {
                Log(type, NLog.LogLevel.Fatal, message);
            }

            /// <summary>
            /// Записывает в журнал исключение. Уровень Fatal (6)
            /// </summary>
            /// <param name="message">Сообщение</param>
            /// <param name="exception">Исключение</param>
            /// <param name="type">Тип журнала</param>
            public static void FatalException(string message, Exception exception, LogType type = LogType.Operations)
            {
                LogException(type, NLog.LogLevel.Fatal, exception, message);
            }

            /// <summary>
            /// Записывает в журнал сообщение
            /// </summary>
            /// <param name="type">Тип журнала</param>
            /// <param name="level">Уровень сообщения</param>
            /// <param name="message">Сообщение</param>
            /// <param name="parameters">Параметры формирования сообщения</param>
            private static void Log(LogType type, NLog.LogLevel level, string message, params object[] parameters)
            {
                try
                {
                    NLog.Logger logger = GetLogger(type);
                    if (logger == null)
                    {
                        return;
                    }

                    if (logger.IsEnabled(level))
                    {
                        string msg = parameters.Length > 0 ? string.Format(message, parameters) : message;
                        logger.Log(level, msg);
                    }
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
            private static void LogException(LogType type, NLog.LogLevel level, Exception exception, string message, params object[] parameters)
            {
                NLog.Logger logger = GetLogger(type);
                if (logger == null)
                {
                    return;
                }

                if (logger.IsEnabled(level))
                {
                    string msg = parameters.Length > 0 ? string.Format(message, parameters) : message;
                    logger.LogException(level, msg, exception);
                }
            }

            /// <summary>
            /// Регистрирует конфигурацию логирования
            /// </summary>
            /// <param name="type">Тип журнала</param>
            /// <param name="logConfiguration">Название конфигурации</param>
            private static void RegisterLog2(LogType type, string logConfiguration)
            {
                Loggers[(int)type] = NLog.LogManager.GetLogger(logConfiguration);
            }

            /// <summary>
            /// Возвращает компонент журналирования по типу журнала
            /// </summary>
            /// <param name="type">Тип журнала</param>
            /// <returns>Компонент журналирования</returns>
            private static NLog.Logger GetLogger(LogType type)
            {
                return Loggers[(int)type];
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

        #endregion

        public override void Write(string message)
        {
            Logger.Error(message, LogType.System);
        }
    }
}
