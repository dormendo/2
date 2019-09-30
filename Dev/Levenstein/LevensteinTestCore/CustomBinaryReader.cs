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
	/// ���������� BinaryReader ��� ��������� ������������
	/// </summary>
	public class CustomBinaryReader : BinaryReader
	{
		private static BinaryFormatter formatter = new BinaryFormatter();
		
		/// <summary>
		/// �����������
		/// </summary>
		public CustomBinaryReader(Stream input) : base(input)
		{
		}
		/// <summary>
		/// ��������� Guid �������� � �������� ������.
		/// </summary>
		public Guid ReadGuid()
		{
			return new Guid(this.ReadBytes(16));
		}
		/// <summary>
		/// ��������� String �������� �� �������� ������.
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
		/// ��������� � �������� ������ ������
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
		/// ��������� � �������� ������ ������
		/// </summary>
		public T ReadObject<T>() where T : class
		{
			return this.ReadObject() as T;
		}

		/// <summary>
		/// ��������� DataTime � �������� ������
		/// </summary>
		public DateTime ReadDateTime()
		{
			return DateTime.FromBinary(this.ReadInt64());
		}

		/// <summary>
		/// ��������� int? � �������� ������
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
		/// ���������� bool? 
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
		/// ���������� Guid? 
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
		/// ���������� DateTime? 
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
		/// ���������� TimeSpan
		/// </summary>
		/// <returns></returns>
		public TimeSpan ReadTimeSpan()
		{
			return new TimeSpan(this.ReadInt64());
		}

		/// <summary>
		/// ��������� int � ��������� ������������ �� 1 �����
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
