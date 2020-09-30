using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestClient
{
	class Utils
	{
		private static void TestSampleRate()
		{
			double sampleRate = 97.11111;
			int[] source = new int[10000];
			List<int> traced = new List<int>();
			List<int> notTraced = new List<int>();

			for (int i = 0; i < source.Length; i++)
			{
				source[i] = i + 1;
			}

			for (int i = 0; i < source.Length; i++)
			{
				double rateIfChosen = (traced.Count + 1) / (i + 1) * 100.0;
				double rateIfNotChosen = traced.Count / (i + 1) * 100.0;
				double deltaIfChosen = Math.Abs(rateIfChosen - sampleRate);
				double deltaIfNotChosen = Math.Abs(rateIfNotChosen - sampleRate);
				if (deltaIfNotChosen < deltaIfChosen)
				{
					notTraced.Add(source[i]);
				}
				else
				{
					traced.Add(source[i]);
				}
			}

			Console.WriteLine($"{sampleRate}, {((double)traced.Count) / source.Length * 100}");
		}
	}
}
