using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestLog
{
    public class MyLog : LogBase, IDisposable
    {
        private class Segment
        {
            public ConcurrentQueue<string> Queue = new ConcurrentQueue<string>();

            public int Count = 0;

            public int RefCount = 0;

            public void Enqueue(string msg)
            {
                Interlocked.Increment(ref this.RefCount);
                try
                {
                    this.Queue.Enqueue(msg);
                    Interlocked.Increment(ref this.Count);
                }
                finally
                {
                    Interlocked.Decrement(ref this.RefCount);
                }
            }

            public override string ToString()
            {
                return string.Format("{0}, {1}, {2}", this.Queue.Count, this.Count, this.RefCount);
            }
        }
        
        private object _currentSegment = new Segment();

        [ThreadStatic]
        private static StringBuilder strBuilder;

        private int _msgCount = 0;

        private string _fileName = "mylog.log";

        private CancellationTokenSource _cts = new CancellationTokenSource();

        private Task _cleanupTask;

        public int RefCount1 = 0;
        public int RefCount2 = 0;
        public int RefCount3 = 0;
        public int RecordsEnqueued = 0;
        public int RecordsWritten = 0;
        public int Exchanges = 0;

        public MyLog()
        {
            File.Delete(this._fileName);
            this._cleanupTask = Task.Factory.StartNew(Cleanup, this._cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public override void Write(string message)
        {
            StringBuilder sb = AcquireStringBuilder();
            DateTime now = DateTime.Now;
            sb.Append(now.Year).Append('.').Append(now.Month).Append('.').Append(now.Day).Append(' ').
                Append(now.Hour).Append(':').Append(now.Minute).Append(':').Append(now.Second).Append('.').Append(now.Millisecond).
                Append(", MESSAGE: ").AppendLine(message);
            this.Enqueue(sb.ToString());
        }

        private void Enqueue(string msg)
        {
            Segment segment = (Segment)Thread.VolatileRead(ref this._currentSegment);
            segment.Enqueue(msg);
            Interlocked.Increment(ref this.RecordsEnqueued);
        }

        private void Cleanup()
        {
            while (true)
            {
                if (!this._cts.IsCancellationRequested)
                {
                    //Thread.Sleep(10);
                }

                Segment oldSegment = (Segment)this._currentSegment;
                if (Thread.VolatileRead(ref oldSegment.Count) > 100 || this._cts.IsCancellationRequested)
                {
                    bool isCancellationRequested = this._cts.IsCancellationRequested;
                    Segment newSegment = new Segment();
                    Thread.VolatileWrite(ref this._currentSegment, newSegment);
                    this.Exchanges++;

                    using (FileStream fs = new FileStream(this._fileName, FileMode.Append, FileAccess.Write, FileShare.None, 1024 * 1024, FileOptions.WriteThrough))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            string msg;
                            while (oldSegment.Queue.TryDequeue(out msg))
                            {
                                sw.WriteLine(msg);
                                this.RecordsWritten++;
                            }

                            if (Thread.VolatileRead(ref oldSegment.RefCount) > 0)
                            {
                                //this.RefCount1++;
                                SpinWait.SpinUntil(() => { return Thread.VolatileRead(ref oldSegment.RefCount) == 0; }, 10);
                                if (Thread.VolatileRead(ref oldSegment.RefCount) > 0)
                                {
                                    //this.RefCount2++;
                                    Thread.Yield();
                                    if (Thread.VolatileRead(ref oldSegment.RefCount) > 0)
                                    {
                                        //this.RefCount3++;
                                        SpinWait.SpinUntil(() => { return Thread.VolatileRead(ref oldSegment.RefCount) == 0; });
                                    }
                                }
                        
                                while (oldSegment.Queue.TryDequeue(out msg))
                                {
                                    sw.WriteLine(msg);
                                    this.RecordsWritten++;
                                }
                            }


                            sw.Flush();
                            fs.Flush();
                        }
                    }

                    if (this._cts.IsCancellationRequested && isCancellationRequested)
                    {
                        break;
                    }
                }
            }
        }

        private static StringBuilder AcquireStringBuilder()
        {
            if (strBuilder == null)
            {
                strBuilder = new StringBuilder(10000);
            }
            else
            {
                strBuilder.Clear();
            }

            return strBuilder;
        }

        public void Dispose()
        {
            this._cts.Cancel();
            try
            {
                this._cleanupTask.Wait();
            }
            catch
            {
            }
        }
    }
}
