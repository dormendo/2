using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

namespace Lanit.Norma.AppServer.Cache
{
	/// <summary>
	/// Сериализует процессы и прочие объекты
	/// </summary>
	public static class ProcessSerializer
	{
		/// <summary>
		/// Десериализует объект и приводит его к заданному типу
		/// </summary>
		/// <typeparam name="T">Тип, к которому должен быть приведён десериализованный объект</typeparam>
		/// <param name="ba">Массив байт</param>
		/// <returns>Десериализованный объект</returns>
		public static T GetObjectFromByteArray<T>(byte[] ba)
		{
			return (T)GetObjectFromByteArray(ba);
		}

		/// <summary>
		/// Десериализует объект
		/// </summary>
		/// <param name="ba">Массив байт</param>
		/// <returns>Десериализованный объект</returns>
		public static object GetObjectFromByteArray(byte[] ba)
		{
			BinaryFormatter bf = new BinaryFormatter();

			using (MemoryStream ms = new MemoryStream(ba))
			{
				ms.Position = 0;
				return bf.Deserialize(ms);
			}
		}

		/// <summary>
		/// Сериализует объект
		/// </summary>
		/// <param name="obj">Объект</param>
		/// <returns>Массив байт</returns>
		public static byte[] GetByteArrayFromObject(object obj)
		{
			BinaryFormatter bf = new BinaryFormatter();

			byte[] ba;
			using (MemoryStream ms = new MemoryStream())
			{
				bf.Serialize(ms, obj);
				ba = ms.ToArray();
			}
			return ba;
		}
	}
}
