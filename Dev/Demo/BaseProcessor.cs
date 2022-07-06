using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Serilog;

namespace GisPaService
{
	public abstract class BaseProcessor<T, K>
	{
		protected readonly IConfiguration _configuration;
		protected readonly Request _request;
		protected readonly string _connectionString;

		protected int _runtimeProcesedCount;
		protected int _repeatIfErrorCount;
		protected abstract BaseDataSaver<T> _dataSaver { get; }
		
		protected volatile int _errors = 0;
		protected int progress = 0;
		
		private ConcurrentQueue<K> _itemsToParse = new ConcurrentQueue<K>();
		protected BlockingCollection<K> _itemsToParseManager;

		private ConcurrentBag<T> _itemsToSave = new ConcurrentBag<T>();
		protected BlockingCollection<T> _itemsToSaveManager;
		protected string _tablespace;

		public BaseProcessor(IConfiguration configuration, Request request)
		{
			_configuration = configuration;
			_request = request;
			_connectionString = _configuration.GetConnectionString("Db");
			
			int threadCount = _configuration.GetValue("GisPa:ThreadCount", 100);
			if (_request.ThreadCount == 0)
			{
				_request.ThreadCount = threadCount;
			}
			
			_tablespace = _configuration.GetValue("GisPa:Tablespace", "");

			_runtimeProcesedCount = _configuration.GetValue("GisPa:RuntimeProcesedCount", 1000);
			_repeatIfErrorCount = _configuration.GetValue("GisPa:RepeatIfErrorCount", 1);
			
			SynchronizationContext.SetSynchronizationContext(null);
			ThreadPool.SetMinThreads(_request.ThreadCount + 10, _request.ThreadCount + 10);
		}
		
		public string Execute()
		{
			_dataSaver.CreateTable();

			int repeatCount = 0;
			do
			{
				_errors = 0;
				if (repeatCount > 0)
				{
					Log.Debug($"repeat {repeatCount}");
				}

				RunIteration();
				repeatCount++;
			}
			while (_errors > 0 && repeatCount <= _repeatIfErrorCount);

			Log.Debug($"table: {_dataSaver.TableName}, iterations: {progress}, errors: {_errors}");
			
			return _dataSaver.TableName;
		}
		
		private void RunIteration()
		{
			bool firstRow = true;
			string cmdSql = _dataSaver.GetCmdSql();

			Task[] parseTasksToWait = null;
			Task saveTaskToWait = null;
			using (_itemsToParseManager = new BlockingCollection<K>(_itemsToParse, _runtimeProcesedCount))
			using (_itemsToSaveManager = new BlockingCollection<T>(_itemsToSave, _runtimeProcesedCount))
			{
				try
				{
					using (NpgsqlConnection conn = new NpgsqlConnection(_connectionString))
					{
						conn.Open();
						using (NpgsqlCommand cmd = new NpgsqlCommand(cmdSql, conn))
						{
							cmd.CommandTimeout = 0;
							using (NpgsqlDataReader dr = cmd.ExecuteReader())
							{
								while (dr.Read())
								{
									if (firstRow)
									{
										(parseTasksToWait, saveTaskToWait) = StartIterationTasks();
										firstRow = false;
									}

									K i = GetItem(dr);
									_itemsToParseManager.Add(i);
								}

								_itemsToParseManager.CompleteAdding();
							}
						}
					}
				}
				catch (Exception err)
				{
					Log.Error(err, err.Message);
					throw err;
				}
				finally
				{
					if (parseTasksToWait != null)
					{
						Task.WaitAll(parseTasksToWait);
					}

					_itemsToSaveManager.CompleteAdding();

					if (saveTaskToWait != null)
					{
						saveTaskToWait.Wait();
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract K GetItem(NpgsqlDataReader dr);

		protected abstract Task ProcessTask();

		private (Task[], Task) StartIterationTasks()
		{
			List<Task> tasks = new List<Task>();
			for (int i = 0; i < _request.ThreadCount; i++)
			{
				tasks.Add(ProcessTask());
			}

			Task saveTask = Task.Factory.StartNew(SaveResponse);

			return (tasks.ToArray(), saveTask);
		}

		private void SaveResponse()
		{
			_dataSaver.SaveResponses(_itemsToSaveManager, () => Interlocked.Increment(ref _errors));
		}
	}
}