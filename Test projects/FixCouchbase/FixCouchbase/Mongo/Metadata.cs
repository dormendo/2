using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixCouchbase
{
	public class Metadata
	{
		public string Key { get; private set; }

		public byte[] Value { get; private set; }

		public unsafe Metadata(string key, int startingInt)
		{
			this.Key = key;

			this.Value = new byte[32 * 1024];
			fixed (byte* bp = this.Value)
			{
				int* ip = (int*)bp;
				for (int i = 0; i < 32 * 1024 / 4; i++)
				{
					ip[i] = startingInt + i;
				}
			}
		}

		private Metadata(string key, byte[] cache)
		{
			this.Key = key;
			this.Value = cache;
		}

		public BsonDocument ToBsonDocument()
		{
			return new BsonDocument { { "_id", this.Key }, { "value", new BsonBinaryData(this.Value) } };
		}

		public static Metadata FromBsonDocument(BsonDocument bd)
		{
			string key = bd.GetValue("_id").AsString;
			byte[] cache = bd.GetValue("value")?.AsBsonBinaryData?.Bytes;

			return new Metadata(key, cache);
		}

		public unsafe bool CheckOffset(int startingInt)
		{
			if (this.Value == null || this.Value.Length != 32 * 1024)
			{
				return false;
			}

			fixed (byte* bp = this.Value)
			{
				int* ip = (int*)bp;
				for (int i = 0; i < 32 * 1024 / 4; i++)
				{
					if (ip[i] != startingInt + i)
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}
