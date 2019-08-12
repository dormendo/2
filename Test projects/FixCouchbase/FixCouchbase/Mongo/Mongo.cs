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
			TestMongo1(5000, 10);
			_metadata.DeleteMany(new BsonDocument());
			TestMongo1(500, 100);
			_metadata.DeleteMany(new BsonDocument());
			TestMongo1(100, 500);
			_metadata.DeleteMany(new BsonDocument());
			TestMongo1(50, 1000);
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

		private void TestMongo1(int iterations, int itemsPerIteration)
		{
			List<WriteModel<BsonDocument>> list = new List<WriteModel<BsonDocument>>(10);
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
					Metadata md = new Metadata(key, keyNumber);
					sw1.Stop();
					sw2.Start();
					BsonDocument mdbd = md.ToBsonDocument();
					sw2.Stop();
					list.Add(new ReplaceOneModel<BsonDocument>(new BsonDocument("_id", key), mdbd) { IsUpsert = true });
				}

				_metadata.BulkWrite(list);

				if (((i + 1) % 1000) == 0)
				{
					Console.WriteLine(i + 1);
				}
			}

			sw.Stop();
			Console.WriteLine($"TestMongo1. {iterations} итераций по {itemsPerIteration}. {sw.Elapsed.ToString()}, {sw.ElapsedMilliseconds / iterations}, {sw1.Elapsed}, {sw2.Elapsed}");
		}
	}
}
