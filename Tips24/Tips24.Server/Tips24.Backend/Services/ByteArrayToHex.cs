using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.Backend
{
	public static class ByteArrayToHex
	{
		public static string ToHex(byte[] data)
		{
			return ToHex(data, "");
		}
		public static string ToHex(byte[] data, string prefix)
		{
			char[] lookup = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
			int i = 0, p = prefix.Length, l = data.Length;
			char[] c = new char[l * 2 + p];
			byte d;
			for (; i < p; ++i) c[i] = prefix[i];
			i = -1;
			--l;
			--p;
			while (i < l)
			{
				d = data[++i];
				c[++p] = lookup[d >> 4];
				c[++p] = lookup[d & 0xF];
			}
			return new string(c, 0, c.Length);
		}
		public static byte[] FromHex(string str)
		{
			return FromHex(str, 0, 0, 0);
		}
		public static byte[] FromHex(string str, int offset, int step)
		{
			return FromHex(str, offset, step, 0);
		}
		public static byte[] FromHex(string str, int offset, int step, int tail)
		{
			byte[] b = new byte[(str.Length - offset - tail + step) / (2 + step)];
			byte c1, c2;
			int l = str.Length - tail;
			int s = step + 1;
			for (int y = 0, x = offset; x < l; ++y, x += s)
			{
				c1 = (byte)str[x];
				if (c1 > 0x60) c1 -= 0x57;
				else if (c1 > 0x40) c1 -= 0x37;
				else c1 -= 0x30;
				c2 = (byte)str[++x];
				if (c2 > 0x60) c2 -= 0x57;
				else if (c2 > 0x40) c2 -= 0x37;
				else c2 -= 0x30;
				b[y] = (byte)((c1 << 4) + c2);
			}
			return b;
		}
	}
}
