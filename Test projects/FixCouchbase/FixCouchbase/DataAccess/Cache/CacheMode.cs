using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lanit.Norma.AppServer.Cache
{
    /// <summary>
    /// Режим работы сервера
    /// </summary>
    public enum CacheMode
    {
        /// <summary>
        /// Выключен
        /// </summary>
        None,

        /// <summary>
        /// Кэш Couchbase
        /// </summary>
        Couchbase,

        /// <summary>
        /// Внутрениий
        /// </summary>
        InProcess
    }
}
