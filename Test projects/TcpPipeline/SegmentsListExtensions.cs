using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Maxima.Tcp
{
    internal static class SegmentsListExtensions
    {
        public static IList<ArraySegment<byte>> Slice([NotNull] this IList<ArraySegment<byte>> source, int requiredOffset)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            int currentOffset = 0;
            List<ArraySegment<byte>> newBufferList = new List<ArraySegment<byte>>();
            for (int i = 0; i < source.Count; i++)
            {
                ArraySegment<byte> segment = source[i];
                int nextOffset = currentOffset + segment.Count;
                // если сегмент прочитан полностью пропускаем
                if (nextOffset <= requiredOffset)
                {
                    currentOffset = nextOffset;
                    continue;
                }
                // если только часть сегмента прочитана, то создаем новый сегмент для осташейся части
                if (currentOffset <= requiredOffset)
                {
                    newBufferList.Add(new ArraySegment<byte>(
                        segment.Array,
                        segment.Offset + (requiredOffset - currentOffset),
                        segment.Count - (requiredOffset - currentOffset)));
                }
                else
                {
                    newBufferList.Add(segment);
                }
                currentOffset = nextOffset;
            }
            return newBufferList;
        }

        public static IList<ArraySegment<byte>> Trim([NotNull] this IList<ArraySegment<byte>> source, int bytesCount)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            int countedSize = 0;
            List<ArraySegment<byte>> newBufferList = new List<ArraySegment<byte>>();
            for (int i = 0; i < source.Count; i++)
            {
                ArraySegment<byte> segment = source[i];
                int nextSize = segment.Count + countedSize;
                if (nextSize <= bytesCount)
                {
                    newBufferList.Add(segment);
                    countedSize = nextSize;
                    continue;
                }
                // если только часть сегмента прочитана, то создаем новый сегмент для осташейся части
                if (countedSize < bytesCount)
                {
                    newBufferList.Add(new ArraySegment<byte>(
                        segment.Array,
                        segment.Offset,
                        bytesCount - countedSize));
                }
                break;
            }
            return newBufferList;
        }

        public static void FillFromArray([NotNull] this IList<ArraySegment<byte>> destination, [NotNull] byte[] source)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (destination.Sum(bytes => bytes.Count) < source.Length)
            {
                throw new ArgumentOutOfRangeException("source", source.Length,
                    "Source size is biger than destination size");
            }

            int sourceOffset = 0;
            for (int i = 0; i < destination.Count; i++)
            {
                ArraySegment<byte> segment = destination[i];
                Debug.Assert(sourceOffset < source.Length);
                int count = Math.Min(segment.Count, source.Length - sourceOffset);
                Buffer.BlockCopy(source, sourceOffset, segment.Array, segment.Offset, count);

                sourceOffset += count;

                if (sourceOffset == source.Length)
                {
                    break;
                }
            }
        }
    }
}