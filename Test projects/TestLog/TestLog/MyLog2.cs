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
    public class MyLog2 : LogBase, IDisposable
    {
        private class CharBuffer : ICacheItemCleanUp
        {
            public char[] Buffer;

            public int Size;

            public CharBuffer()
            {
                this.Buffer = new char[10000];
                this.Size = 0;
            }

            public unsafe CharBuffer Append(char[] src, int length)
            {
                fixed (char* smem = src)
                fixed (char* dmem = &this.Buffer[this.Size])
                    MyBuilder.wstrcpy(dmem, smem, length);
                this.Size += length;
                return this;
            }

            public void CleanUp()
            {
                this.Size = 0;
            }
        }

        private class Segment
        {
            public ConcurrentQueue<CharBuffer> Queue = new ConcurrentQueue<CharBuffer>();

            public int Count = 0;

            public int RefCount = 0;

            public void Enqueue(CharBuffer msg)
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
        private static MyBuilder strBuilder;

        private int _msgCount = 0;

        private string _fileName = "mylog2.log";

        private CancellationTokenSource _cts = new CancellationTokenSource();

        private CacheQueue<CharBuffer> _bufferCache;

        private Task _cleanupTask;

        public int RefCount1 = 0;
        public int RefCount2 = 0;
        public int RefCount3 = 0;
        public int RecordsEnqueued = 0;
        public int RecordsWritten = 0;
        public int Exchanges = 0;

        public MyLog2()
        {
            File.Delete(this._fileName);
            this._bufferCache = new CacheQueue<CharBuffer>(3500);
            this._cleanupTask = Task.Factory.StartNew(Cleanup, this._cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public unsafe override void Write(string message)
        {
            MyBuilder builder = AcquireStringBuilder();
            DateTime now = DateTime.Now;
            builder.Append(now.Year).Append('.').Append(now.Month).Append('.').Append(now.Day).Append(' ').
                Append(now.Hour).Append(':').Append(now.Minute).Append(':').Append(now.Second).Append('.').Append(now.Millisecond).
                Append(", MESSAGE: ").AppendLine(message);
            this.Enqueue(builder);
        }

        private CharBuffer CreateNewBuffer()
        {
            return new CharBuffer();
        }

        private void Enqueue(MyBuilder builder)
        {
            Segment segment = (Segment)Thread.VolatileRead(ref this._currentSegment);
            CharBuffer buffer = this._bufferCache.AcquireCacheItem();
            buffer.Append(builder.GetArray(), builder.Length);
            segment.Enqueue(buffer);
            Interlocked.Increment(ref this.RecordsEnqueued);
        }

        private void Cleanup()
        {
            while (true)
            {
                if (!this._cts.IsCancellationRequested)
                {
                    Thread.Sleep(10);
                }

                Segment oldSegment = (Segment)this._currentSegment;
                if (Thread.VolatileRead(ref oldSegment.Count) > 100 || this._cts.IsCancellationRequested)
                {
                    bool isCancellationRequested = this._cts.IsCancellationRequested;
                    Segment newSegment = new Segment();
                    Thread.VolatileWrite(ref this._currentSegment, newSegment);
                    this.Exchanges++;

                    using (StreamWriter sw = new StreamWriter(this._fileName, true, Encoding.UTF8, 1024 * 1024))
                    {
                        CharBuffer buffer;
                        while (oldSegment.Queue.TryDequeue(out buffer))
                        {
                            sw.WriteLine(buffer.Buffer, 0, buffer.Size);
                            this._bufferCache.ReleaseCacheItem(buffer);
                            Interlocked.Increment(ref this.RecordsWritten);
                        }

                        if (Thread.VolatileRead(ref oldSegment.RefCount) > 0)
                        {
                            this.RefCount1++;
                            SpinWait.SpinUntil(() => { return Thread.VolatileRead(ref oldSegment.RefCount) == 0; }, 10);
                            if (Thread.VolatileRead(ref oldSegment.RefCount) > 0)
                            {
                                this.RefCount2++;
                                Thread.Yield();
                                if (Thread.VolatileRead(ref oldSegment.RefCount) > 0)
                                {
                                    this.RefCount3++;
                                    SpinWait.SpinUntil(() => { return Thread.VolatileRead(ref oldSegment.RefCount) == 0; });
                                }
                            }
                        
                            while (oldSegment.Queue.TryDequeue(out buffer))
                            {
                                sw.WriteLine(buffer.Buffer, 0, buffer.Size);
                                this._bufferCache.ReleaseCacheItem(buffer);
                                Interlocked.Increment(ref this.RecordsWritten);
                            }
                        }

                        sw.Flush();

                    }

                    if (this._cts.IsCancellationRequested && isCancellationRequested)
                    {
                        break;
                    }
                }
            }
        }

        private static MyBuilder AcquireStringBuilder()
        {
            if (strBuilder == null)
            {
                strBuilder = new MyBuilder(10000);
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

        internal int GetBufferCacheSize()
        {
            return this._bufferCache.GetSize();
        }
    }
}
