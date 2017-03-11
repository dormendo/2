using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogEngine
{
    /// <summary>
    /// Режим записи данных в журнал
    /// </summary>
    internal enum NcfLoggerMode
    {
        /// <summary>
        /// Запись в файл происходит непосредственно в момент вызова
        /// </summary>
        Immediate,

        /// <summary>
        /// Запись в файл отложена
        /// </summary>
        Optimized
    }
}
