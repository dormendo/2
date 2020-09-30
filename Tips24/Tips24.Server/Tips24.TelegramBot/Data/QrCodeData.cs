using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.TelegramBot
{
	public class QrCodeData
	{
		public string FileId { get; set; }

		public byte[] StringHash { get; set; }

		public DateTime CreateDateTime { get; set; }

		public bool IsValid(byte[] currentHash)
		{
			if (DateTime.Now - this.CreateDateTime > TimeSpan.FromDays(30))
			{
				return false;
			}

			return CompareQrHashes(currentHash, this.StringHash);
		}

		private static unsafe bool CompareQrHashes(byte[] hash1, byte[] hash2)
		{
			if (hash1 == null || hash2 == null || hash1.Length != hash2.Length)
			{
				return false;
			}

			fixed (byte* p1 = hash1, p2 = hash2)
			{
				for (int i = 0; i < hash1.Length; i++)
				{
					if (p1[i] != p2[i])
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}
