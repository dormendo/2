using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace LogEngine.Configuration
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

        private static ConfigurationProperty s_loggers;

        private static ConfigurationProperty s_files;

        private static ConfigurationPropertyCollection s_properties;

        #endregion

        #region Статический конструктор

        /// <summary>
        /// Статический конструктор
        /// </summary>
        static LogEngineConfigurationSection()
        {
            s_loggers = new ConfigurationProperty(LoggersElementName, typeof(LoggersElementCollection), null);
            s_files = new ConfigurationProperty(FilesElementName, typeof(FilesElementCollection), null);

            s_properties = new ConfigurationPropertyCollection();
            s_properties.Add(s_loggers);
            s_properties.Add(s_files);
        }

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

        #region Свойства

        /// <summary>
        /// Коллекция данных о конфигурациях журналирования
        /// </summary>
        [ConfigurationProperty(LoggersElementName, IsRequired = false)]
        public LoggersElementCollection Loggers
        {
            get
            {
                return (LoggersElementCollection)base[s_loggers];
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
                return (FilesElementCollection)base[s_files];
            }
        }

        #endregion

        #region Переопределённые члены

        /// <summary/>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return s_properties;
            }
        }

        #endregion
    }
}
