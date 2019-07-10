using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace DbLocker
{
	/// <summary>
	/// Сериализует подписание пакетов изменений одного классификатора
	/// </summary>
	internal class SignPatchLockerMssql : SignPatchLocker
	{
		#region Поля

		private SqlConnection _conn;

		#endregion

		#region Конструктор
		
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="classifierId">Идентификатор классификатора</param>
		/// <param name="conn">Соединение с БД</param>
		internal SignPatchLockerMssql(int classifierId, IDbConnection conn)
			: base (classifierId)
		{
			this._conn = (SqlConnection)conn;
		}

		#endregion

		#region Методы

		private string GetResourceName()
		{
			return "test_SIGNPATCH_LOCK_" + this._classifierId.ToString();
		}

		/// <summary>
		/// Забирает блокировку
		/// </summary>
		protected override void Acquire()
		{
			using (SqlCommand cmd = new SqlCommand("sp_getapplock", this._conn))
			{
				cmd.CommandType = System.Data.CommandType.StoredProcedure;
				cmd.CommandTimeout = 0;

				SqlParameter resourceParam = new SqlParameter("@Resource", SqlDbType.NVarChar, 255);
				resourceParam.Value = this.GetResourceName();
				cmd.Parameters.Add(resourceParam);

				SqlParameter lockModeParam = new SqlParameter("@LockMode", SqlDbType.NVarChar, 32);
				lockModeParam.Value = "Exclusive";
				cmd.Parameters.Add(lockModeParam);

				SqlParameter lockOwnerParam = new SqlParameter("@LockOwner", SqlDbType.NVarChar, 32);
				lockOwnerParam.Value = "Session";
				cmd.Parameters.Add(lockOwnerParam);

				SqlParameter returnValue = new SqlParameter();
				returnValue.Direction = ParameterDirection.ReturnValue;
				cmd.Parameters.Add(returnValue);

				cmd.ExecuteNonQuery();

				if (Convert.ToInt32(returnValue.Value) < 0)
				{
					throw new Exception("ПЛОХО");
				}
			}
		}
		#endregion
	}
}
