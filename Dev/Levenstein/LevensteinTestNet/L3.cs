﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace LevensteinTestNet
{
	unsafe class L3
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

		int[] offsetList = new int[100000];
		int[] lenList = new int[100000];
		int[] dist_im1_array = new int[10000];

		int* pOffsetArr;
		int* pLenArr;
		int* dist_im1;

		/// <summary>
		/// Конструктор
		/// </summary>
		public L3(int v_WordSimilarityKoeff, int v_WordExceedingKoeff, int v_max_transformations, int v_CHANGE, int v_ADDITION, int v_DELETION, int v_cuttingLimit)
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

			pOffsetArr = (int*)GCHandle.Alloc(offsetList, GCHandleType.Pinned).AddrOfPinnedObject();
			pLenArr = (int*)GCHandle.Alloc(lenList, GCHandleType.Pinned).AddrOfPinnedObject();
			dist_im1 = (int*)GCHandle.Alloc(dist_im1_array, GCHandleType.Pinned).AddrOfPinnedObject();
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		public L3(int v_WordSimilatyKoeff, int v_WordExceedingKoeff, int v_CHANGE, int v_ADDITION, int v_DELETION, int v_cuttingLimit)
			: this(v_WordSimilatyKoeff, v_WordExceedingKoeff, defaultMaxTransformations, v_CHANGE, v_ADDITION, v_DELETION, v_cuttingLimit)
		{
		}

		/// <summary>
		/// Сравнить 2-е строки и вычислить коэффициент похожести
		/// </summary>
		/// <returns>Включать ли сравниваемую строку в результат</returns>
		public unsafe bool CalculateSimilarity(char** pSourceArr, int* pSourceLenArr, int sourceArrLen, string Sample, out double SimilarityKoeff)
		{
			int ldist;
			double KoeffEnd, KoeffTemp, temp1, temp2;

			fixed (char* pSample = Sample)
			{
				int sampleWordCount, maxSampleWordLength;
				this.ParseString(pSample, Sample.Length, out sampleWordCount, out maxSampleWordLength);

				KoeffEnd = 0;
				for (int i = 0; i < sourceArrLen; i++)
				{
					KoeffTemp = 0;

					int sourceWordLength = pSourceLenArr[i];
					char* sourceWord = pSourceArr[i];
					for (int j = 0; j < sampleWordCount; j++)
					{
						bool result = Levenshtein_Distance(sourceWord, sourceWordLength, pSample + *(pOffsetArr + j), *(pLenArr + j), out ldist, dist_im1);
						if (result)
						{
							temp1 = (sourceWordLength + *(pLenArr + j)) / 2 + 1;
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
					KoeffEnd += KoeffTemp;
				}

				if (sourceArrLen != 0)
					KoeffEnd = m_WordSimilarityKoeff * 1.0 * KoeffEnd / sourceArrLen;
				else
					KoeffEnd = 0;
				if (KoeffEnd > 0)
					KoeffEnd += m_WordExceedingKoeff * 1.0 / (Math.Abs(sourceArrLen - sampleWordCount) + 1);

				SimilarityKoeff = KoeffEnd;
			}

			return (SimilarityKoeff >= m_cuttingLimit);
		}

		[TargetedPatchingOptOut("speed")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe void ParseString(char* pSample, int len, out int wordCount, out int maxWordLen)
		{
			maxWordLen = 0;
			wordCount = 0;
			bool isSpace = true;
			int offset = 0;
			for (int i = 0; i < len; i++)
			{
				char c = *(pSample + i);
				if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
				{
					if (!isSpace)
					{
						int wordLen = i - offset;
						pOffsetArr[wordCount] = offset;
						pLenArr[wordCount] = wordLen;
						if (wordLen > maxWordLen)
						{
							maxWordLen = wordLen;
						}
						wordCount++;
					}

					isSpace = true;
				}
				else
				{
					if (isSpace)
					{
						offset = i;
					}

					isSpace = false;
				}
			}

			if (!isSpace)
			{
				int wordLen = len - offset;
				pOffsetArr[wordCount] = offset;
				pLenArr[wordCount] = wordLen;
				if (wordLen > maxWordLen)
				{
					maxWordLen = wordLen;
				}

				wordCount++;
			}
		}

		[TargetedPatchingOptOut("speed")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe bool Levenshtein_Distance(char* sourceWord, int sourceWordLength, char* Sample, int sampleLen, out int ldist, int* dist_im1)
		{
			ldist = 0;
			int max_transformations;
			max_transformations = m_auto_max_transformations ? ((1 + (sourceWordLength) / 2) * (m_ADDITION + m_CHANGE + m_DELETION) / 3) : m_max_transformations;

			if (Math.Abs(sourceWordLength - sampleLen) <= max_transformations)
				if ((ldist = l_dist_raw(sourceWord, sourceWordLength, Sample, sampleLen, dist_im1)) <= max_transformations)
					return (true);

			return (false);
		}

		[TargetedPatchingOptOut("speed")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe int l_dist_raw(char* sourceWord, int sourceWordLength, char* Sample, int sampleLen, int* dist_im1)
		{
			//Console.WriteLine(new string(sourceWord, 0, sourceWordLength) + "-" + new string(Sample, 0, sampleLen));
			int i, j;

			int dist_i_jm1, dist_j0;
			int MinDist = 0;

			int accumulateAdditions = 0;
			for (i = 0; i <= sampleLen; i++)
			{
				dist_im1[i] = accumulateAdditions;
				accumulateAdditions += m_ADDITION;
			}

			dist_j0 = 0;

			for (i = 0; i < sourceWordLength; i++)
			{
				dist_i_jm1 = dist_j0 += m_DELETION;
				for (j = 0; j < sampleLen; j++)
				{
					int x = dist_im1[j] + (sourceWord[i] == Sample[j] ? 0 : m_CHANGE);
					int y = dist_i_jm1 + m_ADDITION;
					int z = dist_im1[j + 1] + m_DELETION;
					MinDist = x < y ? Math.Min(x, z) : Math.Min(y, z);
					dist_im1[j] = dist_i_jm1;
					dist_i_jm1 = MinDist;
				}
				dist_im1[j] = MinDist;
			}
			return MinDist;
		}
	}
}
