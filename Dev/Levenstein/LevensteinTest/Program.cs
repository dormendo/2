using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LevensteinTest
{
	class Program
	{
		unsafe static void Main(string[] args)
		{
			int i11 = 0;
			int i12 = 0;
			int i13 = 4;
			i11 = i12 += i13;


			int iter = 100000;
			L l = new L(95, 5, 1, 1, 1, 85);
			L2 l2 = new L2(95, 5, 1, 1, 1, 85);
			L3 l3 = new L3(95, 5, 1, 1, 1, 85);
			L4 l4 = new L4(95, 5, 1, 1, 1, 85);
			L5 l5 = new L5(95, 5, 1, 1, 1, 85);
			L6 l6 = new L6(95, 5, 1, 1, 1, 85);
			L7 l7 = new L7(95, 5, 1, 1, 1, 85);
			double d;
			bool result = l.CalculateSimilarity("БАНК ГАЗПРОП", "ТРИ БАНК ВОДКА СТОЛБ ИНВЕСТ ГАЗПРОМ СБЕРБАНК ИННОВАЦИИ", out d);
			bool result2 = l2.CalculateSimilarity(new string[] { "БАНК", "ГАЗПРОП" }, "ТРИ БАНК ВОДКА СТОЛБ ИНВЕСТ ГАЗПРОМ СБЕРБАНК ИННОВАЦИИ", out d);

			string[] sourceArr = new string[] { "БАНК", "ГАЗПРОП" };
			List<GCHandle> handles = new List<GCHandle>();
			handles.Add(GCHandle.Alloc(sourceArr[0], GCHandleType.Pinned));
			handles.Add(GCHandle.Alloc(sourceArr[1], GCHandleType.Pinned));
			char*[] sourcePArr = new char*[] { (char*)handles[0].AddrOfPinnedObject(), (char*)handles[1].AddrOfPinnedObject() };
			int[] sourceLenArr = new int[] { 4, 7 };
			fixed (char** pSourceArr = sourcePArr)
			fixed (int* pSourceLenArr = sourceLenArr)
			{
				bool result3 = l3.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, "ТРИ БАНК ВОДКА СТОЛБ ИНВЕСТ ГАЗПРОМ СБЕРБАНК ИННОВАЦИИ", out d);
				bool result4 = l4.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, "ТРИ БАНК ВОДКА СТОЛБ ИНВЕСТ ГАЗПРОМ СБЕРБАНК ИННОВАЦИИ", out d);
				bool result5 = l5.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, "ТРИ БАНК ВОДКА СТОЛБ ИНВЕСТ ГАЗПРОМ СБЕРБАНК ИННОВАЦИИ", out d);
				bool result6 = l6.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, "ТРИ БАНК ВОДКА СТОЛБ ИНВЕСТ ГАЗПРОМ СБЕРБАНК ИННОВАЦИИ", out d);

				byte[] input7;
				using (MemoryStream ms = new MemoryStream())
				{
					using (CustomBinaryWriter w = new CustomBinaryWriter(ms))
					{ // А ТРИ БАНК ВОДКА СТОЛБ ИНВЕСТ ГАЗПРОМ СБЕРБАНК ИННОВАЦИИ АВИАСТРОЕНИЕ
						w.Write(10);
						w.Write(1);
						w.Write(3);
						w.Write(4);
						w.Write(5);
						w.Write(5);
						w.Write(6);
						w.Write(7);
						w.Write(8);
						w.Write(9);
						w.Write(12);
						w.WriteUnicodeString("АТРИБАНКВОДКАСТОЛБИНВЕСТГАЗПРОМСБЕРБАНКИННОВАЦИИАВИАСТРОЕНИЕ");
					}

					input7 = ms.ToArray();
				}

				bool result7 = l7.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, input7, out d);

				Stopwatch sw = new Stopwatch();
				sw.Start();
				for (int i = 0; i < iter; i++)
				{
					l.CalculateSimilarity("БАНК ГАЗПРОП", "СБЕРБАНК ИННОВАЦИИ СТОЛБ ВОДКА ТРИ БАНК ГАЗПРОМ ИНВЕСТ А АВИАСТРОЕНИЕ", out d);
				}
				sw.Stop();
				Console.WriteLine($"{sw.Elapsed}");

				sw.Restart();
				for (int i = 0; i < iter; i++)
				{
					l2.CalculateSimilarity(sourceArr, "СБЕРБАНК ИННОВАЦИИ СТОЛБ ВОДКА ТРИ БАНК ГАЗПРОМ ИНВЕСТ А АВИАСТРОЕНИЕ", out d);
				}
				sw.Stop();
				Console.WriteLine($"{sw.Elapsed}");

				sw.Restart();
				for (int i = 0; i < iter; i++)
				{
					l3.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, "СБЕРБАНК ИННОВАЦИИ СТОЛБ ВОДКА ТРИ БАНК ГАЗПРОМ ИНВЕСТ А АВИАСТРОЕНИЕ", out d);
				}
				sw.Stop();
				Console.WriteLine($"{sw.Elapsed}");

				sw.Restart();
				for (int i = 0; i < iter; i++)
				{
					l4.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, "А ТРИ БАНК ВОДКА СТОЛБ ИНВЕСТ ГАЗПРОМ СБЕРБАНК ИННОВАЦИИ АВИАСТРОЕНИЕ", out d);
				}
				sw.Stop();
				Console.WriteLine($"{sw.Elapsed}");

				sw.Restart();
				for (int i = 0; i < iter; i++)
				{
					l5.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, "А ТРИ БАНК ВОДКА СТОЛБ ИНВЕСТ ГАЗПРОМ СБЕРБАНК ИННОВАЦИИ АВИАСТРОЕНИЕ", out d);
				}
				sw.Stop();
				Console.WriteLine($"{sw.Elapsed}");

				sw.Restart();
				for (int i = 0; i < iter; i++)
				{
					l6.CalculateSimilarity(pSourceArr, pSourceLenArr, 2, "А ТРИ БАНК ВОДКА СТОЛБ ИНВЕСТ ГАЗПРОМ СБЕРБАНК ИННОВАЦИИ АВИАСТРОЕНИЕ", out d);
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
			}

			foreach (GCHandle handle in handles)
			{
				handle.Free();
			}

			Console.ReadLine();
		}
	}
}
