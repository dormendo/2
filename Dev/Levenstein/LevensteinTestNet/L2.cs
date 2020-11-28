using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace LevensteinTestNet
{
	class L2
	{

		private const int defaultMaxTransformations = 5;

		/// <summary>
		/// Автоматическое определение максимального числа преобразований.
		/// </summary>
		private bool m_auto_max_transformations = false;

		private int m_CHANGE = 1;

		private int m_ADDITION = 1;

		private int m_DELETION = 1;

		private int m_max_transformations = defaultMaxTransformations;
		/// <summary>
		/// Порог отсекания
		/// </summary>
		protected int m_cuttingLimit = 85;

		/// <summary>
		/// Какой вклад в общий коэффициент похожести вносят "похожие" слова,
		/// 1 - это значение, какой вклад вносят 
		/// </summary>
		protected int m_WordSimilarityKoeff = 75;
		/// <summary>
		/// Какой вклад в общий коэффициент похожести вносят "точно совпадающие" слова,
		/// 1 - это значение, какой вклад вносят 
		/// </summary>
		protected int m_WordExceedingKoeff = 25;

		/// <summary>
		/// Символы класса Space Characters, по которым необходимо разбивать строку на слова
		/// </summary>
		protected static char[] spaceCharsToSplitBy = new char[] { ' ', '\t', '\n', '\r' };

		/// <summary>
		/// Конструктор
		/// </summary>        
		public L2(int v_WordSimilarityKoeff, int v_WordExceedingKoeff, int v_max_transformations, int v_CHANGE, int v_ADDITION, int v_DELETION, int v_cuttingLimit)
		{
			if (v_WordSimilarityKoeff < 0 || v_WordSimilarityKoeff > 100)
			{
				throw new ArgumentException("1");
			}
			m_WordSimilarityKoeff = v_WordSimilarityKoeff;
			if (v_WordExceedingKoeff < 0 || v_WordExceedingKoeff > 100)
			{
				throw new ArgumentException("2");
			}
			m_WordExceedingKoeff = v_WordExceedingKoeff;
			if ((v_WordSimilarityKoeff + m_WordExceedingKoeff) != 100)
			{
				throw new ArgumentException("3");
			}
			if (v_max_transformations < 1)
			{
				throw new ArgumentException("4");
			}
			m_max_transformations = v_max_transformations;
			if (v_CHANGE < 1)
			{
				throw new ArgumentException("5");
			}
			m_CHANGE = v_CHANGE;
			if (v_ADDITION < 1)
			{
				throw new ArgumentException("6");
			}
			m_ADDITION = v_ADDITION;
			if (v_DELETION < 1)
			{
				throw new ArgumentException("7");
			}
			m_DELETION = v_DELETION;
			m_auto_max_transformations = false;
			if (v_cuttingLimit < 0 || v_cuttingLimit > 100)
			{
				throw new ArgumentException("8");
			}
			m_cuttingLimit = v_cuttingLimit;
		}

		/// <summary>
		/// Конструктор
		/// </summary>        
		public L2(int v_WordSimilatyKoeff, int v_WordExceedingKoeff, int v_CHANGE, int v_ADDITION, int v_DELETION, int v_cuttingLimit)
			: this(v_WordSimilatyKoeff, v_WordExceedingKoeff, defaultMaxTransformations, v_CHANGE, v_ADDITION, v_DELETION, v_cuttingLimit)
		{
		}

		/// <summary>
		/// Сравнить 2-е строки и вычислить коэффициент похожести
		/// </summary>
		/// <param name="Source">Кого сравниваем</param>
		/// <param name="Sample">С чем сравниваем</param>
		/// <param name="SimilarityKoeff">Коэффициент похожести</param>
		/// <returns>Включать ли сравниваемую строку в результат</returns>
		public unsafe bool CalculateSimilarity(string[] SourceArr, string Sample, out double SimilarityKoeff)
		{
			string[] SampleArr;

			int ldist;
			double KoeffEnd, KoeffTemp, temp1, temp2;

			SampleArr = Sample.Split(spaceCharsToSplitBy, StringSplitOptions.RemoveEmptyEntries);
			int maxSampleWordLength = 0;
			for (int i = 0; i < SampleArr.Length; i++)
			{
				if (maxSampleWordLength < SampleArr[i].Length)
				{
					maxSampleWordLength = SampleArr[i].Length;
				}
			}
			//Array.Sort(SampleArr, (x, y) => x.Length.CompareTo(y.Length));


			int* dist_im1 = stackalloc int[maxSampleWordLength + 1];

			
			KoeffEnd = 0;
			for (int i = 0; i < SourceArr.Length; i++)
			{
				KoeffTemp = 0;

				int sourceWordLength = SourceArr[i].Length;
				fixed (char* sourceWord = SourceArr[i])
				{
					//Array.Sort(SampleArr, (x, y) => Math.Abs(x.Length - sourceWordLength).CompareTo(Math.Abs(y.Length - sourceWordLength)));


					for (int j = 0; j < SampleArr.Length; j++)
					{
						bool result = Levenshtein_Distance(sourceWord, sourceWordLength, SampleArr[j], out ldist, dist_im1);
						if (result)
						{
							temp1 = (SourceArr[i].Length + SampleArr[j].Length) / 2 + 1;
							temp2 = (temp1) / (ldist + temp1);
							if (temp2 > KoeffTemp)
							{
								KoeffTemp = temp2;
							}
							if (temp2 == 1)
							{
								break;
							}
						}
					}
				}
				KoeffEnd += KoeffTemp;
			}

			if (SourceArr.Length != 0)
				KoeffEnd = m_WordSimilarityKoeff * 1.0 * KoeffEnd / SourceArr.Length;
			else
				KoeffEnd = 0;
			if (KoeffEnd > 0)
				KoeffEnd += m_WordExceedingKoeff * 1.0 / (Math.Abs(SourceArr.Length - SampleArr.Length) + 1);

			SimilarityKoeff = KoeffEnd;

			return (SimilarityKoeff >= m_cuttingLimit);
		}

		[TargetedPatchingOptOut("speed")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe bool Levenshtein_Distance(char* sourceWord, int sourceWordLength, string Sample, out int ldist, int* dist_im1)
		{
			ldist = 0;
			int max_transformations;
			max_transformations = m_auto_max_transformations ? ((1 + (sourceWordLength) / 2) * (m_ADDITION + m_CHANGE + m_DELETION) / 3) : m_max_transformations;

			if (Math.Abs(sourceWordLength - Sample.Length) <= max_transformations)
				if ((ldist = l_dist_raw(sourceWord, sourceWordLength, Sample, dist_im1)) <= max_transformations)
					return (true);

			return (false);
		}

		[TargetedPatchingOptOut("speed")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe int l_dist_raw(char* sourceWord, int sourceWordLength, string strSample, int* dist_im1)
		{
			//Console.WriteLine(new string(sourceWord, 0, sourceWordLength) + "-" + strSample);
			int i, j;
			int len_Sample = strSample.Length;

			int dist_i_jm1, dist_j0;
			int MinDist = 0;

			int accumulateAdditions = 0;
			for (i = 0; i <= len_Sample; i++)
			{
				dist_im1[i] = accumulateAdditions;
				accumulateAdditions += m_ADDITION;
			}

			dist_j0 = 0;

			fixed (char* Sample = strSample)
			{
				for (i = 0; i < sourceWordLength; i++)
				{
					char sourceChar = sourceWord[i];
					dist_i_jm1 = dist_j0 += m_DELETION;
					for (j = 0; j < len_Sample; j++)
					{
						int x = dist_im1[j] + (sourceChar == Sample[j] ? 0 : m_CHANGE);
						int y = dist_i_jm1 + m_ADDITION;
						int z = dist_im1[j + 1] + m_DELETION;
						MinDist = x < y ? Math.Min(x, z) : Math.Min(y, z);
						dist_im1[j] = dist_i_jm1;
						dist_i_jm1 = MinDist;
					}
					dist_im1[j] = MinDist;
				}
			}
			return MinDist;
		}
	}
}
