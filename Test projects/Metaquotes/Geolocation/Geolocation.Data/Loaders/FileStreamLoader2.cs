using System;
using System.IO;

namespace Geolocation.Data
{
	/// <summary>
	/// Версия FileStreamLoader, не приводящая к фрагментации LOH
	/// Вычитывает файл блоками по 64кб, данными из которых инициализирует структуры данных.
	/// Наиболее производительная версия загрузчика
	/// </summary>
	internal class FileStreamLoader2 : IDbLoader
	{
		/// <summary>
		/// Обеспечивает инициализацию блока памяти данными из файла.
		/// Данные вычитываются последовательно и по необходимости блоками заданного размера
		/// </summary>
		private class Reader
		{
			private FileStream _fs;
			private byte[] _buffer;
			private int _bufferPosition;
			private int _dataLength;

			internal Reader(FileStream fs, int bufferLength)
			{
				_fs = fs;
				_buffer = new byte[bufferLength];
			}

			internal unsafe bool ReadBytes(byte* destination, int length)
			{
				int bytesCopied = 0;

				while (true)
				{
					if (_dataLength == 0 || _bufferPosition == _dataLength)
					{
						int bytesRead = _fs.Read(_buffer, 0, _buffer.Length);
						if (bytesRead == 0)
						{
							return false;
						}

						_dataLength = bytesRead;
						_bufferPosition = 0;
					}

					int bytesToCopy = Math.Min(length - bytesCopied, _dataLength - _bufferPosition);
					fixed (byte* bufferPtr = &_buffer[_bufferPosition])
					{
						Utils.CopyMemory(destination + bytesCopied, bufferPtr, bytesToCopy);
					}

					_bufferPosition += bytesToCopy;
					bytesCopied += bytesToCopy;

					if (bytesCopied == length)
					{
						return true;
					}
				}
			}
		}

		/// <summary>
		/// Инициализирует структуры данных информацией из файла БД
		/// </summary>
		/// <param name="fileName">Путь к файлу БД</param>
		/// <param name="holder">Хранилище структур данных БД</param>
		public void Load(string fileName, DataHolder holder)
		{
			using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read,
				FileShare.None, 64 * 1024, FileOptions.SequentialScan))
			{
				Reader reader = new Reader(fs, 64 * 1024);
				if (!ReadFile(reader, holder))
				{
					throw new Exception();
				}
			}
		}

		private static unsafe bool ReadFile(Reader reader, DataHolder holder)
		{
			int blockSize = sizeof(DatabaseFileHeader);
			fixed (DatabaseFileHeader* headerPtr = &holder.Header)
			{
				if (!reader.ReadBytes((byte*)headerPtr, blockSize))
				{
					return false;
				}
			}

			holder.IpTable = new IpRecord[holder.Header.records];
			holder.LocationTable = new LocationRecord[holder.Header.records];
			holder.LocationByNameIndex = new int[holder.Header.records];

			blockSize = sizeof(IpRecord) * holder.Header.records;
			fixed (IpRecord* ipPtr = &holder.IpTable[0])
			{
				if (!reader.ReadBytes((byte*)ipPtr, blockSize))
				{
					return false;
				}
			}

			blockSize = sizeof(LocationRecord) * holder.Header.records;
			fixed (LocationRecord* locationPtr = &holder.LocationTable[0])
			{
				if (!reader.ReadBytes((byte*)locationPtr, blockSize))
				{
					return false;
				}
			}

			blockSize = sizeof(int) * holder.Header.records;
			fixed (int* indexPtr = &holder.LocationByNameIndex[0])
			{
				return reader.ReadBytes((byte*)indexPtr, blockSize);
			}
		}
	}
}
