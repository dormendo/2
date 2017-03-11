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
//    public class ListInt3 : IBinarySerialize
//    {
//        private MemoryStream _ms;
//        private BinaryWriter _bw;

//        private static readonly byte _comma = Encoding.UTF8.GetBytes(",")[0];
//        private static readonly byte[] _empty = new byte[0];
  
//        public void Init()
//        {   
//            this.Initialize();
//        }

//        private void Initialize(int capacity = 8000)
//        {
//            this.CleanUp();
//            this._ms = new MemoryStream(capacity);
//            this._bw = new BinaryWriter(this._ms);
//        }
  
//        public void Accumulate(SqlInt32 value)
//        {
//            if (value.IsNull)
//            {
//                return;
//            }
               
//            this._bw.Write(value.Value.ToString());
//            this._bw.Write(_comma);
//        }
  
//        public void Merge(ListInt3 other)
//        {
//            byte[] b = other._ms.ToArray();
//            if (b.Length > 0)
//            {
//                this._ms.Write(b, 0, b.Length);
//            }
//        }
  
//        /// <summary>
//        /// Called at the end of aggregation, to return the results of the aggregation.
//        /// </summary>
//        /// <returns></returns>
//        public SqlBinary Terminate()
//        {
//            try
//            {
//                if (this._ms == null || this._bw == null || this._ms.Length == 0)
//                {
//                    return new SqlBinary(_empty);
//                }

//                this._ms.Position = 0;
//                byte[] buffer = new byte[this._ms.Length - 1];
//                this._ms.Read(buffer, 0, buffer.Length);
//                return new SqlBinary(buffer);
//            }
//            finally
//            {
//                this.CleanUp();
//            }
//        }

//        private void CleanUp()
//        {
//            if (this._bw != null)
//            {
//                this._bw.Dispose();
//                this._bw = null;
//            }
//            if (this._ms != null)
//            {
//                this._ms.Dispose();
//                this._ms = null;
//            }
//        }
  
//        public void Read(BinaryReader r)
//        {
//            this.Initialize((int)r.BaseStream.Length);
//            r.BaseStream.CopyTo(this._ms);
//        }
  
//        public void Write(BinaryWriter w)
//        {
//            if (this._ms != null && this._bw != null)
//            {
//                w.Write(this._ms.ToArray());
                
//                this.CleanUp();
//            }
//        }
//    }
