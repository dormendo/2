using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LevensteinTestNet
{
	class Program
	{
		unsafe static void Main(string[] args)
		{
			int i11 = 0;
			int i12 = 0;
			int i13 = 4;
			i11 = i12 += i13;

			string str1 = "А A ТРИ ТРИ ТАНК ТАНК РУКА РУКА ВОДА ВОДА ЖАЛО ЖАЛО БАНК БАНК ВОДКА ВОДКА СТОЛБ СТОЛБ ИНВЕСТ ИНВЕСТ ЖИЛСНАБ ЖИЛСНАБ ГАЗПРОМ ГАЗПРОМ СБЕРБАНК СБЕРБАНК ИННОВАЦИИ ИННОВАЦИИ АВИАСТРОЕНИЕ АВИАСТРОЕНИЕ";
			string str2 = "ЖИЛСНАБ ВОДА ЖАЛО СБЕРБАНК ИННОВАЦИИ СТОЛБ ВОДКА ТРИ БАНК ТАНК ГАЗПРОМ ИНВЕСТ А РУКА АВИАСТРОЕНИЕ ЖИЛСНАБ ВОДА ЖАЛО СБЕРБАНК ИННОВАЦИИ СТОЛБ ВОДКА ТРИ БАНК ТАНК ГАЗПРОМ ИНВЕСТ А РУКА АВИАСТРОЕНИЕ";

			int iter = 100000;
			L l = new L(95, 5, 1, 1, 1, 85);
			L2 l2 = new L2(95, 5, 1, 1, 1, 85);
			L3 l3 = new L3(95, 5, 1, 1, 1, 85);
			L4 l4 = new L4(95, 5, 1, 1, 1, 85);
			L5 l5 = new L5(95, 5, 1, 1, 1, 85);
			L6 l6 = new L6(95, 5, 1, 1, 1, 85);
			L7 l7 = new L7(95, 5, 1, 1, 1, 85);
			L8 l8 = new L8(95, 5, 1, 1, 1, 85);
			double d;
			bool result = l.CalculateSimilarity("БАНК ГАЗПРОП", str1, out d);
			bool result2 = l2.CalculateSimilarity(new string[] { "БОНК", "ГАЗПРОМ" }, str1, out d);


			string sourceString = "БОРЩ ГОЗГЛОМ";
			string[] sourceArr = sourceString.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			List<GCHandle> handles = new List<GCHandle>();
			handles.Add(GCHandle.Alloc(sourceArr[0], GCHandleType.Pinned));
			handles.Add(GCHandle.Alloc(sourceArr[1], GCHandleType.Pinned));
			char*[] sourcePArr = new char*[] { (char*)handles[0].AddrOfPinnedObject(), (char*)handles[1].AddrOfPinnedObject() };
			int[] sourceLenArr = new int[] { 4, 7 };
			fixed (char** pSourceArr = sourcePArr)
			fixed (int* pSourceLenArr = sourceLenArr)
			{
				bool result3 = l3.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, str1, out d);
				bool result4 = l4.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, str1, out d);
				bool result5 = l5.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, str1, out d);
				bool result6 = l6.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, str1, out d);

				byte[] input7;
				using (MemoryStream ms = new MemoryStream())
				{
					using (CustomBinaryWriter w = new CustomBinaryWriter(ms))
					{ // А ТРИ БАНК ВОДКА СТОЛБ ИНВЕСТ ГАЗПРОМ СБЕРБАНК ИННОВАЦИИ АВИАСТРОЕНИЕ
						w.Write(30);
						w.Write(1);
						w.Write(1);
						w.Write(3);
						w.Write(3);
						w.Write(4);
						w.Write(4);
						w.Write(4);
						w.Write(4);
						w.Write(4);
						w.Write(4);
						w.Write(4);
						w.Write(4);
						w.Write(4);
						w.Write(4);
						w.Write(5);
						w.Write(5);
						w.Write(5);
						w.Write(5);
						w.Write(6);
						w.Write(6);
						w.Write(7);
						w.Write(7);
						w.Write(7);
						w.Write(7);
						w.Write(8);
						w.Write(8);
						w.Write(9);
						w.Write(9);
						w.Write(12);
						w.Write(12);
						w.WriteUnicodeString(str1.Replace(" ", ""));
					}

					input7 = ms.ToArray();
				}

				bool result7 = l7.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, input7, out d);
				Console.WriteLine(d);
				bool result8 = l8.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, input7, out d);
				Console.WriteLine(d);

				//GC.TryStartNoGCRegion(256 * 1024 * 1024, 256 * 1024 * 1024, false);
				Stopwatch sw = new Stopwatch();
				sw.Start();
				for (int i = 0; i < iter; i++)
				{
					l.CalculateSimilarity(sourceString, str2, out d);
				}
				sw.Stop();
				Console.WriteLine($"{sw.Elapsed}");

				sw.Restart();
				for (int i = 0; i < iter; i++)
				{
					l2.CalculateSimilarity(sourceArr, str2, out d);
				}
				sw.Stop();
				Console.WriteLine($"{sw.Elapsed}");

				sw.Restart();
				for (int i = 0; i < iter; i++)
				{
					l3.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, str2, out d);
				}
				sw.Stop();
				Console.WriteLine($"{sw.Elapsed}");

				sw.Restart();
				for (int i = 0; i < iter; i++)
				{
					l4.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, str1, out d);
				}
				sw.Stop();
				Console.WriteLine($"{sw.Elapsed}");

				sw.Restart();
				for (int i = 0; i < iter; i++)
				{
					l5.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, str1, out d);
				}
				sw.Stop();
				Console.WriteLine($"{sw.Elapsed}");

				sw.Restart();
				for (int i = 0; i < iter; i++)
				{
					l6.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, str1, out d);
				}
				sw.Stop();
				Console.WriteLine($"{sw.Elapsed}");

				sw.Restart();
				for (int i = 0; i < iter; i++)
				{
					l7.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, input7, out d);
				}
				sw.Stop();
				Console.WriteLine($"{sw.Elapsed}");

				sw.Restart();
				for (int i = 0; i < iter; i++)
				{
					l8.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, input7, out d);
				}
				sw.Stop();
				Console.WriteLine($"{sw.Elapsed}");
			}

			foreach (GCHandle handle in handles)
			{
				handle.Free();
			}

			Console.ReadLine();
		}
	}
}
