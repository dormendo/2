using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Configuration;
using Couchbase.Configuration.Client.Providers;
using Couchbase.Core;

namespace Lanit.Norma.AppServer.Cache
{
	/// <summary>
	/// 
	/// </summary>
	public class CouchbaseCache2 : ICache
	{
		private Cluster _cache;
		private IBucket _bucket;
		private string _config;
		private string _bucketName;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="config"></param>
		/// <param name="bucket"></param>
		public CouchbaseCache2(string config, string bucket)
		{
			this._config = config;
			this._bucketName = bucket;
			_cache = new Cluster(config);
			_bucket = _cache.OpenBucket(_bucketName);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <param name="delta"></param>
		/// <returns></returns>
		public ulong Increment(string key, ulong defaultValue, ulong delta)
		{
			return _bucket.Increment(key, delta, defaultValue).Value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool Store(string key, object value)
		{
			var res = _bucket.Upsert(key, value).Success;
			return res;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="expiresAt"></param>
		/// <returns></returns>
		public IResult ExecuteStore(string key, object value, DateTime expiresAt)
		{
			var res = _bucket.Upsert(new Document<dynamic>
			{
				Id = key,
				Content = value,
				Expiry = (uint)new DateTimeOffset(expiresAt).ToUnixTimeSeconds()
			});
			return res;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="validFor"></param>
		/// <returns></returns>
		public bool Store(string key, object value, TimeSpan validFor)
		{
			var res = _bucket.Upsert(new Document<dynamic>
			{
				Id = key,
				Content = value,
				Expiry = (uint)(DateTimeOffset.Now.ToUnixTimeSeconds() + validFor.TotalSeconds)
			});
			return res.Success;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="docs"></param>
		/// <returns></returns>
		public bool BulkStore(List<IDocument<dynamic>> docs)
		{
			var res = _bucket.UpsertAsync(docs);
			res.Wait();

			//int failsCount = res.Count(x => x.Value.Success == false);
			//if (failsCount > 0)
			//{
			//	var fails = res.Where(x => x.Value.Success == false).ToList();
			//}

			//var o = Get<Dictionary<string, object>>("DataClassifier.LoadElements31569");
			//var o = Get<Lanit.Norma.Portal.Schemas.CachedPositionType[]>("FieldsCache_02/28/2019 15:39:08_51433370-3d6d-4af4-aff8-aa571f031e9a_622144");
			//foreach (var kv in keysValues)
			//{
			//	var t = Get<Marshal.MarshalClassifier>(kv.Key);
			//}

			//return failsCount == 0;
			return true;
		}

		/// <summary>
		/// Помещает коллекцию объектов в кэш
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		public bool BulkDelete(List<string> keys)
		{
			var res = _bucket.Remove(keys, TimeSpan.FromSeconds(10));
			int failsCount = res.Count(x => x.Value.Success == false);
			if (failsCount > 0)
			{
				var fails = res.Where(x => x.Value.Success == false).ToList();
			}
			return failsCount == 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		public T Get<T>(string key) where T : class
		{
			return _bucket.GetDocument<T>(key).Document?.Content;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="newExpiration"></param>
		/// <returns></returns>
		public T Get<T>(string key, DateTime newExpiration) where T : class
		{
			return _bucket.GetAndTouch<T>(key, newExpiration - DateTime.Now).Value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public IOperationResult<object> ExecuteGet(string key)
		{
			return _bucket.Get<object>(key);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		public IOperationResult<T> ExecuteGet<T>(string key) where T : class
		{
			return _bucket.Get<T>(key);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <param name="delta"></param>
		/// <returns></returns>
		public IOperationResult ExecuteIncrement(string key, ulong defaultValue, ulong delta)
		{
			return _bucket.Increment(key, delta, defaultValue);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public object Get(string key)
		{
			return _bucket.Get<object>(key).Value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="newExpiration"></param>
		/// <returns></returns>
		public object Get(string key, DateTime newExpiration)
		{
			return _bucket.GetAndTouch<object>(key, newExpiration - DateTime.Now).Value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool Remove(string key)
		{
			return _bucket.Remove(key).Success;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public IOperationResult ExecuteRemove(string key)
		{
			return _bucket.Remove(key);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public object GetBaseCache()
		{
			return _cache;
		}

		/// <summary>
		/// Сохраняет значение как элемент словаря
		/// </summary>
		/// <param name="keyOfMap">Ключ доступа к словарю</param>
		/// <param name="keyInMap">Ключ внутри словаря</param>
		/// <param name="value">Значение</param>
		public void StoreInMap<T>(string keyOfMap, string keyInMap, T value) where T : class
		{
			byte[] content = ProcessSerializer.GetByteArrayFromObject(value);
			string base64Value = Convert.ToBase64String(content);
			_bucket.MapAdd(keyOfMap, keyInMap, base64Value, true);
		}

		/// <summary>
		/// Возвращает значение из элемента словаря
		/// </summary>
		/// <param name="keyOfMap">Ключ доступа к словарю</param>
		/// <param name="keyInMap">Ключ внутри словаря</param>
		public T GetFromMap<T>(string keyOfMap, string keyInMap) where T : class
		{
			IResult<string> result = _bucket.MapGet<string>(keyOfMap, keyInMap);
			if (result.Success)
			{
				byte[] content = Convert.FromBase64String(result.Value);
				return ProcessSerializer.GetObjectFromByteArray<T>(content);
			}

			return null;
		}

		/// <summary>
		/// Сохраняет значение как элемент словаря
		/// </summary>
		/// <param name="keyOfMap">Ключ доступа к словарю</param>
		/// <param name="keyInMap">Ключ внутри словаря</param>
		/// <param name="value">Значение</param>
		public void StoreInMapRaw(string keyOfMap, string keyInMap, string value)
		{
			_bucket.MapAdd(keyOfMap, keyInMap, value, true);
		}

		/// <summary>
		/// Возвращает значение из элемента словаря
		/// </summary>
		/// <param name="keyOfMap">Ключ доступа к словарю</param>
		/// <param name="keyInMap">Ключ внутри словаря</param>
		public string GetFromMapRaw(string keyOfMap, string keyInMap)
		{
			IResult<string> result = _bucket.MapGet<string>(keyOfMap, keyInMap);
			if (result.Success)
			{
				return result.Value;
			}

			return null;
		}

		/// <summary>
		/// Сбрасывает кеш
		/// </summary>
		public void Flush()
		{
			using (Couchbase.Management.IBucketManager bm = _bucket.CreateManager("Administrator", ""))
			{
				bm.Flush();
			}
		}
	}
}
