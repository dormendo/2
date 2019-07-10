using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace DbLocker
{
	/// <summary>
	/// Сериализует подписание пакетов изменений одного классификатора
	/// </summary>
	public abstract class SignPatchLocker
	{
		#region Поля
		
		/// <summary>
		/// Идентификатор классификатора
		/// </summary>
		protected int _classifierId;
		
		#endregion

		#region Создание экземпляра

		/// <summary>
		/// Создаёт экземпляр и блокирует ресурс
		/// </summary>
		/// <param name="classifierId">Идентификатор классификатора</param>
		/// <param name="conn">Соединение с БД</param>
		/// <returns>Экземпляр</returns>
		public static void Lock(int classifierId, IDbConnection conn)
		{
			SignPatchLocker locker;
			if (conn is SqlConnection)
			{
				locker = new SignPatchLockerMssql(classifierId, conn);
			}
			else
			{
				locker = new SignPatchLockerOracle(classifierId, conn);
			}
			
			locker.Acquire();
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="classifierId">Идентификатор классификатора</param>
		protected SignPatchLocker(int classifierId)
		{
			this._classifierId = classifierId;
		}

		#endregion

		#region Методы

		/// <summary>
		/// Забирает блокировку
		/// </summary>
		protected abstract void Acquire();

		#endregion
	}
}
