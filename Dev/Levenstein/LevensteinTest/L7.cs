using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace LevensteinTest
{
	unsafe class L7
	{

		private const int defaultMaxTransformations = 5;

		/// <summary>
		/// Автоматическое определение максимального числа преобразований.
		/// </summary>
		private bool m_auto_max_transformations = false;
		private int m_int_auto_max_transformations = 0;

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

		protected int transformConst;


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
		public L7(int v_WordSimilarityKoeff, int v_WordExceedingKoeff, int v_max_transformations, int v_CHANGE, int v_ADDITION, int v_DELETION, int v_cuttingLimit)
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
			m_int_auto_max_transformations = 0;
			if (v_cuttingLimit < 0 || v_cuttingLimit > 100)
			{
				throw new ArgumentException("8");
			}
			m_cuttingLimit = v_cuttingLimit;

			transformConst = (m_ADDITION + m_CHANGE + m_DELETION) / 3;

			pOffsetArr = (int*)GCHandle.Alloc(offsetList, GCHandleType.Pinned).AddrOfPinnedObject();
			pLenArr = (int*)GCHandle.Alloc(lenList, GCHandleType.Pinned).AddrOfPinnedObject();
			dist_im1 = (int*)GCHandle.Alloc(dist_im1_array, GCHandleType.Pinned).AddrOfPinnedObject();
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		public L7(int v_WordSimilatyKoeff, int v_WordExceedingKoeff, int v_CHANGE, int v_ADDITION, int v_DELETION, int v_cuttingLimit)
			: this(v_WordSimilatyKoeff, v_WordExceedingKoeff, defaultMaxTransformations, v_CHANGE, v_ADDITION, v_DELETION, v_cuttingLimit)
		{
		}

		/// <summary>
		/// Сравнить 2-е строки и вычислить коэффициент похожести
		/// </summary>
		/// <returns>Включать ли сравниваемую строку в результат</returns>
		public unsafe bool CalculateSimilarity(char** pSourceArr, int* pSourceLenArr, int sourceArrLen, byte[] sampleData, out double SimilarityKoeff)
		{
			int ldist;
			double KoeffEnd, KoeffTemp, temp1, temp2;

			fixed (byte* pbSample = sampleData)
			{
				int sampleWordCount, maxSampleWordLength, sampleStartIndex;
				sampleStartIndex = this.ParseString(pbSample, out sampleWordCount, out maxSampleWordLength);

				char* pSample = (char*)&pbSample[sampleStartIndex];
				KoeffEnd = 0;
				for (int i = 0; i < sourceArrLen; i++)
				{
					KoeffTemp = 0;

					int sourceWordLength = pSourceLenArr[i];
					char* sourceWord = pSourceArr[i];

					int sampleWordIndex = this.GetStartingIndex(sampleWordCount, sourceWordLength);
					int leftIndex, rightIndex;
					if (sampleWordIndex >= 0)
					{
						leftIndex = rightIndex = sampleWordIndex;
					}
					else
					{
						sampleWordIndex = ~sampleWordIndex;
						leftIndex = (sampleWordIndex == 0 ? 0 : sampleWordIndex - 1);
						rightIndex = (sampleWordIndex < sampleWordCount - 1 ? sampleWordIndex + 1 : sampleWordCount - 1);
					}


					int currentIndex;
					bool movedToLeft, movedToRight;
					while (true)
					{
						movedToLeft = false;
						movedToRight = false;

						if (leftIndex == rightIndex)
						{
							currentIndex = leftIndex;
							movedToLeft = true;
							movedToRight = true;
						}
						else if (leftIndex < 0)
						{
							if (rightIndex >= sampleWordCount)
							{
								break;
							}

							currentIndex = rightIndex;
							movedToRight = true;
						}
						else if (rightIndex >= sampleWordCount)
						{
							currentIndex = leftIndex;
							movedToLeft = true;
						}
						else
						{
							int rightDistance = this.pLenArr[rightIndex] - sourceWordLength;
							if (rightDistance == 0)
							{
								currentIndex = rightIndex;
								movedToRight = true;
							}
							else
							{
								int leftDistance = sourceWordLength - this.pLenArr[leftIndex];
								if (leftDistance < rightDistance)
								{
									currentIndex = leftIndex;
									movedToLeft = true;
								}
								else
								{
									currentIndex = rightIndex;
									movedToRight = true;
								}
							}
						}

						if (movedToLeft)
						{
							leftIndex--;
						}
						if (movedToRight)
						{
							rightIndex++;
						}

						temp1 = (sourceWordLength + pLenArr[currentIndex]) / 2 + 1;
						if (KoeffTemp > 0 && sourceWordLength != pLenArr[currentIndex])
						{
							double preTemp2 = (sourceWordLength < pLenArr[currentIndex] ?
								(temp1) / (m_ADDITION * (pLenArr[currentIndex] - sourceWordLength) + temp1) :
								(temp1) / (m_DELETION * (sourceWordLength - pLenArr[currentIndex]) + temp1));
							if (preTemp2 < KoeffTemp)
							{
								break;
							}
						}

						bool result = Levenshtein_Distance(sourceWord, sourceWordLength, pSample + pOffsetArr[currentIndex], pLenArr[currentIndex], out ldist);
						if (result)
						{
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

				//if (sourceArrLen != 0)
					KoeffEnd = m_WordSimilarityKoeff * 1.0 * KoeffEnd / sourceArrLen;
				//else
				//	KoeffEnd = 0;
				if (KoeffEnd > 0)
					KoeffEnd += m_WordExceedingKoeff * 1.0 / (abs(ref sourceArrLen, ref sampleWordCount) + 1);

				SimilarityKoeff = KoeffEnd;
			}

			return (SimilarityKoeff >= m_cuttingLimit);
		}

		[TargetedPatchingOptOut("speed")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetStartingIndex(int sampleWordCount, int sourceWordLength)
		{
			int startIndex = 0;
			int endIndex = sampleWordCount - 1;
			while (true)
			{
				int testIndex = (startIndex + endIndex) / 2;
				int testIndexWordLen = this.pLenArr[testIndex];

				if (testIndexWordLen == sourceWordLength)
				{
					return testIndex;
				}
				else if (startIndex == endIndex)
				{
					return ~startIndex;
				}
				else if (testIndexWordLen < sourceWordLength)
				{
					startIndex = testIndex + 1;
				}
				else
				{
					endIndex = testIndex - 1;
				}
			}
		}

		[TargetedPatchingOptOut("speed")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe int ParseString(byte* pSampleData, out int wordCount, out int maxWordLen)
		{
			int offset = 0;
			int* p = (int*)pSampleData;
			wordCount = *p;
			maxWordLen = 0;
			for (int i = 0; i < wordCount; i++)
			{
				int len = p[i + 1];
				if (len > maxWordLen)
				{
					maxWordLen = len;
				}

				pLenArr[i] = len;
				pOffsetArr[i] = offset;
				offset += len;
			}

			return (wordCount + 2) * sizeof(int);
		}

		[TargetedPatchingOptOut("speed")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe bool Levenshtein_Distance(char* sourceWord, int sourceWordLength, char* Sample, int sampleLen, out int ldist)
		{
			ldist = 0;
			int max_transformations;
			max_transformations = (m_auto_max_transformations ? ((1 + (sourceWordLength) / 2) * transformConst) : m_max_transformations);

			//if (Math.Abs(sourceWordLength - sampleLen) <= max_transformations)
			if (abs(ref sourceWordLength, ref sampleLen) <= max_transformations)
				if ((ldist = l_dist_raw(sourceWord, sourceWordLength, Sample, sampleLen)) <= max_transformations)
					return (true);

			return (false);
		}

		[TargetedPatchingOptOut("speed")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe int l_dist_raw(char* sourceWord, int sourceWordLength, char* Sample, int sampleLen)
		{
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
					MinDist = (x < y ? Math.Min(x, z) : Math.Min(y, z));
					dist_im1[j] = dist_i_jm1;
					dist_i_jm1 = MinDist;
				}
				dist_im1[j] = MinDist;
			}
			return MinDist;
		}

		[TargetedPatchingOptOut("speed")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int optional(bool flag, int value)
		{
			return -Convert.ToInt32(flag) & value;
		}

		[TargetedPatchingOptOut("speed")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int ifelse(bool flag, int v1, int v2)
		{
			return (-Convert.ToInt32(flag) & v1) | ((Convert.ToInt32(flag) - 1) & v2);
		}

		[TargetedPatchingOptOut("speed")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int ifelse(int flag, int v1, int v2)
		{
			return (-flag & v1) | ((flag - 1) & v2);
		}

		[TargetedPatchingOptOut("speed")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int abs(int v1, int v2)
		{
			return (v1 < v2 ? v2 - v1 : v1 - v2);
		}

		[TargetedPatchingOptOut("speed")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int abs(ref int v1, ref int v2)
		{
			return (v1 < v2 ? v2 - v1 : v1 - v2);
		}
	}
}
