//using System;
//using System.Collections.Generic;
//using System.Data.SqlTypes;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.SqlServer.Server;

//    [Serializable]
//    [SqlUserDefinedAggregate(Format.UserDefined, IsInvariantToDuplicates = false, IsInvariantToNulls = true, IsInvariantToOrder = true, IsNullIfEmpty = false, MaxByteSize = -1)]
//    public class List : IBinarySerialize
//    {
//        private StringBuilder _result;

//        private bool _hasValues;
  
//        public void Init()
//        {
//            this._result = new StringBuilder(8000);
//        }
  
//        public void Accumulate(SqlString value)
//        {
//            if (value.IsNull)
//            {
//                return;
//            }
  
//            if (this._hasValues)
//            {
//                this._result.Append(",");
//            }
          
//            this._result.Append(value.Value);

//            this._hasValues = true;
//        }
  
//        public void Merge(List other)
//        {
//            if (this._hasValues && other._hasValues)
//            {
//                this._result.Append(",");
//            }
//            this._result.Append(other._result);
//        }
  
//        /// <summary>
//        /// Called at the end of aggregation, to return the results of the aggregation.
//        /// </summary>
//        /// <returns></returns>
//        public SqlString Terminate()
//        {
//            if (this._result != null && this._hasValues)
//            {
//                return new SqlString(this._result.ToString());
//            }
  
//            return new SqlString(string.Empty);
//        }
  
//        public void Read(BinaryReader r)
//        {
//            this._result = new StringBuilder(r.ReadString());
//            this._hasValues = this._result.Length > 0;
//        }
  
//        public void Write(BinaryWriter w)
//        {
//            w.Write(this._result.ToString());
//        }
//    }
