using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.PaymentService.YksSms
{
	public enum PaymentRequestStatus : byte
	{
		/// <summary>
		/// Поступил запрос от сотрудника
		/// </summary>
		Arrived = 0,
		
		/// <summary>
		/// Создание платежа завершилось ошибкой
		/// </summary>
		CreateFailed = 1,

		/// <summary>
		/// Создание платежа успешно завершено
		/// </summary>
		Created = 2,

		/// <summary>
		/// Платёж на стадии выполнения
		/// </summary>
		CheckPending = 3,

		/// <summary>
		/// Проверка завершилась ошибкой
		/// </summary>
		CheckFailed = 4,

		/// <summary>
		/// Платёж отклонён при обработке Яндекс.Кассой
		/// </summary>
		CanceledByKassa = 5,

		/// <summary>
		/// Платёж отклонён при обработке Яндекс.Кассой
		/// </summary>
		CanceledByUser = 6,

		/// <summary>
		/// Платёж проведён
		/// </summary>
		Accepted = 7
	}
}
