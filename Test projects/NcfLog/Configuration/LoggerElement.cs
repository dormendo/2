using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Maxima.BookmakerPrototype.Log.Configuration
{
    /// <summary>
    /// Конфигурация логирования
    /// </summary>
    public class LoggerElement : ConfigurationElement
    {
        #region Константы

        private const string NameName = "name";

        private const string MinLevelName = "minLevel";

        private const string TargetName = "target";

        #endregion
        
        #region Static Fields

        private static ConfigurationPropertyCollection properties;
        
        #endregion
        
        #region Статический конструктор
        
        /// <summary>
        /// Статический конструктор
        /// </summary>
        static LoggerElement()
        {
            properties = new ConfigurationPropertyCollection();

            properties.Add(new ConfigurationProperty(NameName, typeof(string), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey));
            properties.Add(new ConfigurationProperty(MinLevelName, typeof(string), "Trace"));
            properties.Add(new ConfigurationProperty(TargetName, typeof(string), null, ConfigurationPropertyOptions.IsRequired));
        }
        
        #endregion
        
        #region Свойства
        
        /// <summary>
        /// Строковый идентификатор конфигурации логирования
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
        /// Минимальный уровень логирования
        /// </summary>
        [ConfigurationProperty(MinLevelName)]
        public string MinLevel
        {
            get
            {
                return (string)this[MinLevelName];
            }
        }

        /// <summary>
        /// Ссылка на конфигурацию файла журнала
        /// </summary>
        [ConfigurationProperty(TargetName, IsRequired = true)]
        public string Target
        {
            get
            {
                return (string)this[TargetName];
            }
        }

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