using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.Backend
{
	public class AuthKey
	{
		private string _keyStr;

		private byte[] _key;

		public static AuthKey Create(string keyStr)
		{
			if (keyStr == null || keyStr.Length != 32)
			{
				return null;
			}

			byte[] binary = ByteArrayToHex.FromHex(keyStr);
			return new AuthKey(binary, keyStr);
		}

		public static AuthKey Create(byte[] key)
		{
			if (key == null || key.Length != 16)
			{
				return null;
			}

			return new AuthKey(key, null);
		}

		private AuthKey(byte[] binary, string keyStr)
		{
			this._keyStr = keyStr;
			this._key = binary;
		}

		public byte[] ToArray()
		{
			return this._key;
		}

		public override string ToString()
		{
			if (this._keyStr == null)
			{
				this._keyStr = ByteArrayToHex.ToHex(this._key);
			}

			return this._keyStr;
		}
	}
}
