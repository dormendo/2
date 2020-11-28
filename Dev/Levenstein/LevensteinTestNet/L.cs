using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevensteinTestNet
{
	class L
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
		public L(int v_WordSimilarityKoeff, int v_WordExceedingKoeff, int v_max_transformations, int v_CHANGE, int v_ADDITION, int v_DELETION, int v_cuttingLimit)
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
		public L(int v_WordSimilatyKoeff, int v_WordExceedingKoeff, int v_CHANGE, int v_ADDITION, int v_DELETION, int v_cuttingLimit)
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
		public bool CalculateSimilarity(string Source, string Sample, out double SimilarityKoeff)
		{
			string[] SourceArr, SampleArr;

			int ldist;
			double KoeffEnd, KoeffTemp, temp1, temp2;

			SourceArr = Source.Split(spaceCharsToSplitBy, StringSplitOptions.RemoveEmptyEntries);
			SampleArr = Sample.Split(spaceCharsToSplitBy, StringSplitOptions.RemoveEmptyEntries);
			KoeffEnd = 0;
			for (int i = 0; i < SourceArr.Length; i++)
			{
				KoeffTemp = 0;
				for (int j = 0; j < SampleArr.Length; j++)
				{
					bool result = Levenshtein_Distance(SourceArr[i], SampleArr[j], out ldist);
					if (result)
					{
						temp1 = (SourceArr[i].Length + SampleArr[j].Length) / 2;
						temp2 = (1 + temp1) / (ldist + 1 + temp1);
						if (temp2 > KoeffTemp)
						{
							KoeffTemp = temp2;
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

		private bool Levenshtein_Distance(string Source, string Sample, out int ldist)
		{
			ldist = 0;
			int max_transformations;
			max_transformations = m_auto_max_transformations ? ((1 + (Source.Length) / 2) * (m_ADDITION + m_CHANGE + m_DELETION) / 3) : m_max_transformations;

			if (Math.Abs(Source.Length - Sample.Length) <= max_transformations)
				if ((ldist = l_dist_raw(Source, Sample, Source.Length, Sample.Length)) <= max_transformations)
					return (true);

			return (false);
		}

		private static int Smallest_Of(int x, int y, int z)
		{
			return (x < y) ? Math.Min(x, z) : Math.Min(y, z);
		}

		private int l_dist_raw(string Source, string Sample, int len_Source, int len_Sample)
		{
			//Console.WriteLine(Source + "-" + Sample);
			int i, j;
			int[] dist_im1 = new int[len_Sample + 2];
			int dist_i_jm1, dist_j0;
			int MinDist = 0;

			for (i = 1, dist_im1[0] = 0; i <= len_Sample; i++)
				dist_im1[i] = dist_im1[i - 1] + m_ADDITION;
			dist_j0 = 0;

			for (i = 1; i <= len_Source; i++)
			{
				dist_i_jm1 = dist_j0 += m_DELETION;
				for (j = 1; j <= len_Sample; j++)
				{
					MinDist = Smallest_Of(dist_im1[j - 1] + ((Source[i - 1] == Sample[j - 1]) ? 0 : m_CHANGE),
										   dist_i_jm1 + m_ADDITION,
										   dist_im1[j] + m_DELETION);
					dist_im1[j - 1] = dist_i_jm1;
					dist_i_jm1 = MinDist;
				}
				dist_im1[j - 1] = MinDist;
			}
			return MinDist;
		}
	}
}
