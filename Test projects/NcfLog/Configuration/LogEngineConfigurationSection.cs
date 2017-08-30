using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Maxima.BookmakerPrototype.Log.Configuration
{
    /// <summary>
    /// Секция logEngine в конфигурации
    /// </summary>
    public class LogEngineConfigurationSection : ConfigurationSection
    {
        #region Константы

        public const string SectionName = "logEngine";

        public const string LoggersElementName = "loggers";

        public const string FilesElementName = "files";

        public const string LoggerElementName = "logger";

        public const string FileElementName = "file";

        #endregion

        #region Статические поля

        private static ConfigurationProperty loggers;

        private static ConfigurationProperty files;

        private static ConfigurationPropertyCollection properties;

        #endregion

        #region Статический конструктор

        /// <summary>
        /// Статический конструктор
        /// </summary>
        static LogEngineConfigurationSection()
        {
            loggers = new ConfigurationProperty(LoggersElementName, typeof(LoggersElementCollection), null);
            files = new ConfigurationProperty(FilesElementName, typeof(FilesElementCollection), null);

            properties = new ConfigurationPropertyCollection();
            properties.Add(loggers);
            properties.Add(files);
        }

        #endregion

        #region Свойства

        /// <summary>
        /// Коллекция данных о конфигурациях журналирования
        /// </summary>
        [ConfigurationProperty(LoggersElementName, IsRequired = false)]
        public LoggersElementCollection Loggers
        {
            get
            {
                return (LoggersElementCollection)base[loggers];
            }
        }

        /// <summary>
        /// Коллекция данных о файлах журналов
        /// </summary>
        [ConfigurationProperty(FilesElementName, IsRequired = false)]
        public FilesElementCollection Files
        {
            get
            {
                return (FilesElementCollection)base[files];
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

        #region Методы

        /// <summary>
        /// Возвращает настройки секции
        /// </summary>
        /// <param name="sectionName">Название секции</param>
        /// <returns>Настройки</returns>
        public static LogEngineConfigurationSection GetSection(string sectionName = SectionName)
        {
            return ConfigurationManager.GetSection(SectionName) as LogEngineConfigurationSection;
        }

        #endregion
    }
}
