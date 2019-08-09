using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lanit.Norma.AppServer.Cache
{
    /// <summary>
    /// Префиксы ключей кэша
    /// </summary>
    public static class CacheKeysPrefixes
    {
        /// <summary>
        /// StatePagingGetData
        /// </summary>
        public const string StatePagingGetData = "StatePaging.GetData";

        /// <summary>
        /// StatePagingGetDataCounter
        /// </summary>
        public const string StatePagingGetDataCounter = "StatePaging.GetDataCounter";
        
        /// <summary>
        /// DataElementLoadProperties
        /// </summary>
        public const string DataElementLoadProperties = "DataElement.LoadProperties";

        /// <summary>
        /// DataElementLoadExtCols
        /// </summary>
        public const string DataElementLoadExtCols = "DataElement.LoadExtCols";

        /// <summary>
        /// DataElementLoadColLinks
        /// </summary>
        public const string DataElementLoadColLinks = "DataElement.LoadColLinks";

        /// <summary>
        /// DataClassifierLoadProperties
        /// </summary>
        public const string DataClassifierLoadProperties = "DataClassifier.LoadProperties";

        /// <summary>
        /// DataClassifierLoadElements
        /// </summary>
        public const string DataClassifierLoadElements = "DataClassifier.LoadElements";
        
        /// <summary>
        /// DataClassifierLoadColLinks
        /// </summary>
        public const string DataClassifierLoadColLinks = "DataClassifier.LoadColLinks";

        /// <summary>
        /// DataElementLoadAncColLinksColl
        /// </summary>
        public const string DataElementLoadAncColLinksColl = "DataElement.LoadAncColLinksColl";
    }
}
