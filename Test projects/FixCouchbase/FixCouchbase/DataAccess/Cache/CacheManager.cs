using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Couchbase;
using Couchbase.Configuration;
using Couchbase.Configuration.Client.Providers;
using Newtonsoft.Json;

namespace Lanit.Norma.AppServer.Cache
{
	/// <summary>
	/// Менеджер кэша
	/// </summary>
	public static class CacheManager
	{
		private readonly static ICache _cache;
		private readonly static ICache _storage;

		/// <summary>
		/// Режим работы кэша
		/// </summary>
		public readonly static CacheMode Mode;

		static CacheManager()
		{
			_cache = new CouchbaseCache("couchbaseClients/couchbase", "default");
			_storage = new CouchbaseCache2("couchbaseClients/couchbase", "default");
		}
		/// <summary>
		/// Кэш
		/// </summary>
		public static ICache Cache { get { return _cache; } }

		/// <summary>
		/// Хранилище
		/// </summary>
		public static ICache Storage { get { return _storage; } }
	}
}
