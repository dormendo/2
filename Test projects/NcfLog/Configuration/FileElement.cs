using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Maxima.BookmakerPrototype.Log.Configuration
{
    /// <summary>
    /// Конфигурация файлов
    /// </summary>
    public class FileElement : ConfigurationElement
    {
        #region Константы

        private const string NameName = "name";

        private const string FileNameName = "fileName";

        private const string ModeName = "mode";

        #endregion
        
        #region Статические поля

        private static ConfigurationPropertyCollection properties;
        
        #endregion
        
        #region Статический конструктор
        
        /// <summary>
        /// Статический конструктор
        /// </summary>
        static FileElement()
        {
            properties = new ConfigurationPropertyCollection();

            properties.Add(new ConfigurationProperty(NameName, typeof(string), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey));
            properties.Add(new ConfigurationProperty(FileNameName, typeof(string), null, ConfigurationPropertyOptions.IsRequired));
            properties.Add(new ConfigurationProperty(ModeName, typeof(string), null));
        }
        
        #endregion
        
        #region Свойства
        
        /// <summary>
        /// Строковый идентификатор файла журнала
        /// </summary>
        [ConfigurationProperty(NameName, IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return (string)this[NameName];
            }
        }

        /// <summary>
        /// Название файла
        /// </summary>
        [ConfigurationProperty(FileNameName, IsRequired = true)]
        public string FileName
        {
            get
            {
                return (string)this[FileNameName];
            }
        }

        /// <summary>
        /// Режим записи в журнал
        /// </summary>
        [ConfigurationProperty(ModeName)]
        public string Mode
        {
            get
            {
                return (string)this[ModeName];
            }
        }
        
        #endregion
        
        #region Методы

        /// <summary>
        /// Возвращает коллекцию свойств
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return properties;
            }
        }
        
        #endregion
    }
}