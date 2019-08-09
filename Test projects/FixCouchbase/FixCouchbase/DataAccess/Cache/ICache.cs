using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Couchbase;

namespace Lanit.Norma.AppServer.Cache
{
	/// <summary>
	/// Интерфейс кэша
	/// </summary>
	public interface ICache
	{
		/// <summary>
		/// Увеличивает значение
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <param name="delta"></param>
		/// <returns></returns>
		ulong Increment(string key, ulong defaultValue, ulong delta);

		/// <summary>
		/// Помещает объект в кэш
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool Store(string key, object value);

		/// <summary>
		/// Помещает объект в кэш
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="expiresAt"></param>
		/// <returns></returns>
		IResult ExecuteStore(string key, object value, DateTime expiresAt);

		/// <summary>
		/// Помещает объект в кэш
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="validFor"></param>
		/// <returns></returns>
		bool Store(string key, object value, TimeSpan validFor);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="docs"></param>
		/// <returns></returns>
		bool BulkStore(List<IDocument<dynamic>> docs);

		/// <summary>
		/// Помещает коллекцию объектов в кэш
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		bool BulkDelete(List<string> keys);

		/// <summary>
		/// Возвращает объект по ключу
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		T Get<T>(string key) where T : class;

		/// <summary>
		/// Возвращает объект по ключу
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="newExpiration"></param>
		/// <returns></returns>
		T Get<T>(string key, DateTime newExpiration) where T : class;

		/// <summary>
		/// Возвращает объект по ключу
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		IOperationResult<object> ExecuteGet(string key);

		/// <summary>
		/// Возвращает объект по ключу
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		IOperationResult<T> ExecuteGet<T>(string key) where T : class;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <param name="delta"></param>
		/// <returns></returns>
		IOperationResult ExecuteIncrement(string key, ulong defaultValue, ulong delta);

		/// <summary>
		/// Возвращает объект по ключу
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		object Get(string key);

		/// <summary>
		/// Возвращает объект по ключу
		/// </summary>
		/// <param name="key"></param>
		/// <param name="newExpiration"></param>
		/// <returns></returns>
		object Get(string key, DateTime newExpiration);

		/// <summary>
		/// Удаляет объект из кэша
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		bool Remove(string key);

		/// <summary>
		/// Удаляет объект из кэша
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		IOperationResult ExecuteRemove(string key);

		/// <summary>
		/// Возвращает конкретную реализацию кэша
		/// </summary>
		object GetBaseCache();

		/// <summary>
		/// Сохраняет значение как элемент словаря
		/// </summary>
		/// <param name="keyOfMap">Ключ доступа к словарю</param>
		/// <param name="keyInMap">Ключ внутри словаря</param>
		/// <param name="value">Значение</param>
		void StoreInMap<T>(string keyOfMap, string keyInMap, T value) where T : class;

		/// <summary>
		/// Возвращает значение из элемента словаря
		/// </summary>
		/// <param name="keyOfMap">Ключ доступа к словарю</param>
		/// <param name="keyInMap">Ключ внутри словаря</param>
		T GetFromMap<T>(string keyOfMap, string keyInMap) where T : class;

		/// <summary>
		/// Сохраняет значение как элемент словаря
		/// </summary>
		/// <param name="keyOfMap">Ключ доступа к словарю</param>
		/// <param name="keyInMap">Ключ внутри словаря</param>
		/// <param name="value">Значение</param>
		void StoreInMapRaw(string keyOfMap, string keyInMap, string value);

		/// <summary>
		/// Возвращает значение из элемента словаря
		/// </summary>
		/// <param name="keyOfMap">Ключ доступа к словарю</param>
		/// <param name="keyInMap">Ключ внутри словаря</param>
		string GetFromMapRaw(string keyOfMap, string keyInMap);

		/// <summary>
		/// Сбрасывает кеш
		/// </summary>
		void Flush();
	}
}
