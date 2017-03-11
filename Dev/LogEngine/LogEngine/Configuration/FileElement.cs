using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace LogEngine.Configuration
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
		
		#region Static Fields

		private static ConfigurationPropertyCollection s_properties;
		
		#endregion
		
		#region Статический конструктор
		
		/// <summary>
		/// Статический конструктор
		/// </summary>
		static FileElement()
		{
			s_properties = new ConfigurationPropertyCollection();

			s_properties.Add(new ConfigurationProperty(NameName, typeof(string), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey));
            s_properties.Add(new ConfigurationProperty(FileNameName, typeof(string), null, ConfigurationPropertyOptions.IsRequired));
            s_properties.Add(new ConfigurationProperty(ModeName, typeof(string), null));
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