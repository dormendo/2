using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using GisPaService.Properties;
using Npgsql;
using NpgsqlTypes;
using Serilog;

namespace GisPaService
{
	public abstract class BaseDataSaver<T>
	{
		protected string _connectionString;
		protected string _tablespace;
		protected string _requestProgress;
		protected abstract string CreateTableSql { get; }
		protected abstract string Columns { get; }
		
		public abstract string TableName { get; }
		
		
		public BaseDataSaver(string connectionString, string tablespace, string requestProgress)
		{
			_connectionString = connectionString;
			_tablespace = tablespace;
			_requestProgress = requestProgress;
		}
		
		public void CreateTable()
		{
			string sql = string.Format(CreateTableSql, TableName, string.IsNullOrWhiteSpace(_tablespace) ? "" : "TABLESPACE " + _tablespace);
			using (NpgsqlConnection conn = new NpgsqlConnection(_connectionString))
			{
				conn.Open();
				using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
				{
					cmd.CommandTimeout = 0;
					cmd.ExecuteNonQuery();
				}
			}

			Log.Debug("create table " + TableName);
		}
		
		public void SaveResponses(BlockingCollection<T> itemsToSaveManager, Action notifyErrorAction)
		{
			try
			{
				using (NpgsqlConnection conn = new NpgsqlConnection(_connectionString))
				{
					conn.Open();

					using (NpgsqlBinaryImporter importer = conn.BeginBinaryImport($"COPY {TableName} ({Columns}) FROM STDIN (FORMAT BINARY);"))
					{
						while (!itemsToSaveManager.IsCompleted)
						{
							if (itemsToSaveManager.TryTake(out T model, 100))
							{
								if (!ImportRow(importer, model))
								{
									notifyErrorAction();
								}
							}
						}

						importer.Complete();
					}
				}
			}
			catch (Exception err)
			{
				Log.Error(err, err.ToString());
				throw;
			}
		}

		protected abstract bool ImportRow(NpgsqlBinaryImporter importer, T model);
		public abstract string GetCmdSql();
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void Write<T>(NpgsqlBinaryImporter importer, T value, NpgsqlDbType type)
		{
			if (value == null)
			{
				importer.WriteNull();
			}
			else
			{
				importer.Write(value, type);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void Write<T>(NpgsqlBinaryImporter importer, T? value, NpgsqlDbType type) where T : struct
		{
			if (value == null)
			{
				importer.WriteNull();
			}
			else
			{
				importer.Write(value.Value, type);
			}
		}
	}
}