using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogEngine
{
    internal class OptimizedFileWriter : IFileWriter
    {
        private string _fileName;

        private ConcurrentQueue<byte[]> Queue = new ConcurrentQueue<byte[]>();

        private CancellationTokenSource _cts = new CancellationTokenSource();

        private Task _cleanupTask;

        internal OptimizedFileWriter(string fileName)
        {
            this._fileName = fileName;
        }

        void IFileWriter.Initialize()
        {
            if (this._cleanupTask == null)
            {
                this._cleanupTask = Task.Factory.StartNew(CleanUp, this._cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
            }
        }

        void IFileWriter.Write(byte[] buffer)
        {
            this.Queue.Enqueue(buffer);
        }

        private void CleanUp()
        {
            DateTime prevIterationStartTime = DateTime.UtcNow;

            using (FileStream fs = new FileStream(this._fileName, FileMode.Append, FileAccess.Write, FileShare.Read, 16 * 1024 * 1024, FileOptions.WriteThrough))
            {
                while (true)
                {
                    bool isCancellationRequested = this._cts.IsCancellationRequested;
                    byte[] msg;
                    bool hasWrittenRecords = false;

                    while (this.Queue.TryDequeue(out msg))
                    {
                        fs.Write(msg, 0, msg.Length);
                        hasWrittenRecords = true;
                    }

                    //Console.WriteLine(this.RecordsWritten);
                    if (!hasWrittenRecords)
                    {
                        fs.Flush();
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }

                    if (this._cts.IsCancellationRequested && isCancellationRequested)
                    {
                        break;
                    }
                }
            }
        }

        public void Dispose()
        {
            this._cts.Cancel();
            this._cleanupTask.Wait();
        }
    }
}
