using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maxima.BookmakerPrototype.Log
{
    internal interface ILogEngine : IDisposable
    {
        void Initialize();
        
        /// <summary>
        /// Записывает в журнал сообщение
        /// </summary>
        /// <param name="level">Уровень сообщения</param>
        /// <param name="message">Сообщение</param>
        /// <param name="parameters">Параметры формирования сообщения</param>
        void Log(LogLevel level, string message, params object[] parameters);

        /// <summary>
        /// Записывает в журнал сообщение об исключении
        /// </summary>
        /// <param name="level">Уровень сообщения</param>
        /// <param name="exception">Исключение</param>
        /// <param name="message">Сообщение</param>
        /// <param name="parameters">Параметры формирования сообщения</param>
        void LogException(LogLevel level, Exception exception, string message, params object[] parameters);
    }
}
