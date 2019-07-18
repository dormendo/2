using Npgsql;
using NpgsqlTypes;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DbLocker
{
	static class Program2
	{
		static void Main()
		{
			TestMssql();
			TestOracle();
		}

		#region MSSQL

		//static string mssqlConnstr = "Data Source=notebook; Database=test; Integrated Security=true; Enlist=true; Application Name=AAA; Pooling=false; MultipleActiveResultSets=true";
		static string mssqlConnstr = "Data Source=nsigpb_agl; Database=test; Integrated Security=true; Enlist=true; Application Name=AAA; Pooling=false; MultipleActiveResultSets=true";

		static void TestMssql()
		{
			TestMssqIteration("1", 10);
			TestMssqIteration("10", 10);
			TestMssqIteration("512", 1);
			//TestMssqIteration("1024", 1);
		}

		static void TestMssqIteration(string name, int iterations)
		{
			Stopwatch r = new Stopwatch();
			Stopwatch w = new Stopwatch();

			TestMssql1(name, w, r);
			w.Reset();
			r.Reset();

			for (int i = 0; i < iterations; i++)
			{
				TestMssql1(name, w, r);
			}
			Console.WriteLine($"MSSQL, {name}Мб, SQL, {iterations} раз. Запись: {w.ElapsedMilliseconds}, чтение {r.ElapsedMilliseconds}");
			w.Reset();
			r.Reset();

			TestMssql2(name, w, r);
			w.Reset();
			r.Reset();

			for (int i = 0; i < iterations; i++)
			{
				TestMssql2(name, w, r);
			}
			Console.WriteLine($"MSSQL, {name}Мб, SQL/FileStream, {iterations} раз. Запись: {w.ElapsedMilliseconds}, чтение {r.ElapsedMilliseconds}");
			w.Reset();
			r.Reset();

			TestMssql3(name, w, r);
			w.Reset();
			r.Reset();

			for (int i = 0; i < iterations; i++)
			{
				TestMssql3(name, w, r);
			}
			Console.WriteLine($"MSSQL, {name}Мб, файл/FileStream, {iterations} раз. Запись: {w.ElapsedMilliseconds}, чтение {r.ElapsedMilliseconds}");
			w.Reset();
			r.Reset();
		}

		static void TestMssql1(string name, Stopwatch writeStopwatch, Stopwatch readStopwatch)
		{
			using (SqlConnection conn = new SqlConnection(mssqlConnstr))
			{
				conn.Open();

				writeStopwatch.Start();
				using (FileStream fs = new FileStream($"C:\\Temp\\files\\{name}.rar", FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024 * 1024, FileOptions.SequentialScan))
				{
					using (SqlCommand cmd = new SqlCommand("UPDATE b SET d = @d WHERE id = @id", conn))
					{
						cmd.CommandTimeout = 0;
						SqlParameter dParam = new SqlParameter("@d", SqlDbType.VarBinary, -1);
						dParam.Value = fs;
						cmd.Parameters.Add(dParam);
						SqlParameter idParam = new SqlParameter("@id", SqlDbType.Int);
						idParam.Value = 1;
						cmd.Parameters.Add(idParam);

						cmd.ExecuteNonQuery();
					}
				}
				writeStopwatch.Stop();

				using (SqlCommand cmd = new SqlCommand("CHECKPOINT", conn))
				{
					cmd.ExecuteNonQuery();
				}

				using (SqlCommand cmd = new SqlCommand("DBCC DROPCLEANBUFFERS", conn))
				{
					cmd.ExecuteNonQuery();
				}
			}

			Thread.Sleep(1000);

			using (SqlConnection conn = new SqlConnection(mssqlConnstr))
			{
				conn.Open();

				readStopwatch.Start();
				using (FileStream fs = new FileStream($"C:\\Temp\\files\\{name}_1.rar", FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024 * 1024, FileOptions.WriteThrough))
				{
					using (SqlCommand cmd = new SqlCommand("SELECT d FROM b WHERE id = @id", conn))
					{
						cmd.CommandTimeout = 0;
						SqlParameter idParam = new SqlParameter("@id", SqlDbType.Int);
						idParam.Value = 1;
						cmd.Parameters.Add(idParam);

						using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
						{
							if (dr.Read())
							{
								dr.GetStream(0).CopyTo(fs);
							}
						}
					}
				}
				readStopwatch.Stop();
			}
		}

		static void TestMssql2(string name, Stopwatch writeStopwatch, Stopwatch readStopwatch)
		{
			using (SqlConnection conn = new SqlConnection(mssqlConnstr))
			{
				conn.Open();

				writeStopwatch.Start();
				using (FileStream fs = new FileStream($"C:\\Temp\\files\\{name}.rar", FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024 * 1024, FileOptions.SequentialScan))
				{
					using (SqlCommand cmd = new SqlCommand("UPDATE b2 SET d = @d WHERE id = @id", conn))
					{
						cmd.CommandTimeout = 0;
						SqlParameter dParam = new SqlParameter("@d", SqlDbType.VarBinary, -1);
						dParam.Value = fs;
						cmd.Parameters.Add(dParam);
						SqlParameter idParam = new SqlParameter("@id", SqlDbType.Int);
						idParam.Value = 1;
						cmd.Parameters.Add(idParam);

						cmd.ExecuteNonQuery();
					}
				}
				writeStopwatch.Stop();

				using (SqlCommand cmd = new SqlCommand("CHECKPOINT", conn))
				{
					cmd.ExecuteNonQuery();
				}

				using (SqlCommand cmd = new SqlCommand("DBCC DROPCLEANBUFFERS", conn))
				{
					cmd.ExecuteNonQuery();
				}
			}

			Thread.Sleep(1000);

			using (SqlConnection conn = new SqlConnection(mssqlConnstr))
			{
				conn.Open();

				readStopwatch.Start();
				using (FileStream fs = new FileStream($"C:\\Temp\\files\\{name}_2.rar", FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024 * 1024, FileOptions.WriteThrough))
				{
					using (SqlCommand cmd = new SqlCommand("SELECT d FROM b2 WHERE id = @id", conn))
					{
						cmd.CommandTimeout = 0;
						SqlParameter idParam = new SqlParameter("@id", SqlDbType.Int);
						idParam.Value = 1;
						cmd.Parameters.Add(idParam);

						using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
						{
							if (dr.Read())
							{
								dr.GetStream(0).CopyTo(fs);
							}
						}
					}
				}
				readStopwatch.Stop();
			}
		}

		static void TestMssql3(string name, Stopwatch writeStopwatch, Stopwatch readStopwatch)
		{
			using (SqlConnection conn = new SqlConnection(mssqlConnstr))
			{
				conn.Open();

				writeStopwatch.Start();
				using (FileStream fs = new FileStream($"C:\\Temp\\files\\{name}.rar", FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024 * 1024, FileOptions.SequentialScan))
				{
					using (SqlTransaction tx = conn.BeginTransaction())
					{
						string pathName = null;
						byte[] txContext = null;
						using (SqlCommand cmd = new SqlCommand("SELECT d.PathName(), GET_FILESTREAM_TRANSACTION_CONTEXT() FROM b2 WHERE id = @id", conn, tx))
						{
							SqlParameter idParam = new SqlParameter("@id", SqlDbType.Int);
							idParam.Value = 1;
							cmd.Parameters.Add(idParam);

							using (SqlDataReader dr = cmd.ExecuteReader())
							{
								if (dr.Read())
								{
									pathName = dr.GetString(0);
									txContext = dr.GetSqlBytes(1).Buffer;
								}
							}
						}

						using (SqlFileStream sfs = new SqlFileStream(pathName, txContext, FileAccess.Write, FileOptions.SequentialScan, 0))
						{
							fs.CopyTo(sfs);
						}

						tx.Commit();
					}
				}
				writeStopwatch.Stop();

				using (SqlCommand cmd = new SqlCommand("CHECKPOINT", conn))
				{
					cmd.ExecuteNonQuery();
				}

				using (SqlCommand cmd = new SqlCommand("DBCC DROPCLEANBUFFERS", conn))
				{
					cmd.ExecuteNonQuery();
				}
			}

			Thread.Sleep(1000);

			using (SqlConnection conn = new SqlConnection(mssqlConnstr))
			{
				conn.Open();

				readStopwatch.Start();
				using (FileStream fs = new FileStream($"C:\\Temp\\files\\{name}_3.rar", FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024 * 1024, FileOptions.WriteThrough))
				{
					using (SqlTransaction tx = conn.BeginTransaction())
					{
						string pathName = null;
						byte[] txContext = null;
						using (SqlCommand cmd = new SqlCommand("SELECT d.PathName(), GET_FILESTREAM_TRANSACTION_CONTEXT() FROM b2 WHERE id = @id", conn, tx))
						{
							SqlParameter idParam = new SqlParameter("@id", SqlDbType.Int);
							idParam.Value = 1;
							cmd.Parameters.Add(idParam);

							using (SqlDataReader dr = cmd.ExecuteReader())
							{
								if (dr.Read())
								{
									pathName = dr.GetString(0);
									txContext = dr.GetSqlBytes(1).Buffer;
								}
							}
						}

						using (SqlFileStream sfs = new SqlFileStream(pathName, txContext, FileAccess.Read, FileOptions.SequentialScan, 0))
						{
							sfs.CopyTo(fs);
						}

						tx.Commit();
					}
				}
				readStopwatch.Stop();
			}
		}

		#endregion

		#region Oracle

		//static string oracleConnstr = "Data Source=rac1; User Id=TEST; Password=TEST; Enlist=true; Pooling=false;Metadata Pooling=false;";
		static string oracleConnstr = "Data Source=oraclerdf64; User Id=NORMA_CB_NEW; Password=NORMA_CB_NEW; Enlist=true; Pooling=false;Metadata Pooling=false;";
		static int oracleChunkSize = 4 * 1024 * 1024;

		static void TestOracle()
		{
			TestOracleIteration("1", 10);
			TestOracleIteration("10", 10);
			TestOracleIteration("512", 1);
			//TestOracleIteration("1024", 1);
		}

		static void TestOracleIteration(string name, int iterations)
		{
			Stopwatch r = new Stopwatch();
			Stopwatch w = new Stopwatch();

			TestOracle1(name, w, r);
			w.Reset();
			r.Reset();

			for (int i = 0; i < iterations; i++)
			{
				TestOracle1(name, w, r);
			}
			Console.WriteLine($"Oracle, {name}Мб, DBMS_LOB, {iterations} раз. Запись: {w.ElapsedMilliseconds}, чтение {r.ElapsedMilliseconds}");
			w.Reset();
			r.Reset();

			TestOracle2(name, w, r);
			w.Reset();
			r.Reset();

			for (int i = 0; i < iterations; i++)
			{
				TestOracle2(name, w, r);
			}
			Console.WriteLine($"Oracle, {name}Мб, ADO.NET, {iterations} раз. Запись: {w.ElapsedMilliseconds}, чтение {r.ElapsedMilliseconds}");
			w.Reset();
			r.Reset();
		}

		#region DataBigBlob

		static byte[] OracleCopyBuffer(byte[] source, int length)
		{
			byte[] copy = new byte[length];
			Buffer.BlockCopy(source, 0, copy, 0, length);
			return copy;
		}

		static void OracleAddBlob(OracleConnection conn, int id, byte[] blob)
		{
			using (OracleCommand cmd = new OracleCommand("BBA", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.FetchSize = 4 * 1024 * 1024;
				OracleParameter idParam = new OracleParameter("p_ID", OracleDbType.Decimal);
				idParam.Value = id;
				cmd.Parameters.Add(idParam);
				OracleParameter dataParam = new OracleParameter("p_DATA", OracleDbType.Blob);
				dataParam.Value = blob;
				cmd.Parameters.Add(dataParam);

				cmd.ExecuteNonQuery();
			}
		}

		static void OracleAppendBlob(OracleConnection conn, int id, byte[] blob)
		{
			using (OracleCommand cmd = new OracleCommand("BBAP", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.FetchSize = 4 * 1024 * 1024;
				OracleParameter idParam = new OracleParameter("p_ID", OracleDbType.Decimal);
				idParam.Value = id;
				cmd.Parameters.Add(idParam);
				OracleParameter dataParam = new OracleParameter("p_DATA", OracleDbType.Blob);
				dataParam.Value = blob;
				cmd.Parameters.Add(dataParam);

				cmd.ExecuteNonQuery();
			}
		}

		static byte[] OracleLoadBlob(OracleConnection conn, int id, int startPos, int length)
		{
			using (OracleCommand cmd = new OracleCommand("BBL", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.FetchSize = 4 * 1024 * 1024;
				OracleParameter idParam = new OracleParameter("p_ID", OracleDbType.Decimal);
				idParam.Value = id;
				cmd.Parameters.Add(idParam);
				OracleParameter startPosParam = new OracleParameter("p_START_POS", OracleDbType.Int32);
				startPosParam.Value = startPos + 1;
				cmd.Parameters.Add(startPosParam);
				OracleParameter lengthParam = new OracleParameter("p_LENGTH", OracleDbType.Int32);
				lengthParam.Value = length;
				cmd.Parameters.Add(lengthParam);
				OracleParameter dataParam = new OracleParameter("p_DATA", OracleDbType.Blob);
				dataParam.Direction = ParameterDirection.InputOutput;
				dataParam.Value = new byte[1];
				cmd.Parameters.Add(dataParam);
				OracleParameter dataLengthParam = new OracleParameter("p_DATA_LENGTH", OracleDbType.Int32);
				dataLengthParam.Value = length;
				dataLengthParam.Direction = ParameterDirection.Output;
				cmd.Parameters.Add(dataLengthParam);

				cmd.ExecuteNonQuery();

				OracleBlob b = dataParam.Value as OracleBlob;
				if (b != null && !b.IsNull)
				{
					int dataLength = ((OracleDecimal)dataLengthParam.Value).ToInt32();
					byte[] result = b.Value;
					return (dataLength == length ? result : OracleCopyBuffer(result, dataLength));
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

		static void TestOracle1(string name, Stopwatch writeStopwatch, Stopwatch readStopwatch)
		{
			using (OracleConnection conn = new OracleConnection(oracleConnstr))
			{
				conn.Open();

				writeStopwatch.Start();
				using (FileStream fs = new FileStream($"C:\\Temp\\files\\{name}.rar", FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024 * 1024, FileOptions.SequentialScan))
				{
					int bytesRead = 0;
					bool first = true;
					byte[] buffer = new byte[oracleChunkSize];
					do
					{
						bytesRead = fs.Read(buffer, 0, oracleChunkSize);
						if (bytesRead > 0)
						{
							byte[] storeBuffer = (bytesRead == oracleChunkSize ? buffer : OracleCopyBuffer(buffer, bytesRead));
							if (first)
							{
								OracleAddBlob(conn, 1, storeBuffer);
								first = false;
							}
							else
							{
								OracleAppendBlob(conn, 1, storeBuffer);
							}
						}
					}
					while (bytesRead > 0);
				}
				writeStopwatch.Stop();
			}

			using (OracleConnection conn = new OracleConnection(oracleConnstr))
			{
				conn.Open();

				readStopwatch.Start();
				using (FileStream fs = new FileStream($"C:\\Temp\\files\\{name}_1.rar", FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024 * 1024, FileOptions.WriteThrough))
				{
					int startPosition = 0;
					byte[] bytesRead = OracleLoadBlob(conn, 1, startPosition, oracleChunkSize);
					while (bytesRead != null)
					{
						fs.Write(bytesRead, 0, bytesRead.Length);
						startPosition += bytesRead.Length;
						bytesRead = OracleLoadBlob(conn, 1, startPosition, oracleChunkSize);
					}
				}
				readStopwatch.Stop();
			}
		}

		static void TestOracle2(string name, Stopwatch writeStopwatch, Stopwatch readStopwatch)
		{
			using (OracleConnection conn = new OracleConnection(oracleConnstr))
			{
				conn.Open();

				writeStopwatch.Start();
				using (FileStream fs = new FileStream($"C:\\Temp\\files\\{name}.rar", FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024 * 1024, FileOptions.SequentialScan))
				{
					using (OracleTransaction tx = conn.BeginTransaction())
					{
						//using (OracleCommand cmd = new OracleCommand("DECLARE c int; BEGIN BEGIN SELECT id into c FROM b WHERE id = 1 FOR UPDATE; EXCEPTION WHEN no_data_found THEN INSERT INTO B (ID, D) VALUES (1, HEXTORAW('0')); END; BEGIN SELECT id into c FROM b WHERE id = 1 AND d IS NOT NULL FOR UPDATE; EXCEPTION WHEN no_data_found THEN UPDATE B SET D = HEXTORAW('0') WHERE id = 1; END; END;", conn))
						//{
						//	cmd.CommandTimeout = 0;
						//	cmd.ExecuteNonQuery();
						//}

						//using (OracleCommand cmd = new OracleCommand("SELECT id, d FROM b WHERE id = :id", conn))
						//{
						//	cmd.CommandTimeout = 0;
						//	OracleParameter idParam = new OracleParameter(":id", OracleDbType.Decimal);
						//	idParam.Value = 1;
						//	cmd.Parameters.Add(idParam);

						//	using (OracleDataReader dr = cmd.ExecuteReader())
						//	{
						//		if (dr.Read())
						//		{
						//			OracleBlob blob = dr.GetOracleBlobForUpdate(1);
						//			fs.CopyTo(blob, 4 * 1024 * 1024);
						//		}
						//	}
						//}

						using (OracleCommand cmd = new OracleCommand("BBS", conn))
						{
							cmd.CommandType = CommandType.StoredProcedure;
							cmd.FetchSize = 4 * 1024 * 1024;
							//OracleCommandBuilder.DeriveParameters(cmd);
							OracleParameter idParam = new OracleParameter("p_ID", OracleDbType.Decimal);
							idParam.Value = 1;
							cmd.Parameters.Add(idParam);
							OracleParameter cParam = new OracleParameter("p_c", OracleDbType.RefCursor);
							cParam.Direction = ParameterDirection.Output;
							cmd.Parameters.Add(cParam);

							using (OracleDataReader dr = cmd.ExecuteReader())
							{
								if (dr.Read())
								{
									OracleBlob blob = dr.GetOracleBlob(1);
									
									fs.CopyTo(blob, 4 * 1024 * 1024);
								}
							}
						}
						tx.Commit();
					}
				}
				writeStopwatch.Stop();
			}

			using (OracleConnection conn = new OracleConnection(oracleConnstr))
			{
				conn.Open();

				readStopwatch.Start();
				using (FileStream fs = new FileStream($"C:\\Temp\\files\\{name}_2.rar", FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024 * 1024, FileOptions.WriteThrough))
				{
					using (OracleCommand cmd = new OracleCommand("SELECT d FROM b WHERE id = :id", conn))
					{
						cmd.CommandTimeout = 0;
						cmd.FetchSize = 4 * 1024 * 1024;
						OracleParameter idParam = new OracleParameter(":id", OracleDbType.Decimal);
						idParam.Value = 1;
						cmd.Parameters.Add(idParam);

						using (OracleDataReader dr = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
						{
							if (dr.Read())
							{
								dr.GetOracleBlob(0).CopyTo(fs, 4 * 1024* 1024);
							}
						}
					}
				}
				readStopwatch.Stop();
			}
		}

		#endregion

		#region Postgres

		static void TestPostgres()
		{
			string connstr = "Host=localhost;Username=postgres;Password=H22oqi%bGerS;Database=postgres";
			using (NpgsqlConnection conn = new NpgsqlConnection(connstr))
			{
				conn.Open();

				using (FileStream fs = new FileStream("C:\\Temp\\files\\512.rar", FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024 * 1024, FileOptions.SequentialScan))
				{
					using (NpgsqlCommand cmd = new NpgsqlCommand("UPDATE b SET d = @d WHERE id = @id", conn))
					{
						cmd.CommandTimeout = 0;
						NpgsqlParameter dParam = new NpgsqlParameter("@d", NpgsqlDbType.Bytea, -1);
						dParam.Value = fs;
						cmd.Parameters.Add(dParam);
						NpgsqlParameter idParam = new NpgsqlParameter("@id", NpgsqlDbType.Integer);
						idParam.Value = 1;
						cmd.Parameters.Add(idParam);

						Stopwatch sw1 = Stopwatch.StartNew();
						cmd.ExecuteNonQuery();
						sw1.Stop();
						Console.WriteLine("PostgreSql. Write. " + sw1.Elapsed.ToString());
					}
				}

				using (FileStream fs = new FileStream("C:\\Temp\\files\\512_3.rar", FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024 * 1024, FileOptions.WriteThrough))
				{
					using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT d FROM b WHERE id = @id", conn))
					{
						cmd.CommandTimeout = 0;
						NpgsqlParameter idParam = new NpgsqlParameter("@id", SqlDbType.Int);
						idParam.Value = 1;
						cmd.Parameters.Add(idParam);

						Stopwatch sw1 = Stopwatch.StartNew();
						using (NpgsqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
						{
							if (dr.Read())
							{
								dr.GetStream(0).CopyTo(fs);
							}
						}
						sw1.Stop();
						Console.WriteLine("PostgreSql. Read. " + sw1.Elapsed.ToString());
					}
				}
			}
		}

		#endregion
	}
}
