using Npgsql;
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace DbLocker
{
	static class Program
	{
		private static void TestNpgsql()
		{
			List<int> list = new List<int>();
			using (NpgsqlConnection conn = new NpgsqlConnection("Host=localhost;Username=postgres;Password=H22oqi%bGerS;Database=postgres"))
			{
				conn.Open();
				using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM t1", conn))
				{
					using (NpgsqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							int id = dr.GetInt32(0);

							using (NpgsqlCommand cmd2 = new NpgsqlCommand("SELECT * FROM t1 WHERE id = " + id.ToString(), conn))
							{
								cmd.ExecuteNonQuery();
							}
							list.Add(id);
						}
					}
				}
			}
		}

		static void Main2(string[] args)
		{
			//TestNpgsql();


			string pooling = "Data Source=rac1; User Id=TEST; Password=TEST; Enlist=true; Pooling=true; Min Pool Size=1; Max Pool Size=1;Metadata Pooling=false;";
			string noPooling = "Data Source=rac1; User Id=TEST; Password=TEST; Enlist=true; Pooling=false;Metadata Pooling=false;";

			//Stopwatch sw = Stopwatch.StartNew();
			//for (int i = 0; i < 100; i++)
			//{
			//	using (OracleConnection conn = new OracleConnection(pooling))
			//	{
			//		conn.Open();
			//	}
			//}
			//sw.Stop();
			//Console.WriteLine(sw.Elapsed);

			//sw.Restart();
			//for (int i = 0; i < 100; i++)
			//{
			//	using (OracleConnection conn = new OracleConnection(noPooling))
			//	{
			//		conn.Open();
			//	}
			//}
			//sw.Stop();
			//Console.WriteLine(sw.Elapsed);

			using (OracleConnection conn = new OracleConnection(noPooling))
			{
				conn.Open();

				using (OracleCommand cmd = new OracleCommand("UPDATE t1 SET id = 0", conn))
				{
					cmd.ExecuteNonQuery();
				}

				using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { Timeout = TimeSpan.Zero, IsolationLevel = IsolationLevel.ReadCommitted }))
				{
					conn.EnlistTransaction(Transaction.Current);
					conn.IsEnlisted();

					using (OracleCommand cmd = new OracleCommand("UPDATE t1 SET id = 2", conn))
					{
						cmd.ExecuteNonQuery();
					}

					using (TransactionScope ts2 = new TransactionScope(TransactionScopeOption.Suppress))
					{
						using (OracleCommand cmd = new OracleCommand("UPDATE t1 SET id = 5", conn))
						{
							cmd.ExecuteNonQuery();
						}
					}

					using (OracleCommand cmd = new OracleCommand("UPDATE t1 SET id = 4", conn))
					{
						cmd.ExecuteNonQuery();
					}
				}
				conn.IsEnlisted();

				using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { Timeout = TimeSpan.Zero, IsolationLevel = IsolationLevel.ReadCommitted }))
				{
					conn.EnlistTransaction(Transaction.Current);

					using (OracleCommand cmd = new OracleCommand("UPDATE t1 SET id = 3", conn))
					{
						cmd.ExecuteNonQuery();
					}

					ts.Complete();
				}
			}


			//ReadLine("До открытия соединения");
			//using (OracleConnection conn = new OracleConnection(noPooling))
			//{
			//	conn.Open();
			//	ReadLine("После открытия соединения");

			//	SignPatchLocker.Lock(1, conn);
			//	ReadLine("После наложения блокировки");
			//}
			//ReadLine("После закрытия блокировки");

			//using (OracleConnection conn = new OracleConnection(noPooling))
			//{
			//	conn.Open();
			//	ReadLine("После открытия соединения");

			//	SignPatchLocker.Lock(1, conn);
			//	ReadLine("После наложения блокировки");
			//}
			//ReadLine("После закрытия блокировки");
		}

		static SqlConnection sqlConnection;
		static void Main(string[] args)
		{
			//Stopwatch sw = Stopwatch.StartNew();
			//for (int i = 0; i < 10000; i++)
			//{
			//	using (SqlConnection conn = new SqlConnection("Network Library=dbmssocn;Data Source=notebook; Database=test; Integrated Security=true; Enlist=true; Application Name=AAA; Pooling=false; MultipleActiveResultSets=true"))
			//	{
			//		conn.Open();
			//	}
			//}
			//sw.Stop();
			//Console.WriteLine(sw.Elapsed);

			//sw.Restart();
			//for (int i = 0; i < 10000; i++)
			//{
			//	using (SqlConnection conn = new SqlConnection("Network Library=dbmssocn;Data Source=notebook; Database=test; Integrated Security=true; Enlist=true; Application Name=AAA; Pooling=true; Min Pool Size=1; Max Pool Size=1; Connection reset=true; MultipleActiveResultSets=true"))
			//	{
			//		conn.Open();
			//	}
			//}
			//sw.Stop();
			//Console.WriteLine(sw.Elapsed);

			using (SqlConnection conn = new SqlConnection("Data Source=.; Database=test; Integrated Security=true; Enlist=true; Application Name=AAA; Pooling=false; MultipleActiveResultSets=true"))
			{
				sqlConnection = conn;
				conn.Open();
				
				using (SqlCommand cmd = new SqlCommand("UPDATE t1 SET id = 0", conn))
				{
					cmd.ExecuteNonQuery();
				}

				using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { Timeout = TimeSpan.Zero, IsolationLevel = IsolationLevel.ReadCommitted }))
				{
					conn.EnlistTransaction(Transaction.Current);
					conn.IsEnlisted();

					using (SqlCommand cmd = new SqlCommand("UPDATE t1 SET id = 2", conn))
					{
						cmd.ExecuteNonQuery();
					}
				}
				conn.IsEnlisted();

				using (SqlCommand cmd = new SqlCommand("UPDATE t1 SET id = 0", conn))
				{
					cmd.ExecuteNonQuery();
				}

				using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { Timeout = TimeSpan.Zero, IsolationLevel = IsolationLevel.ReadCommitted }))
				{
					conn.EnlistTransaction(Transaction.Current);
					using (SqlCommand cmd = new SqlCommand("UPDATE t1 SET id = 3", conn))
					{
						cmd.ExecuteNonQuery();
					}

					conn.EnlistTransaction(null);
					using (SqlCommand cmd = new SqlCommand("UPDATE t1 SET id = 5", conn))
					{
						cmd.ExecuteNonQuery();
					}

					conn.EnlistTransaction(Transaction.Current);
					using (SqlCommand cmd = new SqlCommand("UPDATE t1 SET id = 6", conn))
					{
						cmd.ExecuteNonQuery();
					}

					Transaction.Current.TransactionCompleted += Current_TransactionCompleted;
					ts.Complete();
				}
			}


			ReadLine("До открытия соединения");
			using (SqlConnection conn = new SqlConnection("Data Source=.; Database=test; Integrated Security=true; Enlist=true; Application Name=AAA; Pooling=false; Min Pool Size=1; Max Pool Size=1; Connection reset=true; MultipleActiveResultSets=true"))
			{
				conn.Open();
				ReadLine("После открытия соединения");

				SignPatchLocker.Lock(1, conn);
				ReadLine("После наложения блокировки");
			}
			ReadLine("После закрытия блокировки");

			using (SqlConnection conn = new SqlConnection("Data Source=.; Database=test; Integrated Security=true; Enlist=true; Application Name=AAA; Pooling=false; Min Pool Size=1; Max Pool Size=1; Connection reset=true; MultipleActiveResultSets=true"))
			{
				conn.Open();
				ReadLine("После открытия соединения");

				SignPatchLocker.Lock(1, conn);
				ReadLine("После наложения блокировки");
			}
			ReadLine("После закрытия блокировки");
		}

		private static void Current_TransactionCompleted(object sender, TransactionEventArgs e)
		{
			using (SqlCommand cmd = new SqlCommand("UPDATE t1 SET id = 0", sqlConnection))
			{
				cmd.ExecuteNonQuery();
			}
		}

		static void Main3(string[] args)
		{
			//Stopwatch sw = Stopwatch.StartNew();
			//for (int i = 0; i < 10000; i++)
			//{
			//	using (SqlConnection conn = new SqlConnection("Network Library=dbmssocn;Data Source=notebook; Database=test; Integrated Security=true; Enlist=true; Application Name=AAA; Pooling=false; MultipleActiveResultSets=true"))
			//	{
			//		conn.Open();
			//	}
			//}
			//sw.Stop();
			//Console.WriteLine(sw.Elapsed);

			//sw.Restart();
			//for (int i = 0; i < 10000; i++)
			//{
			//	using (SqlConnection conn = new SqlConnection("Network Library=dbmssocn;Data Source=notebook; Database=test; Integrated Security=true; Enlist=true; Application Name=AAA; Pooling=true; Min Pool Size=1; Max Pool Size=1; Connection reset=true; MultipleActiveResultSets=true"))
			//	{
			//		conn.Open();
			//	}
			//}
			//sw.Stop();
			//Console.WriteLine(sw.Elapsed);

			using (NpgsqlConnection conn = new NpgsqlConnection("Host=localhost;Username=postgres;Password=H22oqi%bGerS;Database=postgres;Pooling=false"))
			{
				conn.Open();

				using (NpgsqlCommand cmd = new NpgsqlCommand("UPDATE t1 SET id = 0 WHERE id = 10", conn))
				{
					cmd.ExecuteNonQuery();
				}

				using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { Timeout = TimeSpan.Zero, IsolationLevel = IsolationLevel.Serializable }))
				{
					conn.EnlistTransaction(Transaction.Current);
					conn.IsEnlisted();

					using (NpgsqlConnection conn2 = new NpgsqlConnection("Username=postgres;Host=localhost;Password=H22oqi%bGerS;Database=postgres;Pooling=false"))
					{
						conn2.Open();
						conn2.EnlistTransaction(Transaction.Current);
						using (NpgsqlCommand cmd = new NpgsqlCommand("UPDATE t1 SET id = 0 WHERE id = 10", conn2))
						{
							cmd.ExecuteNonQuery();
						}
					}

					using (NpgsqlCommand cmd = new NpgsqlCommand("UPDATE t1 SET id = 20 WHERE id = 0", conn))
					{
						cmd.ExecuteNonQuery();
					}
				}

				conn.IsEnlisted();

				using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { Timeout = TimeSpan.Zero, IsolationLevel = IsolationLevel.ReadCommitted }))
				{
					conn.EnlistTransaction(Transaction.Current);

					using (NpgsqlCommand cmd = new NpgsqlCommand("UPDATE t1 SET id = 30 WHERE id = 0", conn))
					{
						cmd.ExecuteNonQuery();
					}

					ts.Complete();
				}

				using (NpgsqlCommand cmd = new NpgsqlCommand("UPDATE t1 SET id = 10 WHERE id = 30", conn))
				{
					cmd.ExecuteNonQuery();
				}
			}


			ReadLine("До открытия соединения");
			using (NpgsqlConnection conn = new NpgsqlConnection("Data Source=.; Database=test; Integrated Security=true; Enlist=true; Application Name=AAA; Pooling=false; Min Pool Size=1; Max Pool Size=1; Connection reset=true; MultipleActiveResultSets=true"))
			{
				conn.Open();
				ReadLine("После открытия соединения");

				SignPatchLocker.Lock(1, conn);
				ReadLine("После наложения блокировки");
			}
			ReadLine("После закрытия блокировки");

			using (NpgsqlConnection conn = new NpgsqlConnection("Data Source=.; Database=test; Integrated Security=true; Enlist=true; Application Name=AAA; Pooling=false; Min Pool Size=1; Max Pool Size=1; Connection reset=true; MultipleActiveResultSets=true"))
			{
				conn.Open();
				ReadLine("После открытия соединения");

				SignPatchLocker.Lock(1, conn);
				ReadLine("После наложения блокировки");
			}
			ReadLine("После закрытия блокировки");
		}

		private static void ReadLine(string message)
		{
			Console.WriteLine(message);
			Console.ReadLine();
		}

		static bool IsEnlisted(this SqlConnection sqlConnection)
		{
			object innerConnection = typeof(SqlConnection).GetField("_innerConnection", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(sqlConnection);
			FieldInfo enlistedTransactionField =
				EnumerateInheritanceChain(innerConnection.GetType())
				.Select(t => t.GetField("_enlistedTransaction", BindingFlags.Instance | BindingFlags.NonPublic))
				.Where(fi => fi != null)
				.First();
			object enlistedTransaction = enlistedTransactionField.GetValue(innerConnection);
			return enlistedTransaction != null;
		}

		static IEnumerable<Type> EnumerateInheritanceChain(Type root)
		{
			for (Type current = root; current != null; current = current.BaseType)
				yield return current;
		}

		static bool IsEnlisted(this OracleConnection oracleConnection)
		{
			object innerConnection = typeof(OracleConnection).GetField("m_opoConCtx", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(oracleConnection);
			FieldInfo enlistedTransactionField = innerConnection.GetType().GetField("m_systemTransaction", BindingFlags.Instance | BindingFlags.Public);
				//EnumerateInheritanceChain(innerConnection.GetType())
				//.Select(t => t.GetField("m_systemTransaction", BindingFlags.Instance | BindingFlags.Public))
				//.Where(fi => fi != null)
				//.First();
			object enlistedTransaction = enlistedTransactionField.GetValue(innerConnection);
			return enlistedTransaction != null;
		}

		static bool IsEnlisted(this NpgsqlConnection npgsqlConnection)
		{
			object enlistedTransaction = typeof(NpgsqlConnection).GetProperty("EnlistedTransaction", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(npgsqlConnection);
			return enlistedTransaction != null;
		}
	}
}
