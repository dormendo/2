using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Maxima.BookmakerPrototype.Log.Configuration
{
    /// <summary>
    /// Коллекция элементов satellite
    /// </summary>
    [ConfigurationCollection(typeof(LoggerElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class LoggersElementCollection : ConfigurationElementCollection
    {
        #region Статические поля
        
        private static ConfigurationPropertyCollection properties;
        
        #endregion
        
        #region Конструкторы
        
        /// <summary>
        /// Статический конструктор
        /// </summary>
        static LoggersElementCollection()
        {
            properties = new ConfigurationPropertyCollection();
        }
        
        /// <summary>
        /// Конструктор
        /// </summary>
        public LoggersElementCollection()
        {
        }
        
        #endregion
        
        #region Свойства

        /// <summary/>
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        /// <summary/>
        protected override string ElementName
        {
            get
            {
                return LogEngineConfigurationSection.LoggerElementName;
            }
        }
        
        /// <summary/>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return properties;
            }
        }
        
        #endregion

        #region Индексаторы

        /// <summary/>
        public LoggerElement this[int index]
        {
            get
            {
                return (LoggerElement)base.BaseGet(index);
            }
            
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }

                base.BaseAdd(index, value);
            }
        }

        /// <summary/>
        public new LoggerElement this[string name]
        {
            get
            {
                return (LoggerElement)base.BaseGet(name);
            }
        }
        
        #endregion

        #region Методы

        /// <summary/>
        protected override ConfigurationElement CreateNewElement()
        {
            return new LoggerElement();
        }

        /// <summary/>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LoggerElement)element).Name;
        }
        
        #endregion
    }
}
