using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Maxima.DataAccess
{
    public sealed class AdoWorker<T> : AdoWorker
    {
        private readonly Func<SqlDataReader, T> _mapper;
        private readonly string _commandText;
        private readonly object[] _queryParams;

        public AdoWorker(SqlConnection conn, SqlTransaction transaction, Func<SqlDataReader, T> mapper, string commandText, params object[] queryParams)
            : base(conn, transaction)
        {
            _mapper = mapper;
            _commandText = commandText;
            _queryParams = queryParams != null && queryParams.Length > 0
                ? queryParams
                : null;
        }

        public List<T> GetQueryResult()
        {
            AssertValidConnectionState(Connection);
            using (var cmd = GetCommand(_commandText))
            {
                SetParameters(cmd, _queryParams, false);
                return ReadResult(cmd);
            }
        }

        public List<T> ExecuteStoredProcedureQuery()
        {
            VeriyActualParametersType();
            AssertValidConnectionState(Connection);
            using (var cmd = GetCommand(_commandText))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                var outParams = SetParameters(cmd, _queryParams, true);
                var result = ReadResult(cmd);
                outParams.ForEach(p => p.ReadOutputValue());
                return result;
            }
        }

        [Conditional("DEBUG")]
        private void VeriyActualParametersType()
        {
            if(_queryParams != null && !_queryParams.All(qp => qp is Tuple<string, object>))
                throw new ApplicationException("Stored procedure call requires named parameters, that is, all parameters must be of type Tuple<string, object>");
        }

        private List<T> ReadResult(SqlCommand cmd)
        {
            var result = new List<T>();
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    result.Add(_mapper(reader));
            return result;
        }

        public Task<List<T>> GetQueryResultAsync()
        {
            return Task<List<T>>.Factory.StartNew(GetQueryResult);
        }

        public Task<List<T>> ExecuteStoredProcedureQueryAsync()
        {
            return Task<List<T>>.Factory.StartNew(ExecuteStoredProcedureQuery);            
        }
    }
}