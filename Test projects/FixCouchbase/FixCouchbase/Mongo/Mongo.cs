using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixCouchbase
{
	class Mongo
	{
		public void TestMongo()
		{
			PrepareMongo();

			// 1. Одновременное сохранение по 10 записей.
			_metadata.DeleteMany(new BsonDocument());
			TestMongo1(200, 10, 2);
			TestMongo2(2000, 2);

			_metadata.DeleteMany(new BsonDocument());
			TestMongo1(20, 100, 5);
			TestMongo2(2000, 5);
			// 2. Чтение по одной записи с поиском по ключу
			// 3. Одновременное сохранение по 10 записей. У записи два индекса - идентификатор классификатора и идентификатор раздела.
			// 4. Чтение по одной записи с поиском по индексу и типу записи.
			// 5. Удаление группы записей по индексу.
			// 6. Массовое сохранение записей в коллекцию с одним индексом по "ElementId, ФИО"
			// 7. Переименование ключа из п.6.
		}

		private MongoClient _mc;
		private IMongoDatabase _db;
		private IMongoCollection<BsonDocument> _metadata;
		private void PrepareMongo()
		{
			_mc = new MongoClient("mongodb://nsicluster1:27017");
			_db = _mc.GetDatabase("Norma");
			_metadata = _db.GetCollection<BsonDocument>("Metadata");
		}

		private void TestMongo1(int iterations, int itemsPerIteration, int offset)
		{
			List<WriteModel<BsonDocument>> list = new List<WriteModel<BsonDocument>>(itemsPerIteration);
			Stopwatch sw = Stopwatch.StartNew();
			Stopwatch sw1 = new Stopwatch();
			Stopwatch sw2 = new Stopwatch();
			for (int i = 0; i < iterations; i++)
			{
				list.Clear();
				for (int j = 0; j < itemsPerIteration; j++)
				{
					int keyNumber = i * itemsPerIteration + j;
					string key = "Medatata" + keyNumber.ToString();
					sw1.Start();
					Metadata md = new Metadata(key, keyNumber + offset);
					sw1.Stop();
					sw2.Start();
					BsonDocument mdbd = md.ToBsonDocument();
					sw2.Stop();
					list.Add(new ReplaceOneModel<BsonDocument>(new BsonDocument("_id", key), mdbd) { IsUpsert = true });
				}

				_metadata.BulkWrite(list);

				if (((i + 1) % 100) == 0)
				{
					Console.WriteLine(i + 1);
				}
			}

			sw.Stop();
			Console.WriteLine($"TestMongo1. {iterations} итераций по {itemsPerIteration}. {sw.Elapsed.ToString()}, {sw.ElapsedMilliseconds / iterations}, {sw1.Elapsed}, {sw2.Elapsed}");
		}

		private void TestMongo2(int count, int offset)
		{
			int nulls = 0;
			int incorrect = 0;
			Stopwatch sw = new Stopwatch();
			for (int i = 0; i < count; i++)
			{
				string key = "Medatata" + i.ToString();
				sw.Start();
				BsonDocument bd = _metadata.Find(new BsonDocument("_id", key)).FirstOrDefault();
				sw.Stop();

				if (bd == null)
				{
					nulls++;
				}
				else
				{
					Metadata md = Metadata.FromBsonDocument(bd);
					if (!md.CheckOffset(i + offset))
					{
						incorrect++;
					}
				}

				if (((i + 1) % 100) == 0)
				{
					Console.WriteLine(i + 1);
				}
			}

			sw.Stop();
			Console.WriteLine($"TestMongo2. {sw.Elapsed.ToString()}, {sw.ElapsedMilliseconds * 1000 / count} / 1000, {nulls}, {incorrect}");
		}
	}
}
