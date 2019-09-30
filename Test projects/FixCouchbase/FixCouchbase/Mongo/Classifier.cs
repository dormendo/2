using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixCouchbase
{
	public class Classifier
	{
		public enum Type : byte
		{
			CProps = 1,
			CElements = 2,
			CLinks = 3,
			EProps = 4,
			EExtCols = 5,
			ELinks = 6,
			EAncLinks = 7
		}

		public string Key { get; private set; }

		public Type CacheType { get; private set; }

		public int ClassifierId { get; private set; }

		public int? ElementId { get; private set; }

		public Guid? ElementGuid { get; private set; }

		public byte[] Value { get; private set; }

		public Classifier(Type cacheType, int classifierId)
			: this(null, cacheType, classifierId, null, null, null)
		{
		}

		public Classifier(Type cacheType, int classifierId, int elementId, Guid elementGuid)
			: this(null, cacheType, classifierId, elementId, elementGuid, null)
		{
		}

		private unsafe Classifier(string key, Type cacheType, int classifierId, int? elementId, Guid? elementGuid, byte[] value)
		{
			this.CacheType = cacheType;
			this.ClassifierId = classifierId;
			this.ElementId = elementId;
			this.ElementGuid = elementGuid;
			this.Key = (key ?? this.GetKey());

			if (value != null)
			{
				this.Value = value;
			}
			else
			{
				this.Value = new byte[32 * 1024];
				fixed (byte* bp = this.Value)
				{
					int* ip = (int*)bp;
					for (int i = 0; i < 32 * 1024 / 4; i++)
					{
						ip[i] = i;
					}
				}
			}
		}

		private string GetKey()
		{
			switch (this.CacheType)
			{
				case Type.CProps:
					return "cp" + this.ClassifierId.ToString();
				case Type.CElements:
					return "ce" + this.ClassifierId.ToString();
				case Type.CLinks:
					return "cl" + this.ClassifierId.ToString();
				case Type.EProps:
					return "ep" + this.ElementId.ToString();
				case Type.EExtCols:
					return "ec" + this.ElementId.ToString();
				case Type.ELinks:
					return "el" + this.ElementId.ToString();
				default:
					return "ea" + this.ElementId.ToString();
			}
		}

		public BsonDocument ToBsonDocument()
		{
			if (this.ElementId.HasValue)
			{
				return new BsonDocument {
					{ "_id", this.Key },
					{ "t", (int)this.CacheType },
					{ "c", this.ClassifierId },
					{ "e", this.ElementId.Value },
					{ "eg", new BsonBinaryData(this.ElementGuid.Value) },
					{ "v", new BsonBinaryData(this.Value) }
				};
			}
			else
			{
				return new BsonDocument {
					{ "_id", this.Key },
					{ "t", (int)this.CacheType },
					{ "c", this.ClassifierId },
					{ "v", new BsonBinaryData(this.Value) }
				};
			}
		}

		public static Classifier FromBsonDocument(BsonDocument bd)
		{
			string key = bd.GetValue("_id").AsString;
			Type type = (Type)bd.GetValue("t").AsInt32;
			int classifierId = bd.GetValue("c").AsInt32;
			int? elementId = null;
			Guid? elementGuid = null;
			if (type == Type.EAncLinks || type == Type.EExtCols || type == Type.ELinks || type == Type.EProps)
			{
				elementId = bd.GetValue("e").AsInt32;
				elementGuid = bd.GetValue("eg").AsGuid;
			}

			byte[] cache = bd.GetValue("v")?.AsBsonBinaryData?.Bytes;

			return new Classifier(key, type, classifierId, elementId, elementGuid, cache);
		}

		internal BsonDocument GetFindByIdBson()
		{
		 	return new BsonDocument { { "_id", this.Key } };
		}

		internal BsonDocument GetFindByClassifierIdBson()
		{
			return new BsonDocument { { "c", this.ClassifierId }, { "t", (int)this.CacheType } };
		}

		internal BsonDocument GetFindAllByClassifierIdBson()
		{
			return new BsonDocument { { "c", this.ClassifierId } };
		}

		internal bool Compare(Classifier c1)
		{
			if (c1 == null || c1.Value == null)
			{
				return false;
			}

			if (c1.CacheType != this.CacheType || c1.ClassifierId != this.ClassifierId || c1.ElementId != this.ElementId || c1.ElementGuid != this.ElementGuid)
			{
				return false;
			}

			return CompareBuffers(this.Value, c1.Value);
		}

		private static unsafe bool CompareBuffers(byte[] b1, byte[] b2)
		{
			if (b1.Length != b2.Length)
			{
				return false;
			}

			fixed (byte* bp1 = b1, bp2 = b2)
			{
				for (int i = 0; i < b1.Length; i++)
				{
					if (bp1[i] != bp2[i])
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}
