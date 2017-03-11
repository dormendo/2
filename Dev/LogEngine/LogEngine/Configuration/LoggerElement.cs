using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace LogEngine.Configuration
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

		private static ConfigurationPropertyCollection s_properties;
		
		#endregion
		
		#region Статический конструктор
		
		/// <summary>
		/// Статический конструктор
		/// </summary>
        static LoggerElement()
		{
			s_properties = new ConfigurationPropertyCollection();

			s_properties.Add(new ConfigurationProperty(NameName, typeof(string), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey));
            s_properties.Add(new ConfigurationProperty(MinLevelName, typeof(string), "Trace"));
            s_properties.Add(new ConfigurationProperty(TargetName, typeof(string), null, ConfigurationPropertyOptions.IsRequired));
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
		
		#endregion
		
		#region Переопределённые члены

		/// <summary>
		/// Возвращает коллекцию свойств
		/// </summary>
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