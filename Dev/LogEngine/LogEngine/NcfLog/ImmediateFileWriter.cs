using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogEngine
{
    internal class ImmediateFileWriter : IFileWriter
    {
        private string _fileName;

        private FileStream _fs;
        
        private object _writeLock = new object();

        internal ImmediateFileWriter(string fileName)
        {
            this._fileName = fileName;
        }

        void IFileWriter.Initialize()
        {
            if (this._fs == null)
            {
                lock (this._writeLock)
                {
                    if (this._fs == null)
                    {
                        this._fs = new FileStream(this._fileName, FileMode.Append, FileAccess.Write, FileShare.Read, 64 * 1024);
                    }
                }
            }
        }

        void IFileWriter.Write(byte[] msg)
        {
            if (this._fs != null)
            {
                lock (this._writeLock)
                {
                    if (this._fs != null)
                    {
                        this._fs.Write(msg, 0, msg.Length);
                        this._fs.Flush();
                    }
                }
            }
        }

        public void Dispose()
        {
            if (this._fs != null)
            {
                lock (this._writeLock)
                {
                    if (this._fs != null)
                    {
                        this._fs.Dispose();
                        this._fs = null;
                    }
                }
            }
        }
    }
}
