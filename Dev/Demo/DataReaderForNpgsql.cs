using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace Lfnit.Norma.DataAccess
{
	class DataReaderForNpgsql : AdoDataReader
	{
		IDbConnection _conn;
		IDbTransaction _newTx;
		IDbTransaction _existingTx;
		IDbCommand _cursorCommand;
		List<string> _cursors;

		/// <summary>
		/// контсруктор
		/// </summary>
		internal DataReaderForNpgsql(AdoHelper helper)
			: base(helper)
		{
		}

		/// <summary>
		/// Инициализирует <see cref="IDataReader"/>
		/// </summary>
		/// <param name="command">команда</param>
		/// <param name="id">обязательный параметр - идентификатор, который будет использоваться при логировании</param>
		internal override void Initialize(IDbCommand command, Guid id)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}

			try
			{
				this._conn = command.Connection;

				_id = id;

				if (command.CommandType == CommandType.StoredProcedure)
				{
					if (command.Transaction == null && !this._helper.IsEnlisted(command.Connection))
					{
						_newTx = command.Connection.BeginTransaction(IsolationLevel.ReadCommitted);
						command.Transaction = _newTx;
					}
					_existingTx = command.Transaction;
				}

				_dataReader = command.ExecuteReader();

				if (command.CommandType == CommandType.StoredProcedure)
				{
					bool canDereference = true;
					for (int i = 0; i < _dataReader.FieldCount; i++)
					{
						if (_dataReader.GetDataTypeName(0) != "refcursor")
						{
							canDereference = false;
							break;
						}
					}

					if (canDereference)
					{
						StringBuilder sb = new StringBuilder();
						while (_dataReader.Read())
						{
							for (int i = 0; i < _dataReader.FieldCount; i++)
							{
								if (_cursors == null)
								{
									_cursors = new List<string>();
								}

								string cursor = _dataReader.GetString(i);
								_cursors.Add(cursor);
								sb.Append("FETCH ALL FROM \"").Append(cursor).Append("\";");
							}
						}

						_dataReader.Dispose();

						_cursorCommand = command.Connection.CreateCommand();
						_cursorCommand.CommandText = sb.ToString();
						_cursorCommand.CommandType = CommandType.Text;
						_cursorCommand.CommandTimeout = command.CommandTimeout;
						_cursorCommand.Connection = _conn;
						_cursorCommand.Transaction = command.Transaction;

						_dataReader = _cursorCommand.ExecuteReader();
					}
				}
			}
			catch (Exception ex)
			{
				Exception ex2 = ExceptionCatcher.Catch(ex);
				if (ex == ex2)
				{
					throw;
				}
				else
				{
					throw ex2;
				}
			}
		}

		protected override void CloseInternal()
		{
			Free(true);
		}

		protected override void DisposeInternal()
		{
			Free(false);
		}

		private void Free(bool close)
		{
			try
			{
				if (_dataReader != null)
				{
					if (close)
					{
						_dataReader.Close();
					}
					else
					{
						_dataReader.Dispose();
						_dataReader = null;
					}
				}

				if (_cursorCommand != null)
				{
					_cursorCommand.Dispose();
					_cursorCommand = null;
				}

				if (_newTx != null)
				{
					_newTx.Commit();
					_newTx.Dispose();
					_newTx = null;
					_existingTx = null;
					_cursors = null;
				}
				else if (_cursors != null)
				{
					StringBuilder sb = new StringBuilder();
					for (int i = 0; i < _cursors.Count; i++)
					{
						sb.Append("CLOSE \"").Append(_cursors[i]).Append("\";");
					}

					using (IDbCommand closeCmd = _conn.CreateCommand())
					{
						closeCmd.CommandText = sb.ToString();
						closeCmd.CommandType = CommandType.Text;
						closeCmd.Connection = _conn;
						closeCmd.Transaction = _existingTx;
						closeCmd.ExecuteNonQuery();
					}

					_cursors = null;
				}
			}
			catch (Exception ex)
			{
				Exception ex2 = ExceptionCatcher.Catch(ex);
				if (ex == ex2)
				{
					throw;
				}
				else
				{
					throw ex2;
				}
			}
		}
	}
}
