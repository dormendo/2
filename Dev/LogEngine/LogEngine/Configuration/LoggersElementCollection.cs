using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace LogEngine.Configuration
{
	/// <summary>
	/// Коллекция элементов satellite
	/// </summary>
	[ConfigurationCollection(typeof(LoggerElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
	public class LoggersElementCollection : ConfigurationElementCollection
	{
		#region Статические поля
		
		private static ConfigurationPropertyCollection s_properties;
		
		#endregion
		
		#region Конструкторы
		
		/// <summary>
		/// Статический конструктор
		/// </summary>
		static LoggersElementCollection()
		{
			s_properties = new ConfigurationPropertyCollection();
		}
		
		/// <summary>
		/// Конструктор
		/// </summary>
        public LoggersElementCollection()
		{
		}
		
		#endregion
		
		#region Properties
		
		/// <summary/>
		protected override ConfigurationPropertyCollection Properties
		{
			get
			{
				return s_properties;
			}
		}
		
		#endregion

		#region Indexers

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

		#region Overrides

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
		
		#endregion
	}
}
