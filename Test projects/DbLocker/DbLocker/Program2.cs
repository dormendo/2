using Npgsql;
using NpgsqlTypes;
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
			//Console.ReadLine();
			TestMssql2();
			//Console.ReadLine();
			TestMssql3();
			//Console.ReadLine();
		}

		static void TestMssql()
		{
			string connstr = "Data Source=notebook; Database=test; Integrated Security=true; Enlist=true; Application Name=AAA; Pooling=false; MultipleActiveResultSets=true";
			using (SqlConnection conn = new SqlConnection(connstr))
			{
				conn.Open();

				Stopwatch sw1 = Stopwatch.StartNew();
				using (FileStream fs = new FileStream("C:\\Temp\\files\\10.rar", FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024 * 1024, FileOptions.SequentialScan))
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
				sw1.Stop();
				Console.WriteLine("SQL Server. Write. " + sw1.Elapsed.ToString());

				Stopwatch sw2 = Stopwatch.StartNew();
				using (FileStream fs = new FileStream("C:\\Temp\\files\\10_1.rar", FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024 * 1024, FileOptions.WriteThrough))
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
				sw2.Stop();
				Console.WriteLine("SQL Server. Read. " + sw2.Elapsed.ToString());
			}
		}

		static void TestMssql2()
		{
			string connstr = "Data Source=notebook; Database=test; Integrated Security=true; Enlist=true; Application Name=AAA; Pooling=false; MultipleActiveResultSets=true";
			using (SqlConnection conn = new SqlConnection(connstr))
			{
				conn.Open();

				Stopwatch sw1 = Stopwatch.StartNew();
				using (FileStream fs = new FileStream("C:\\Temp\\files\\10.rar", FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024 * 1024, FileOptions.SequentialScan))
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
				sw1.Stop();
				Console.WriteLine("SQL Server filestream. Write. " + sw1.Elapsed.ToString());

				Stopwatch sw2 = Stopwatch.StartNew();
				using (FileStream fs = new FileStream("C:\\Temp\\files\\10_2.rar", FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024 * 1024, FileOptions.WriteThrough))
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
				sw2.Stop();
				Console.WriteLine("SQL Server filestream. Read. " + sw2.Elapsed.ToString());
			}
		}

		static void TestMssql3()
		{
			string connstr = "Data Source=notebook; Database=test; Integrated Security=true; Enlist=true; Application Name=AAA; Pooling=false; MultipleActiveResultSets=true";
			using (SqlConnection conn = new SqlConnection(connstr))
			{
				conn.Open();

				Stopwatch sw1 = Stopwatch.StartNew();
				using (FileStream fs = new FileStream("C:\\Temp\\files\\10.rar", FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024 * 1024, FileOptions.SequentialScan))
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
				sw1.Stop();
				Console.WriteLine("SQL Server filestream nosql. Write. " + sw1.Elapsed.ToString());

				Stopwatch sw2 = Stopwatch.StartNew();
				using (FileStream fs = new FileStream("C:\\Temp\\files\\10_3.rar", FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024 * 1024, FileOptions.WriteThrough))
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
				sw2.Stop();
				Console.WriteLine("SQL Server filestream nosql. Read. " + sw2.Elapsed.ToString());
			}
		}

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
	}
}
