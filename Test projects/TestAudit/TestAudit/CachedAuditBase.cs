using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAudit
{
    public abstract class CachedAuditBase : AuditBase
    {
        private class CacheItem
        {
            public SqlCommand Command;

            public SqlParameter AuditType;

            public SqlParameter UserId;

            public SqlParameter EventId;

            public SqlParameter CardId;
            
            public SqlParameter Message;

            public CacheItem(string spName)
            {
                this.Command = new SqlCommand(spName);
                this.Command.CommandType = CommandType.StoredProcedure;
                this.AuditType = this.Command.Parameters.Add("@auditType", SqlDbType.TinyInt);
                this.UserId = this.Command.Parameters.Add("@userId", SqlDbType.Int);
                this.EventId = this.Command.Parameters.Add("@eventId", SqlDbType.UniqueIdentifier);
                this.CardId = this.Command.Parameters.Add("@cardId", SqlDbType.UniqueIdentifier);
                this.Message = this.Command.Parameters.Add("@message", SqlDbType.VarBinary, 2000);
            }
        }

        private ConcurrentQueue<CacheItem> _cache;

        protected CachedAuditBase(string spName)
            : base(spName)
        {
            _cache = new ConcurrentQueue<CacheItem>();
        }

        public override async Task WriteMessage(byte auditType, int? userId, Guid? eventId, Guid? cardId, string message)
        {
            CacheItem item = this.AcquireCacheItem();
            using (SqlConnection connection = new SqlConnection(this.ConnectionString))
            {
                await connection.OpenAsync();

                item.Command.Connection = connection;
                item.AuditType.Value = auditType;
                item.UserId.Value = (userId.HasValue ? (object)userId.Value : DBNull.Value);
                item.EventId.Value = (eventId.HasValue ? (object)eventId.Value : DBNull.Value);
                item.CardId.Value = (cardId.HasValue ? (object)cardId.Value : DBNull.Value);
                item.Message.Value = GetBinaryMessage(message);
                try
                {
                    await item.Command.ExecuteNonQueryAsync();
                }
                finally
                {
                    _cache.Enqueue(item);
                }
            }
        }

        private CacheItem AcquireCacheItem()
        {
            CacheItem item;
            if (!_cache.TryDequeue(out item))
            {
                item = new CacheItem(this.SpName);
            }
            return item;
        }

        public int GetCacheSize()
        {
            return this._cache.Count;
        }
    }
}
