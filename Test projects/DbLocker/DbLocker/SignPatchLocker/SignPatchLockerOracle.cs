using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using System.Data;

namespace DbLocker
{
	/// <summary>
	/// Сериализует подписание пакетов изменений одного классификатора
	/// </summary>
	internal class SignPatchLockerOracle : SignPatchLocker
	{
		#region Поля

		private string _lockHandle;

		private OracleConnection _conn;

		#endregion
		
		#region Конструктор
		
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="classifierId">Идентификатор классификатора</param>
		/// <param name="conn">Соединение с БД</param>
		internal SignPatchLockerOracle(int classifierId, IDbConnection conn)
			: base (classifierId)
		{
			this._conn = (OracleConnection)conn;
		}

		#endregion

		#region Методы

		private string GetResourceName()
		{
			return "test_SPL_" + this._classifierId.ToString();
		}

		/// <summary>
		/// Забирает блокировку
		/// </summary>
		protected override void Acquire()
		{
			using (OracleCommand cmd = new OracleCommand("SIGN_PATCH_LOCK_ACQUIRE", this._conn))
			{
				cmd.CommandType = System.Data.CommandType.StoredProcedure;
				cmd.CommandTimeout = 0;
				cmd.BindByName = true;

				OracleParameter resourceParam = new OracleParameter("p_LockName", OracleDbType.Varchar2);
				resourceParam.Value = this.GetResourceName();
				cmd.Parameters.Add(resourceParam);

				OracleParameter lockHandleParam = new OracleParameter("p_LockHandle", OracleDbType.Varchar2, 128);
				lockHandleParam.Direction = ParameterDirection.Output;
				cmd.Parameters.Add(lockHandleParam);

				OracleParameter codeParam = new OracleParameter("p_Code", OracleDbType.Int32);
				codeParam.Direction = ParameterDirection.Output;
				cmd.Parameters.Add(codeParam);

				cmd.ExecuteNonQuery();

				if (((Oracle.DataAccess.Types.OracleDecimal)codeParam.Value).ToInt32() != 0)
				{
					throw new Exception("ПЛОХО");
				}

				this._lockHandle = lockHandleParam.Value.ToString();
			}
		}

		#endregion
	}
}
