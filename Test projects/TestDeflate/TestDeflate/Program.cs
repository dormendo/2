using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDeflate
{
	class Program
	{
		public static void Main()
		{
			string e  = Path.GetExtension("");

			string dir = "C:\\Projects\\TFS\\NORMA2\\NORMA2_CB\\NORMA_BRANCH_2010_10_13\\Lanit.Norma.AppServer\\Resources";
			Compress(dir, "AccessEmpty.accdb", "AccessEmpty12.bin");
			Compress(dir, "AccessEmpty.mdb", "AccessEmpty8.bin");
			Decompress(dir, "AccessEmpty12.bin", "AccessEmpty2.accdb");
			Decompress(dir, "AccessEmpty8.bin", "AccessEmpty2.mdb");

			bool b1 = Compare(dir, "AccessEmpty.accdb", "AccessEmpty2.accdb");
			bool b2 = Compare(dir, "AccessEmpty.mdb", "AccessEmpty2.mdb");
		}

		public static void Compress(string dir, string source, string dest)
		{
			using (FileStream sourceFs = new FileStream(Path.Combine(dir, source), FileMode.Open, FileAccess.Read))
			{
				using (FileStream compressedFileStream = new FileStream(Path.Combine(dir, dest), FileMode.OpenOrCreate, FileAccess.Write))
				{
					using (DeflateStream compressionStream = new DeflateStream(compressedFileStream, CompressionMode.Compress))
					{
						sourceFs.CopyTo(compressionStream);
					}
				}
			}
		}

		public static void Decompress(string dir, string source, string dest)
		{
			using (FileStream originalFileStream = new FileStream(Path.Combine(dir, source), FileMode.Open, FileAccess.Read))
			{
				using (FileStream decompressedFileStream = new FileStream(Path.Combine(dir, dest), FileMode.OpenOrCreate, FileAccess.Write)) 
				{
					using (DeflateStream decompressionStream = new DeflateStream(originalFileStream, CompressionMode.Decompress))
					{
						decompressionStream.CopyTo(decompressedFileStream);
					}
				}
			}
		}

		public static bool Compare(string dir, string file1, string file2)
		{
			byte[] ba1 = File.ReadAllBytes(Path.Combine(dir, file1));
			byte[] ba2 = File.ReadAllBytes(Path.Combine(dir, file2));
			if (ba1.Length != ba2.Length)
			{
				return false;
			}

			for (int i = 0; i < ba1.Length; i++)
			{
				if (ba1[i] != ba2[i])
				{
					return false;
				}
			}

			return true;
		}
	}
}
