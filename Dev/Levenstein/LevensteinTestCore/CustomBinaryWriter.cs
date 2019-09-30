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
	/// ���������� BinaryWriter ��� ��������� ������������
	/// </summary>
	public class CustomBinaryWriter : BinaryWriter
	{
		private static BinaryFormatter formatter = new BinaryFormatter();
		
		/// <summary>
		/// �����������
		/// </summary>
		public CustomBinaryWriter(Stream input) : base(input)
		{
		}
		/// <summary>
		/// ���������� Guid �������� � ������� �����.
		/// </summary>
		public void Write(Guid guid)
		{
			this.Write(guid.ToByteArray());
		}
		/// <summary>
		/// ���������� String �������� � ������� �����. �������������� null ��������.
		/// </summary>
		public override void Write(string str)
		{
			if (str == null)
			{
				this.Write(true);
			}
			else
			{
				this.Write(false);
				base.Write(str);
			}
		}
		/// <summary>
		/// ���������� � ������� ����� ��������� �������. �������������� null ��������.
		/// </summary>
		public void WriteObject(object obj)
		{
			if (obj == null)
			{
				this.Write(0);
			}
			else
			{
				using (MemoryStream ms = new MemoryStream())
				{
					formatter.Serialize(ms, obj);
					byte[] bytes = ms.ToArray();
					this.Write(bytes.Length);
					this.Write(bytes);
				}
			}
		}

		/// <summary>
		/// ���������� DataTime � ������� �����
		/// </summary>
		public void Write(DateTime dateTime)
		{
			this.Write(dateTime.ToBinary());
		}
		
		/// <summary>
		/// ���������� int? � ������� �����
		/// </summary>
		public void WriteNullable(int? i)
		{
			this.Write(i.HasValue);
			if (i.HasValue)
			{
				this.Write(i.Value);
			}
		}

		/// <summary>
		/// ���������� bool? � ������� �����
		/// </summary>
		public void WriteNullable(bool? i)
		{
			this.Write(i.HasValue);
			if (i.HasValue)
			{
				this.Write(i.Value);
			}
		}

		/// <summary>
		/// ���������� Guid? � ������� �����
		/// </summary>
		public void WriteNullable(Guid? i)
		{
			this.Write(i.HasValue);
			if (i.HasValue)
			{
				this.Write(i.Value);
			}
		}

		/// <summary>
		/// ���������� DateTime? � ������� �����
		/// </summary>
		public void WriteNullable(DateTime? dt)
		{
			this.Write(dt.HasValue);
			if (dt.HasValue)
			{
				this.Write(dt.Value);
			}
		}

		/// <summary>
		/// ���������� TimeSpan
		/// </summary>
		/// <param name="value"></param>
		public void Write(TimeSpan value)
		{
			this.Write(value.Ticks);
		}

		/// <summary>
		/// ���������� int � ��������� ������������ �� 1 �����
		/// </summary>
		/// <param name="value"></param>
		public new void Write7BitEncodedInt(int value)
		{
			base.Write7BitEncodedInt(value);
		}

		/// <summary>
		/// ���������� ������ ���� � ������ ������
		/// </summary>
		/// <param name="versionMark">����� ������</param>
		public void WriteByteArrayVersionMark(byte[] versionMark)
		{
			this.BaseStream.Write(versionMark, 0, versionMark.Length);
		}

		public void WriteUnicodeString(string s)
		{
			if (s == null)
			{
				this.Write(-1);
			}
			else if (s.Length == 0)
			{
				this.Write(0);
			}
			else
			{
				byte[] bytes = Encoding.Unicode.GetBytes(s);
				this.Write(s.Length);
				this.Write(bytes);
			}
		}
	}
}
