using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAudit
{
    public abstract class AuditBase
    {
        protected string ConnectionString;

        protected string SpName;

        protected AuditBase(string spName)
        {
            this.ConnectionString = ConfigurationManager.ConnectionStrings["main"].ConnectionString;
            this.SpName = spName;
        }

        public virtual async Task WriteMessage(byte auditType, int? userId, Guid? eventId, Guid? cardId, string message)
        {
            using (SqlConnection connection = new SqlConnection(this.ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(this.SpName, connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@auditType", SqlDbType.TinyInt).Value = auditType;
                    //if (rec.UserId.HasValue)
                    //{
                    //    command.Parameters.Add("@userId", SqlDbType.Int).Value = rec.UserId.Value;
                    //}
                    //if (rec.EventId.HasValue)
                    //{
                    //    command.Parameters.Add("@eventId", SqlDbType.UniqueIdentifier).Value = rec.EventId.Value;
                    //}
                    //if (rec.CardId.HasValue)
                    //{
                    //    command.Parameters.Add("@cardId", SqlDbType.UniqueIdentifier).Value = rec.CardId.Value;
                    //}
                    //if (rec.Message != null)
                    //{
                    //    command.Parameters.Add("@message", SqlDbType.VarBinary).Value = GetMessage(rec.Message);
                    //}
                    command.Parameters.Add("@userId", SqlDbType.Int).Value = (userId.HasValue ? (object)userId.Value : DBNull.Value);
                    command.Parameters.Add("@eventId", SqlDbType.UniqueIdentifier).Value = (eventId.HasValue ? (object)eventId.Value : DBNull.Value);
                    command.Parameters.Add("@cardId", SqlDbType.UniqueIdentifier).Value = (cardId.HasValue ? (object)cardId.Value : DBNull.Value);
                    this.AddMessageParam(command, message);
                    
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        protected virtual void AddMessageParam(SqlCommand command, string message)
        {
            command.Parameters.Add("@message", SqlDbType.VarBinary, 2000).Value = GetBinaryMessage(message);
        }

        protected static byte[] GetBinaryMessage(string message)
        {
            byte[] ba = Encoding.UTF8.GetBytes(message);
            return ba;
        }
    }
}
