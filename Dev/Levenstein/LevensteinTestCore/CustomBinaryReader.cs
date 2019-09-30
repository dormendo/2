using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;
using System.Runtime.Serialization;

namespace LevensteinTestCore
{
	/// <summary>
	/// Расширение BinaryReader для кастомной сериализации
	/// </summary>
	public class CustomBinaryReader : BinaryReader
	{
		private static BinaryFormatter formatter = new BinaryFormatter();
		
		/// <summary>
		/// Конструктор
		/// </summary>
		public CustomBinaryReader(Stream input) : base(input)
		{
		}
		/// <summary>
		/// Считывает Guid значение с текущего потока.
		/// </summary>
		public Guid ReadGuid()
		{
			return new Guid(this.ReadBytes(16));
		}
		/// <summary>
		/// Считывает String значение из текущего потока.
		/// </summary>
		public override string ReadString()
		{
			if (this.ReadBoolean())
			{
				return null;
			}
			return base.ReadString();
		}

		/// <summary>
		/// Считывает с текущего потока объект
		/// </summary>
		public object ReadObject()
		{
			int length = this.ReadInt32();
			if (length == 0)
			{
				return null;
			}
			else
			{
				using (MemoryStream ms = new MemoryStream(this.ReadBytes(length)))
				{
					return formatter.Deserialize(ms);
				}
			}
		}

		/// <summary>
		/// Считывает с текущего потока объект
		/// </summary>
		public T ReadObject<T>() where T : class
		{
			return this.ReadObject() as T;
		}

		/// <summary>
		/// Считывает DataTime с текущего потока
		/// </summary>
		public DateTime ReadDateTime()
		{
			return DateTime.FromBinary(this.ReadInt64());
		}

		/// <summary>
		/// Считывает int? с текущего потока
		/// </summary>
		public int? ReadNullableInt32()
		{
			bool hasValue = this.ReadBoolean();
			if (hasValue)
			{
				return this.ReadInt32();
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Зачитывает bool? 
		/// </summary>
		public bool? ReadNullableBoolean()
		{
			bool hasValue = this.ReadBoolean();
			if (hasValue)
			{
				return this.ReadBoolean();
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Зачитывает Guid? 
		/// </summary>
		public Guid? ReadNullableGuid()
		{
			bool hasValue = this.ReadBoolean();
			if (hasValue)
			{
				return this.ReadGuid();
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Зачитывает DateTime? 
		/// </summary>
		public DateTime? ReadNullableDateTime()
		{
			bool hasValue = this.ReadBoolean();
			if (hasValue)
			{
				return this.ReadDateTime();
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Зачитывает TimeSpan
		/// </summary>
		/// <returns></returns>
		public TimeSpan ReadTimeSpan()
		{
			return new TimeSpan(this.ReadInt64());
		}

		/// <summary>
		/// Считывает int с возможной оптимизацией до 1 байта
		/// </summary>
		public new int Read7BitEncodedInt()
		{
			return base.Read7BitEncodedInt();
		}

		public string ReadUnicodeString()
		{
			int len = this.Read();
			if (len < 0)
			{
				return null;
			}
			else if (len == 0)
			{
				return string.Empty;
			}
			else
			{
				byte[] bytes = this.ReadBytes(len * 2);
				return Encoding.Unicode.GetString(bytes);
			}
		}
	}
}
