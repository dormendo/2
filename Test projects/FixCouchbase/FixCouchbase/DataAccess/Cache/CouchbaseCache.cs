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
	public class CouchbaseCache : ICache
	{
		private Cluster _cache;
		private string _config;
		private string _bucket;
	
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="config"></param>
		/// <param name="bucket"></param>
		public CouchbaseCache(string config, string bucket)
		{
			this._config = config;
			this._bucket = bucket;
			_cache = new Cluster(config);
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
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				return bucket.Increment(key, delta, defaultValue).Value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool Store(string key, object value)
		{
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				var res = bucket.Upsert(key, value).Success;
				return res;
			}
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
			using (var bucket = _cache.OpenBucket(_bucket))
			{
                var res = bucket.Upsert(new Document<dynamic>
				{
					Id = key, Content = value,
					Expiry = (uint)new DateTimeOffset(expiresAt).ToUnixTimeSeconds()
				});
				return res;
			}
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
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				var res = bucket.Upsert(new Document<dynamic>
				{
					Id = key, Content = value,
					Expiry = (uint)(DateTimeOffset.Now.ToUnixTimeSeconds() + validFor.TotalSeconds)
				});
				return res.Success;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="docs"></param>
		/// <returns></returns>
		public bool BulkStore(List<IDocument<dynamic>> docs)
		{
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				var res = bucket.UpsertAsync(docs);
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
		}

		/// <summary>
		/// Помещает коллекцию объектов в кэш
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		public bool BulkDelete(List<string> keys)
		{
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				var res = bucket.Remove(keys, TimeSpan.FromSeconds(10));
				int failsCount = res.Count(x => x.Value.Success == false);
				if (failsCount > 0)
				{
					var fails = res.Where(x => x.Value.Success == false).ToList();
				}
				return failsCount == 0;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		public T Get<T>(string key) where T : class
		{
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				//if (bucket.Exists(key))
				//{
					//var res2 = bucket.GetDocument<T>(key);
					//var res = bucket.Get<T>(key).Value;
					var res = bucket.GetDocument<T>(key);
					return res.Document?.Content;
				//}
				//else
				//{
				//	return null;
				//}
			}
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
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				var res = bucket.GetAndTouch<T>(key, newExpiration - DateTime.Now).Value;
				return res;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public IOperationResult<object> ExecuteGet(string key)
		{
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				var res = bucket.Get<object>(key);
				return res;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		public IOperationResult<T> ExecuteGet<T>(string key) where T : class
		{
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				var res = bucket.Get<T>(key);
				return res;
			}
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
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				return bucket.Increment(key, delta, defaultValue);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public object Get(string key)
		{
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				return bucket.Get<object>(key).Value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="newExpiration"></param>
		/// <returns></returns>
		public object Get(string key, DateTime newExpiration)
		{
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				return bucket.GetAndTouch<object>(key, newExpiration - DateTime.Now).Value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool Remove(string key)
		{
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				return bucket.Remove(key).Success;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public IOperationResult ExecuteRemove(string key)
		{
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				return bucket.Remove(key);
			}
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
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				byte[] content = ProcessSerializer.GetByteArrayFromObject(value);
				string base64Value = Convert.ToBase64String(content);
				bucket.MapAdd(keyOfMap, keyInMap, base64Value, true);
			}
		}

		/// <summary>
		/// Возвращает значение из элемента словаря
		/// </summary>
		/// <param name="keyOfMap">Ключ доступа к словарю</param>
		/// <param name="keyInMap">Ключ внутри словаря</param>
		public T GetFromMap<T>(string keyOfMap, string keyInMap) where T : class
		{
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				IResult<string> result = bucket.MapGet<string>(keyOfMap, keyInMap);
				if (result.Success)
				{
					byte[] content = Convert.FromBase64String(result.Value);
					return ProcessSerializer.GetObjectFromByteArray<T>(content);
				}

				return null;
			}
		}

		/// <summary>
		/// Сохраняет значение как элемент словаря
		/// </summary>
		/// <param name="keyOfMap">Ключ доступа к словарю</param>
		/// <param name="keyInMap">Ключ внутри словаря</param>
		/// <param name="value">Значение</param>
		public void StoreInMapRaw(string keyOfMap, string keyInMap, string value)
		{
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				bucket.MapAdd(keyOfMap, keyInMap, value, true);
			}
		}

		/// <summary>
		/// Возвращает значение из элемента словаря
		/// </summary>
		/// <param name="keyOfMap">Ключ доступа к словарю</param>
		/// <param name="keyInMap">Ключ внутри словаря</param>
		public string GetFromMapRaw(string keyOfMap, string keyInMap)
		{
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				IResult<string> result = bucket.MapGet<string>(keyOfMap, keyInMap);
				if (result.Success)
				{
					return result.Value;
				}

				return null;
			}
		}

		/// <summary>
		/// Сбрасывает кеш
		/// </summary>
		public void Flush()
		{
			using (var bucket = _cache.OpenBucket(_bucket))
			{
				bucket.CreateManager("Administrator", "").Flush();
			}
		}
	}
}
