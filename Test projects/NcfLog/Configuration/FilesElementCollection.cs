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
    [ConfigurationCollection(typeof(FileElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class FilesElementCollection : ConfigurationElementCollection
    {
        #region Статические поля
        
        private static ConfigurationPropertyCollection properties;
        
        #endregion
        
        #region Конструкторы
        
        /// <summary>
        /// Статический конструктор
        /// </summary>
        static FilesElementCollection()
        {
            properties = new ConfigurationPropertyCollection();
        }
        
        /// <summary>
        /// Конструктор
        /// </summary>
        public FilesElementCollection()
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
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return properties;
            }
        }

        /// <summary/>
        protected override string ElementName
        {
            get
            {
                return LogEngineConfigurationSection.FileElementName;
            }
        }
        
        #endregion

        #region Индексаторы

        /// <summary/>
        public FileElement this[int index]
        {
            get
            {
                return (FileElement)base.BaseGet(index);
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
        public new FileElement this[string name]
        {
            get
            {
                return (FileElement)base.BaseGet(name);
            }
        }
        
        #endregion

        #region Методы

        /// <summary/>
        protected override ConfigurationElement CreateNewElement()
        {
            return new FileElement();
        }

        /// <summary/>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FileElement)element).Name;
        }
        
        #endregion
    }
}
