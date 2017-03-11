using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;

    [Serializable]
    [SqlUserDefinedAggregate(Format.UserDefined, IsInvariantToDuplicates = false, IsInvariantToNulls = true, IsInvariantToOrder = true, IsNullIfEmpty = false, MaxByteSize = -1)]
    public class ListSmallint : IBinarySerialize
    {
        private List<short> _list;
  
        public void Init()
        {
            this.Initialize();
        }


        private void Initialize(int capacity = 1000)
        {
            if (this._list != null)
            {
                this._list.Clear();
            }
            else
            {
                this._list = new List<short>(capacity);
            }
        }
  
        public void Accumulate(SqlInt16 value)
        {
            if (value.IsNull)
            {
                return;
            }
               
            this._list.Add(value.Value);
        }
  
        public void Merge(ListSmallint other)
        {
            this._list.AddRange(other._list);
        }
  
        /// <summary>
        /// Called at the end of aggregation, to return the results of the aggregation.
        /// </summary>
        /// <returns></returns>
        public SqlString Terminate()
        {
            if (this._list != null && this._list.Count > 0)
            {
                StringBuilder sb = new StringBuilder(11 * this._list.Count);
                for (int i = 0; i < this._list.Count; i++)
                {
                    sb.Append(this._list[i]).Append(",");
                }
                
                this._list.Clear();

                return new SqlString(sb.ToString(0, sb.Length - 1));
            }
  
            return new SqlString(null);
        }
  
        public void Read(BinaryReader r)
        {
            int l = r.ReadInt32();

            if (l < 0)
            {
                this._list = null;
            }
            else
            {
                this.Initialize(l < 32 ? 32 : l);
                for (int i = 0; i < l; i++)
                {
                    this._list.Add(r.ReadInt16());
                }
            }
        }
  
        public void Write(BinaryWriter w)
        {
            if (this._list == null)
            {
                w.Write((int)0);
            }
            else
            {
                w.Write(this._list.Count);
            }

            for (int i = 0; i < this._list.Count; i++)
            {
                w.Write(this._list[i]);
            }
        }
    }
