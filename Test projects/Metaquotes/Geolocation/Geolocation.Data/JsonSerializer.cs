using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geolocation.Data
{
	/// <summary>
	/// Сериализатор в JSON
	/// </summary>
	internal static class JsonSerializer
	{
		internal static readonly byte[] LocationCountryProperty;
		internal static readonly byte[] LocationRegionProperty;
		internal static readonly byte[] LocationPostalProperty;
		internal static readonly byte[] LocationCityProperty;
		internal static readonly byte[] LocationOrganizationProperty;
		internal static readonly byte[] LocationLatitudeProperty;
		internal static readonly byte[] LocationLongitudeProperty;

		private static Encoding _utf8 = Encoding.UTF8;
		private static byte _objectEnd;
		private static byte _arrayStart;
		private static byte _arrayEnd;
		private static byte _comma;

		/// <summary>
		/// Буфер сериализатора
		/// </summary>
		internal class Buffer
		{
			private byte[] _buffer;

			private int _length = 0;

			internal Buffer(int bufferSize)
			{
				_buffer = new byte[bufferSize];
			}

			internal unsafe void Write(byte b)
			{
				_buffer[_length++] = b;
			}

			internal void Write(byte[] bytes)
			{
				System.Buffer.BlockCopy(bytes, 0, _buffer, _length, bytes.Length);
				_length += bytes.Length;
			}

			internal byte[] GetByteArray()
			{
				byte[] result = new byte[_length];
				System.Buffer.BlockCopy(_buffer, 0, result, 0, _length);
				return result;
			}

			internal void Reset()
			{
				_length = 0;
			}
		}

		static JsonSerializer()
		{
			_objectEnd = (byte)'}';
			_arrayStart = (byte)'[';
			_arrayEnd = (byte)']';
			_comma = (byte)',';

			LocationCountryProperty = _utf8.GetBytes("{\"country\":\"");
			LocationRegionProperty = _utf8.GetBytes("\",\"region\":\"");
			LocationPostalProperty = _utf8.GetBytes("\",\"postal\":\"");
			LocationCityProperty = _utf8.GetBytes("\",\"city\":\"");
			LocationOrganizationProperty = _utf8.GetBytes("\",\"organization\":\"");
			LocationLatitudeProperty = _utf8.GetBytes("\",\"latitude\":");
			LocationLongitudeProperty = _utf8.GetBytes(",\"longitude\":");
		}

		internal static Buffer GetBufferForLocation()
		{
			int length =
				LocationCountryProperty.Length +
				LocationRegionProperty.Length +
				LocationPostalProperty.Length +
				LocationCityProperty.Length +
				LocationOrganizationProperty.Length +
				LocationLatitudeProperty.Length +
				LocationLongitudeProperty.Length +
				9 /*latitude*/ + 9/*longitude*/ +
				(8 /*country*/ + 12 /*region*/ + 12 /*postal*/ + 24 /*city*/ + 32 /*organization*/) * 6 /*максимальная длина символа в UTF-8*/;
			return new Buffer(length);
		}

		internal static unsafe void WriteLocationRecord(Buffer buffer, LocationRecord rec)
		{
			buffer.Write(LocationCountryProperty);
			buffer.Write(_utf8.GetBytes(new string(rec.country)));
			buffer.Write(LocationRegionProperty);
			buffer.Write(_utf8.GetBytes(new string(rec.region)));
			buffer.Write(LocationPostalProperty);
			buffer.Write(_utf8.GetBytes(new string(rec.postal)));
			buffer.Write(LocationCityProperty);
			buffer.Write(_utf8.GetBytes(new string(rec.city)));
			buffer.Write(LocationOrganizationProperty);
			buffer.Write(_utf8.GetBytes(new string(rec.organization)));
			buffer.Write(LocationLatitudeProperty);
			buffer.Write(_utf8.GetBytes(rec.latitude.ToString(CultureInfo.InvariantCulture)));
			buffer.Write(LocationLongitudeProperty);
			buffer.Write(_utf8.GetBytes(rec.longitude.ToString(CultureInfo.InvariantCulture)));
			buffer.Write(_objectEnd);
		}

		internal static void StartArray(Buffer buffer)
		{
			buffer.Write(_arrayStart);
		}

		internal static void EndArray(Buffer buffer)
		{
			buffer.Write(_arrayEnd);
		}

		internal static void Comma(Buffer buffer)
		{
			buffer.Write(_comma);
		}
	}
}
