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
			// 2. Чтение по одной записи с поиском по ключу
			_metadata.DeleteMany(new BsonDocument());
			//TestMongo1(200, 10, 2);
			//TestMongo2(2000, 2);

			//_metadata.DeleteMany(new BsonDocument());
			//TestMongo1(20, 100, 5);
			//TestMongo2(2000, 4);



			// 3. Одновременное сохранение по N записей. У записи два индекса - идентификатор классификатора и идентификатор раздела.
			_classifier.DeleteMany(new BsonDocument());
			TestMongo3(10000);
			// 4. Чтение по одной записи с поиском по индексу и типу записи.
			TestMongo4(10000);
			// 5. Удаление группы записей по индексу.
			// 6. Массовое сохранение записей в коллекцию с одним индексом по "ElementId, ФИО"
			// 7. Переименование ключа из п.6.
		}

		private MongoClient _mc;
		private IMongoDatabase _db;
		private IMongoCollection<BsonDocument> _metadata;
		private IMongoCollection<BsonDocument> _classifier;

		private Classifier.Type[] classifierTypes = {
			Classifier.Type.CProps, Classifier.Type.CElements, Classifier.Type.CLinks,
			Classifier.Type.EProps, Classifier.Type.EExtCols, Classifier.Type.ELinks, Classifier.Type.EAncLinks
		};

		private void PrepareMongo()
		{
			_mc = new MongoClient("mongodb://localhost:27017");
			_db = _mc.GetDatabase("Norma");
			_metadata = _db.GetCollection<BsonDocument>("Metadata");
			_classifier = _db.GetCollection<BsonDocument>("Classifier");
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


		private void TestMongo3(int iterations)
		{
			List<WriteModel<BsonDocument>> list = new List<WriteModel<BsonDocument>>(7);

			Stopwatch sw = new Stopwatch();
			for (int i = 0; i < iterations; i++)
			{
				string iStr = i.ToString();
				Guid elementGuid = new Guid("00000000-0000-0000-0000-" + new string('0', 12 - iStr.Length) + iStr);
				list.Clear();
				for (int j = 0; j < classifierTypes.Length; j++)
				{
					Classifier.Type type = classifierTypes[j];
					Classifier c;
					if (type == Classifier.Type.CProps || type == Classifier.Type.CElements || type == Classifier.Type.CLinks)
					{
						c = new Classifier(type, i);
					}
					else
					{
						c = new Classifier(type, i, i, elementGuid);
					}

					BsonDocument mdbd = c.ToBsonDocument();
					list.Add(new ReplaceOneModel<BsonDocument>(new BsonDocument("_id", c.Key), mdbd) { IsUpsert = true });
				}

				sw.Start();
				_classifier.BulkWrite(list);
				sw.Stop();

				if (((i + 1) % 100) == 0)
				{
					Console.WriteLine(i + 1);
				}
			}

			sw.Stop();
			Console.WriteLine($"TestMongo3. {iterations} итераций. {sw.Elapsed.ToString()}, {sw.ElapsedMilliseconds * 1000 / iterations} мc/1000");
		}

		private void TestMongo4(int count)
		{
			int nulls1 = 0, nulls2 = 0;
			int incorrect1 = 0, incorrect2 = 0;
			Stopwatch sw1 = new Stopwatch();
			Stopwatch sw2 = new Stopwatch();
			for (int i = 0; i < count; i++)
			{
				string iStr = i.ToString();
				Guid elementGuid = new Guid("00000000-0000-0000-0000-" + new string('0', 12 - iStr.Length) + iStr);

				for (int j = 0; j < classifierTypes.Length; j++)
				{
					Classifier.Type type = classifierTypes[j];
					Classifier c;
					if (type == Classifier.Type.CProps || type == Classifier.Type.CElements || type == Classifier.Type.CLinks)
					{
						c = new Classifier(type, i);
					}
					else
					{
						c = new Classifier(type, i, i, elementGuid);
					}

					BsonDocument byIdBd = c.GetFindByIdBson();
					BsonDocument byIdxBd = c.GetFindByClassifierIdBson();

					sw1.Start();
					BsonDocument bd1 = _classifier.Find(byIdBd).FirstOrDefault();
					sw1.Stop();

					if (bd1 == null)
					{
						nulls1++;
					}
					else
					{
						Classifier c1 = Classifier.FromBsonDocument(bd1);
						if (!c.Compare(c1))
						{
							incorrect1++;
						}
					}

					sw2.Start();
					BsonDocument bd2 = _classifier.Find(byIdxBd).FirstOrDefault();
					sw2.Stop();

					if (bd2 == null)
					{
						nulls2++;
					}
					else
					{
						Classifier c2 = Classifier.FromBsonDocument(bd2);
						if (!c.Compare(c2))
						{
							incorrect2++;
						}
					}

				}

				if (((i + 1) % 100) == 0)
				{
					Console.WriteLine(i + 1);
				}
			}

			Console.WriteLine($"TestMongo4. Id:{sw1.Elapsed.ToString()}, {sw1.ElapsedMilliseconds * 1000 / count} / 1000, {nulls1}, {incorrect1}");
			Console.WriteLine($"TestMongo4. Idx:{sw2.Elapsed.ToString()}, {sw2.ElapsedMilliseconds * 1000 / count} / 1000, {nulls2}, {incorrect2}");
		}

	}
}
