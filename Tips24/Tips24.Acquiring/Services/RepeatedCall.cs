using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.Acquiring
{
	/// <summary>
	/// Вызывает метод несколько раз подряд с заданным интервалом до первого успешного вызова
	/// </summary>
	public static class RepeatedCall
	{
		/// <summary>
		/// Вызывает делегат, не принимающий параметров и не возвращающий значений.
		/// Удачной попыткой считается вызов, в результате которого не вброшено исключение.
		/// </summary>
		/// <param name="action">Делегат вызова</param>
		/// <param name="tryCount">Количество попыток</param>
		/// <param name="interval">Интервал между попытками в миллисекундах</param>
		public static async Task<T> ActionAsync<T>(Func<Task<T>> action, int tryCount, int interval)
		{
			return await ActionAsync(action, tryCount, interval, null, null);
		}

		/// <summary>
		/// Вызывает делегат, не принимающий параметров и не возвращающий значений.
		/// Удачной попыткой считается вызов, в результате которого не вброшено исключение.
		/// </summary>
		/// <param name="action">Делегат вызова</param>
		/// <param name="tryCount">Количество попыток</param>
		/// <param name="interval">Интервал между попытками в миллисекундах</param>
		public static async Task ActionAsync(Func<Task> action, int tryCount, int interval)
		{
			await ActionAsync(async () => { await action(); return true; }, tryCount, interval, null, null);
		}

		/// <summary>
		/// Вызывает делегат, не принимающий параметров и не возвращающий значений.
		/// Удачной попыткой считается вызов, в результате которого не вброшено исключение.
		/// </summary>
		/// <param name="action">Делегат вызова</param>
		/// <param name="tryCount">Количество попыток</param>
		/// <param name="interval">Интервал между попытками в миллисекундах</param>
		/// <param name="exceptionalAction">Делегат вызова для обработки исключения</param>
		/// <param name="tryCountKey">Ключ для записи количества попыток в словарь Exception.Data</param>
		public static async Task<T> ActionAsync<T>(Func<Task<T>> action, int tryCount, int interval, Action<Exception> exceptionalAction, string tryCountKey)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			if (tryCount < 1)
			{
				throw new ArgumentException("tryCount");
			}
			if (interval < 0)
			{
				throw new ArgumentException("interval");
			}

			for (int i = 0; i < tryCount; i++)
			{
				try
				{
					return await action();
				}
				catch (Exception ex)
				{
					if (!string.IsNullOrEmpty(tryCountKey))
					{
						ex.Data[tryCountKey] = i + 1;
					}
					if (exceptionalAction != null)
					{
						exceptionalAction(ex);
					}
					if (i == tryCount - 1)
					{
						throw;
					}
				}
				if (interval > 0)
				{
					await Task.Delay(interval);
				}
			}

			return default(T);
		}
	}
}
