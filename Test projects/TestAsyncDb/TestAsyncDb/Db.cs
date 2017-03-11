using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestAsyncDb
{
	public class Db
	{
		private const string connStr = "Server=localhost; Initial catalog=test; Integrated security=true; Pooling=true; Min pool size=200; Max pool size=200";

		private const string sql = "WAITFOR DELAY '00:00:00.003'"; 

		private static int count = 0;

		public static void Run()
		{
			using (SqlConnection conn = new SqlConnection(connStr))
			{
				conn.Open();
				using (SqlCommand cmd = new SqlCommand(sql, conn))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static async Task RunAsyncAwait()
		{
			//int c = Interlocked.Increment(ref count);
			//Console.WriteLine(c);
			using (SqlConnection conn = new SqlConnection(connStr))
			{
				conn.Open();
				using (SqlCommand cmd = new SqlCommand(sql, conn))
				{
					await cmd.ExecuteNonQueryAsync();
				}
			}
			//Console.WriteLine("Completed {0}", c);
		}

        private class IarState : IDisposable
        {
            public SqlConnection Connection;

            public SqlCommand Command;

            public void Dispose()
            {
                if (this.Command != null)
                {
                    this.Command.Dispose();
                    this.Command = null;
                }
                if (this.Connection != null)
                {
                    this.Connection.Dispose();
                    this.Connection = null;
                }
            }
        }

        private class AsyncResult : IAsyncResult
        {
            private bool _completedSynchronously;
            private bool _isCompleted;
            private ManualResetEvent _event = new ManualResetEvent(false);
            
            public object AsyncState
            {
                get
                {
                    return null;
                }
            }

            public WaitHandle AsyncWaitHandle
            {
                get { throw new NotImplementedException(); }
            }

            public bool CompletedSynchronously
            {
                get
                {
                    return this._completedSynchronously;
                }
            }

            public bool IsCompleted
            {
                get
                {
                    return this._isCompleted;
                }
            }

            public void SetCompletedSynchronously()
            {
                this._completedSynchronously = true;
                this.SetIsCompleted();
            }

            public void SetIsCompleted()
            {
                this._isCompleted = true;
                this._event.Set();
            }
        }

        public static void BeginRunIar()
        {
            IarState state = new IarState();
            try
            {
                state.Connection = new SqlConnection(connStr);
                state.Connection.Open();
                state.Command = new SqlCommand(sql, state.Connection);
                IAsyncResult iar = state.Command.BeginExecuteNonQuery(ContinueIar, state);
                if (iar.CompletedSynchronously)
                {
                    state.Command.EndExecuteNonQuery(iar);
                    state.Dispose();
                }
            }
            catch
            {
                state.Dispose();
            }
        }

        public static void ContinueIar(IAsyncResult iar)
        {
            IarState state = (IarState)iar.AsyncState;
            try
            {
                state.Command.EndExecuteNonQuery(iar);
            }
            finally
            {
                state.Dispose();
            }
        }

        public static void EndRunIar(IAsyncResult iar)
        {
        }
	}
}
