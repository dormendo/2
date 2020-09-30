using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tips24.PaymentService.SbReg
{
	public class Logger : IDisposable
	{
		private FileStream _fs;

		private string _filePath;

		public Logger(string filePath)
		{
			this._filePath = filePath;
		}

		public async Task WriteLine(int line, string message)
		{
			await this.Write("Строка " + line.ToString() + ": " + message, true);
		}

		public async Task Write(string message, bool newLine)
		{
			this.EnsureFileStreamCreated();

			if (newLine)
			{
				message = message + Environment.NewLine;
			}

			byte[] ba = Encoding.UTF8.GetBytes(message);
			await this._fs.WriteAsync(ba);
			await this._fs.FlushAsync();
		}

		private void EnsureFileStreamCreated()
		{
			if (this._fs == null)
			{
				this._fs = new FileStream(this._filePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read, 128 * 1024, FileOptions.WriteThrough);
				this._fs.Write(Encoding.UTF8.GetPreamble());
			}
		}

		public void Dispose()
		{
			if (this._fs != null)
			{
				this._fs.Dispose();
			}
		}
	}
}
