using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TestLog
{
    public sealed class MyBuilder
    {
        internal const int DefaultCapacity = 16;
        internal const int MaxChunkSize = 8000;
        internal char[] m_ChunkChars;
        internal MyBuilder m_ChunkPrevious;
        internal int m_ChunkLength;
        internal int m_ChunkOffset;
        internal int m_MaxCapacity;
        private const string CapacityField = "Capacity";
        private const string MaxCapacityField = "m_MaxCapacity";
        private const string StringValueField = "m_StringValue";
        private const string ThreadIDField = "m_currentThread";

        public int Capacity
        {
            get
            {
                return this.m_ChunkChars.Length + this.m_ChunkOffset;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_NegativeCapacity");
                if (value > this.MaxCapacity)
                    throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_Capacity");
                if (value < this.Length)
                    throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_SmallCapacity");
                if (this.Capacity == value)
                    return;
                char[] chArray = new char[value - this.m_ChunkOffset];
                Array.Copy((Array)this.m_ChunkChars, (Array)chArray, this.m_ChunkLength);
                this.m_ChunkChars = chArray;
            }
        }

        public int MaxCapacity
        {
            get
            {
                return this.m_MaxCapacity;
            }
        }

        public int Length
        {
            get
            {
                return this.m_ChunkOffset + this.m_ChunkLength;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_NegativeLength");
                if (value > this.MaxCapacity)
                    throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_SmallCapacity");
                int capacity = this.Capacity;
                if (value == 0 && this.m_ChunkPrevious == null)
                {
                    this.m_ChunkLength = 0;
                    this.m_ChunkOffset = 0;
                }
                else
                {
                    int repeatCount = value - this.Length;
                    if (repeatCount > 0)
                    {
                        this.Append(char.MinValue, repeatCount);
                    }
                    else
                    {
                        MyBuilder chunkForIndex = this.FindChunkForIndex(value);
                        if (chunkForIndex != this)
                        {
                            char[] chArray = new char[capacity - chunkForIndex.m_ChunkOffset];
                            Array.Copy((Array)chunkForIndex.m_ChunkChars, (Array)chArray, chunkForIndex.m_ChunkLength);
                            this.m_ChunkChars = chArray;
                            this.m_ChunkPrevious = chunkForIndex.m_ChunkPrevious;
                            this.m_ChunkOffset = chunkForIndex.m_ChunkOffset;
                        }
                        this.m_ChunkLength = value - chunkForIndex.m_ChunkOffset;
                    }
                }
            }
        }

       public char this[int index]
        {
            get
            {
                MyBuilder stringBuilder = this;
                do
                {
                    int index1 = index - stringBuilder.m_ChunkOffset;
                    if (index1 >= 0)
                    {
                        if (index1 >= stringBuilder.m_ChunkLength)
                            throw new IndexOutOfRangeException();
                        else
                            return stringBuilder.m_ChunkChars[index1];
                    }
                    else
                        stringBuilder = stringBuilder.m_ChunkPrevious;
                }
                while (stringBuilder != null);
                throw new IndexOutOfRangeException();
            }
            set
            {
                MyBuilder stringBuilder = this;
                do
                {
                    int index1 = index - stringBuilder.m_ChunkOffset;
                    if (index1 >= 0)
                    {
                        if (index1 >= stringBuilder.m_ChunkLength)
                            throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_Index");
                        stringBuilder.m_ChunkChars[index1] = value;
                        return;
                    }
                    else
                        stringBuilder = stringBuilder.m_ChunkPrevious;
                }
                while (stringBuilder != null);
                throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_Index");
            }
        }

        public MyBuilder()
            : this(16)
        {
        }

        public MyBuilder(int capacity)
            : this(string.Empty, capacity)
        {
        }

        public MyBuilder(string value)
            : this(value, 16)
        {
        }

        public MyBuilder(string value, int capacity)
            : this(value, 0, value != null ? value.Length : 0, capacity)
        {
        }

        public unsafe MyBuilder(string value, int startIndex, int length, int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("capacity", "ArgumentOutOfRange_MustBePositive");
            else if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", "ArgumentOutOfRange_MustBeNonNegNum");
            }
            else
            {
                if (startIndex < 0)
                    throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_StartIndex");
                if (value == null)
                    value = string.Empty;
                if (startIndex > value.Length - length)
                    throw new ArgumentOutOfRangeException("length", "ArgumentOutOfRange_IndexLength");
                this.m_MaxCapacity = int.MaxValue;
                if (capacity == 0)
                    capacity = 16;
                if (capacity < length)
                    capacity = length;
                this.m_ChunkChars = new char[capacity];
                this.m_ChunkLength = length;
                fixed (char* chPtr = value)
                    MyBuilder.ThreadSafeCopy((char*)((IntPtr)chPtr + startIndex * 2), this.m_ChunkChars, 0, length);
            }
        }

        public MyBuilder(int capacity, int maxCapacity)
        {
            if (capacity > maxCapacity)
                throw new ArgumentOutOfRangeException("capacity", "ArgumentOutOfRange_Capacity");
            if (maxCapacity < 1)
                throw new ArgumentOutOfRangeException("maxCapacity", "ArgumentOutOfRange_SmallMaxCapacity");
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity", "ArgumentOutOfRange_MustBePositive");
            }
            else
            {
                if (capacity == 0)
                    capacity = Math.Min(16, maxCapacity);
                this.m_MaxCapacity = maxCapacity;
                this.m_ChunkChars = new char[capacity];
            }
        }

        private MyBuilder(MyBuilder from)
        {
            this.m_ChunkLength = from.m_ChunkLength;
            this.m_ChunkOffset = from.m_ChunkOffset;
            this.m_ChunkChars = from.m_ChunkChars;
            this.m_ChunkPrevious = from.m_ChunkPrevious;
            this.m_MaxCapacity = from.m_MaxCapacity;
        }

        private MyBuilder(int size, int maxCapacity, MyBuilder previousBlock)
        {
            this.m_ChunkChars = new char[size];
            this.m_MaxCapacity = maxCapacity;
            this.m_ChunkPrevious = previousBlock;
            if (previousBlock == null)
                return;
            this.m_ChunkOffset = previousBlock.m_ChunkOffset + previousBlock.m_ChunkLength;
        }

        public int EnsureCapacity(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("capacity", "ArgumentOutOfRange_NegativeCapacity");
            if (this.Capacity < capacity)
                this.Capacity = capacity;
            return this.Capacity;
        }

        public char[] GetArray()
        {
            return this.m_ChunkChars;
        }

        public override unsafe string ToString()
        {
            if (this.Length == 0)
                return string.Empty;
            string str = FastAllocateString(this.Length);
            MyBuilder stringBuilder = this;
            fixed (char* chPtr = str)
            {
                do
                {
                    if (stringBuilder.m_ChunkLength > 0)
                    {
                        char[] chArray = stringBuilder.m_ChunkChars;
                        int num = stringBuilder.m_ChunkOffset;
                        int charCount = stringBuilder.m_ChunkLength;
                        if ((long)(uint)(charCount + num) > (long)str.Length || (uint)charCount > (uint)chArray.Length)
                            throw new ArgumentOutOfRangeException("chunkLength", "ArgumentOutOfRange_Index");
                        fixed (char* smem = chArray)
                            wstrcpy(chPtr + num, smem, charCount);
                    }
                    stringBuilder = stringBuilder.m_ChunkPrevious;
                }
                while (stringBuilder != null);
            }
            return str;
        }

        public unsafe string ToString(int startIndex, int length)
        {
            int length1 = this.Length;
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_StartIndex");
            if (startIndex > length1)
                throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_StartIndexLargerThanLength");
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "ArgumentOutOfRange_NegativeLength");
            if (startIndex > length1 - length)
                throw new ArgumentOutOfRangeException("length", "ArgumentOutOfRange_IndexLength");
            MyBuilder stringBuilder = this;
            int num1 = startIndex + length;
            string str = FastAllocateString(length);
            int num2 = length;
            fixed (char* chPtr = str)
            {
                while (num2 > 0)
                {
                    int num3 = num1 - stringBuilder.m_ChunkOffset;
                    if (num3 >= 0)
                    {
                        if (num3 > stringBuilder.m_ChunkLength)
                            num3 = stringBuilder.m_ChunkLength;
                        int num4 = num2;
                        int charCount = num4;
                        int index = num3 - num4;
                        if (index < 0)
                        {
                            charCount += index;
                            index = 0;
                        }
                        num2 -= charCount;
                        if (charCount > 0)
                        {
                            char[] chArray = stringBuilder.m_ChunkChars;
                            if ((long)(uint)(charCount + num2) > (long)length || (uint)(charCount + index) > (uint)chArray.Length)
                                throw new ArgumentOutOfRangeException("chunkCount", "ArgumentOutOfRange_Index");
                            fixed (char* smem = &chArray[index])
                                wstrcpy(chPtr + num2, smem, charCount);
                        }
                    }
                    stringBuilder = stringBuilder.m_ChunkPrevious;
                }
            }
            return str;
        }

        public MyBuilder Clear()
        {
            this.Length = 0;
            return this;
        }

        public MyBuilder Append(char value, int repeatCount)
        {
            if (repeatCount < 0)
                throw new ArgumentOutOfRangeException("repeatCount", "ArgumentOutOfRange_NegativeCount");
            if (repeatCount == 0)
                return this;
            int num = this.m_ChunkLength;
            while (repeatCount > 0)
            {
                if (num < this.m_ChunkChars.Length)
                {
                    this.m_ChunkChars[num++] = value;
                    --repeatCount;
                }
                else
                {
                    this.m_ChunkLength = num;
                    this.ExpandByABlock(repeatCount);
                    num = 0;
                }
            }
            this.m_ChunkLength = num;
            return this;
        }

        public unsafe MyBuilder Append(char[] value, int startIndex, int charCount)
        {
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_GenericPositive");
            if (charCount < 0)
                throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_GenericPositive");
            if (value == null)
            {
                if (startIndex == 0 && charCount == 0)
                    return this;
                else
                    throw new ArgumentNullException("value");
            }
            else
            {
                if (charCount > value.Length - startIndex)
                    throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_Index");
                if (charCount == 0)
                    return this;
                fixed (char* chPtr = &value[startIndex])
                    this.Append(chPtr, charCount);
                return this;
            }
        }

        public unsafe MyBuilder Append(string value)
        {
            if (value != null)
            {
                char[] chArray = this.m_ChunkChars;
                int index = this.m_ChunkLength;
                int length = value.Length;
                int num = index + length;
                if (num < chArray.Length)
                {
                    if (length <= 2)
                    {
                        if (length > 0)
                            chArray[index] = value[0];
                        if (length > 1)
                            chArray[index + 1] = value[1];
                    }
                    else
                    {
                        fixed (char* smem = value)
                        fixed (char* dmem = &chArray[index])
                            wstrcpy(dmem, smem, length);
                    }
                    this.m_ChunkLength = num;
                }
                else
                    this.AppendHelper(value);
            }
            return this;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal unsafe extern void ReplaceBufferInternal(char* newBuffer, int newLength);

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal unsafe extern void ReplaceBufferAnsiInternal(sbyte* newBuffer, int newLength);
        
        public unsafe MyBuilder Append(string value, int startIndex, int count)
        {
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_Index");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_GenericPositive");
            if (value == null)
            {
                if (startIndex == 0 && count == 0)
                    return this;
                else
                    throw new ArgumentNullException("value");
            }
            else
            {
                if (count == 0)
                    return this;
                if (startIndex > value.Length - count)
                    throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_Index");
                fixed (char* chPtr = value)
                    this.Append((char*)((IntPtr)chPtr + startIndex * 2), count);
                return this;
            }
        }

        public MyBuilder AppendLine()
        {
            return this.Append(Environment.NewLine);
        }

        public MyBuilder AppendLine(string value)
        {
            this.Append(value);
            return this.Append(Environment.NewLine);
        }

        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Arg_NegativeArgCount");
            if (destinationIndex < 0)
            {
                throw new ArgumentOutOfRangeException("destinationIndex", "ArgumentOutOfRange_MustBeNonNegNum");
            }
            else
            {
                if (destinationIndex > destination.Length - count)
                    throw new ArgumentException("ArgumentOutOfRange_OffsetOut");
                if ((uint)sourceIndex > (uint)this.Length)
                    throw new ArgumentOutOfRangeException("sourceIndex", "ArgumentOutOfRange_Index");
                if (sourceIndex > this.Length - count)
                    throw new ArgumentException("Arg_LongerThanSrcString");
                MyBuilder stringBuilder = this;
                int num1 = sourceIndex + count;
                int destinationIndex1 = destinationIndex + count;
                while (count > 0)
                {
                    int num2 = num1 - stringBuilder.m_ChunkOffset;
                    if (num2 >= 0)
                    {
                        if (num2 > stringBuilder.m_ChunkLength)
                            num2 = stringBuilder.m_ChunkLength;
                        int count1 = count;
                        int sourceIndex1 = num2 - count;
                        if (sourceIndex1 < 0)
                        {
                            count1 += sourceIndex1;
                            sourceIndex1 = 0;
                        }
                        destinationIndex1 -= count1;
                        count -= count1;
                        MyBuilder.ThreadSafeCopy(stringBuilder.m_ChunkChars, sourceIndex1, destination, destinationIndex1, count1);
                    }
                    stringBuilder = stringBuilder.m_ChunkPrevious;
                }
            }
        }

        public unsafe MyBuilder Insert(int index, string value, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
            int length = this.Length;
            if ((uint)index > (uint)length)
                throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_Index");
            if (value == null || value.Length == 0 || count == 0)
                return this;
            long num = (long)value.Length * (long)count;
            if (num > (long)(this.MaxCapacity - this.Length))
                throw new OutOfMemoryException();
            MyBuilder chunk;
            int indexInChunk;
            this.MakeRoom(index, (int)num, out chunk, out indexInChunk, false);
            fixed (char* chPtr = value)
            {
                for (; count > 0; --count)
                    this.ReplaceInPlaceAtChunk(ref chunk, ref indexInChunk, chPtr, value.Length);
            }
            return this;
        }

        public MyBuilder Remove(int startIndex, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "ArgumentOutOfRange_NegativeLength");
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_StartIndex");
            if (length > this.Length - startIndex)
                throw new ArgumentOutOfRangeException("index","ArgumentOutOfRange_Index");
            if (this.Length == length && startIndex == 0)
            {
                this.Length = 0;
                return this;
            }
            else
            {
                if (length > 0)
                {
                    MyBuilder chunk;
                    int indexInChunk;
                    this.Remove(startIndex, length, out chunk, out indexInChunk);
                }
                return this;
            }
        }

        public MyBuilder Append(bool value)
        {
            return this.Append(value.ToString());
        }

        public MyBuilder Append(sbyte value)
        {
            return this.Append(value.ToString((IFormatProvider)CultureInfo.CurrentCulture));
        }

        public MyBuilder Append(byte value)
        {
            return this.Append(value.ToString((IFormatProvider)CultureInfo.CurrentCulture));
        }

        public MyBuilder Append(char value)
        {
            if (this.m_ChunkLength < this.m_ChunkChars.Length)
                this.m_ChunkChars[this.m_ChunkLength++] = value;
            else
                this.Append(value, 1);
            return this;
        }

        public MyBuilder Append(short value)
        {
            return this.Append(value.ToString((IFormatProvider)CultureInfo.CurrentCulture));
        }

        public MyBuilder Append(int value)
        {
            return this.Append(value.ToString((IFormatProvider)CultureInfo.CurrentCulture));
        }

        public MyBuilder Append(long value)
        {
            return this.Append(value.ToString((IFormatProvider)CultureInfo.CurrentCulture));
        }

        public MyBuilder Append(float value)
        {
            return this.Append(value.ToString((IFormatProvider)CultureInfo.CurrentCulture));
        }

        public MyBuilder Append(double value)
        {
            return this.Append(value.ToString((IFormatProvider)CultureInfo.CurrentCulture));
        }

        public MyBuilder Append(Decimal value)
        {
            return this.Append(value.ToString((IFormatProvider)CultureInfo.CurrentCulture));
        }

        public MyBuilder Append(ushort value)
        {
            return this.Append(value.ToString((IFormatProvider)CultureInfo.CurrentCulture));
        }

        public MyBuilder Append(uint value)
        {
            return this.Append(value.ToString((IFormatProvider)CultureInfo.CurrentCulture));
        }

        public MyBuilder Append(ulong value)
        {
            return this.Append(value.ToString((IFormatProvider)CultureInfo.CurrentCulture));
        }

        public MyBuilder Append(object value)
        {
            if (value == null)
                return this;
            else
                return this.Append(value.ToString());
        }

        public unsafe MyBuilder Append(char[] value)
        {
            if (value != null && value.Length > 0)
            {
                fixed (char* chPtr = &value[0])
                    this.Append(chPtr, value.Length);
            }
            return this;
        }

        public unsafe MyBuilder Insert(int index, string value)
        {
            if ((uint)index > (uint)this.Length)
                throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_Index");
            if (value != null)
            {
                fixed (char* chPtr = value)
                    this.Insert(index, chPtr, value.Length);
            }
            return this;
        }

        public MyBuilder Insert(int index, bool value)
        {
            return this.Insert(index, value.ToString(), 1);
        }

        public MyBuilder Insert(int index, sbyte value)
        {
            return this.Insert(index, value.ToString((IFormatProvider)CultureInfo.CurrentCulture), 1);
        }

        public MyBuilder Insert(int index, byte value)
        {
            return this.Insert(index, value.ToString((IFormatProvider)CultureInfo.CurrentCulture), 1);
        }

        public MyBuilder Insert(int index, short value)
        {
            return this.Insert(index, value.ToString((IFormatProvider)CultureInfo.CurrentCulture), 1);
        }

        public unsafe MyBuilder Insert(int index, char value)
        {
            this.Insert(index, &value, 1);
            return this;
        }

        public MyBuilder Insert(int index, char[] value)
        {
            if ((uint)index > (uint)this.Length)
                throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_Index");
            if (value != null)
                this.Insert(index, value, 0, value.Length);
            return this;
        }

        public unsafe MyBuilder Insert(int index, char[] value, int startIndex, int charCount)
        {
            int length = this.Length;
            if ((uint)index > (uint)length)
                throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_Index");
            if (value == null)
            {
                if (startIndex == 0 && charCount == 0)
                    return this;
                else
                    throw new ArgumentNullException("ArgumentNull_String");
            }
            else
            {
                if (startIndex < 0)
                    throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_StartIndex");
                if (charCount < 0)
                    throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_GenericPositive");
                if (startIndex > value.Length - charCount)
                    throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_Index");
                if (charCount > 0)
                {
                    fixed (char* chPtr = &value[startIndex])
                        this.Insert(index, chPtr, charCount);
                }
                return this;
            }
        }

        public MyBuilder Insert(int index, int value)
        {
            return this.Insert(index, value.ToString((IFormatProvider)CultureInfo.CurrentCulture), 1);
        }

        public MyBuilder Insert(int index, long value)
        {
            return this.Insert(index, value.ToString((IFormatProvider)CultureInfo.CurrentCulture), 1);
        }

        public MyBuilder Insert(int index, float value)
        {
            return this.Insert(index, value.ToString((IFormatProvider)CultureInfo.CurrentCulture), 1);
        }

       public MyBuilder Insert(int index, double value)
        {
            return this.Insert(index, value.ToString((IFormatProvider)CultureInfo.CurrentCulture), 1);
        }

        public MyBuilder Insert(int index, Decimal value)
        {
            return this.Insert(index, value.ToString((IFormatProvider)CultureInfo.CurrentCulture), 1);
        }

        public MyBuilder Insert(int index, ushort value)
        {
            return this.Insert(index, value.ToString((IFormatProvider)CultureInfo.CurrentCulture), 1);
        }

        public MyBuilder Insert(int index, uint value)
        {
            return this.Insert(index, value.ToString((IFormatProvider)CultureInfo.CurrentCulture), 1);
        }

        public MyBuilder Insert(int index, ulong value)
        {
            return this.Insert(index, value.ToString((IFormatProvider)CultureInfo.CurrentCulture), 1);
        }

        public MyBuilder Insert(int index, object value)
        {
            if (value == null)
                return this;
            else
                return this.Insert(index, value.ToString(), 1);
        }

        public MyBuilder AppendFormat(string format, object arg0)
        {
            return this.AppendFormat((IFormatProvider)null, format, new object[1]
    {
    arg0
    });
        }

        public MyBuilder AppendFormat(string format, object arg0, object arg1)
        {
            return this.AppendFormat((IFormatProvider)null, format, new object[2]
    {
    arg0,
    arg1
    });
        }

        public MyBuilder AppendFormat(string format, object arg0, object arg1, object arg2)
        {
            return this.AppendFormat((IFormatProvider)null, format, arg0, arg1, arg2);
        }

        public MyBuilder AppendFormat(string format, params object[] args)
        {
            return this.AppendFormat((IFormatProvider)null, format, args);
        }

        public MyBuilder AppendFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (format == null || args == null)
                throw new ArgumentNullException(format == null ? "format" : "args");
            int index1 = 0;
            int length = format.Length;
            char ch = char.MinValue;
            ICustomFormatter customFormatter = (ICustomFormatter)null;
            if (provider != null)
                customFormatter = (ICustomFormatter)provider.GetFormat(typeof(ICustomFormatter));
            while (true)
            {
                bool flag;
                int repeatCount;
                do
                {
                    if (index1 < length)
                    {
                        ch = format[index1];
                        ++index1;
                        if ((int)ch == 125)
                        {
                            if (index1 < length && (int)format[index1] == 125)
                                ++index1;
                            else
                                MyBuilder.FormatError();
                        }
                        if ((int)ch == 123)
                        {
                            if (index1 >= length || (int)format[index1] != 123)
                                --index1;
                            else
                                goto label_10;
                        }
                        else
                            goto label_12;
                    }
                    if (index1 != length)
                    {
                        int index2 = index1 + 1;
                        if (index2 == length || (int)(ch = format[index2]) < 48 || (int)ch > 57)
                            MyBuilder.FormatError();
                        int index3 = 0;
                        do
                        {
                            index3 = index3 * 10 + (int)ch - 48;
                            ++index2;
                            if (index2 == length)
                                MyBuilder.FormatError();
                            ch = format[index2];
                        }
                        while ((int)ch >= 48 && (int)ch <= 57 && index3 < 1000000);
                        if (index3 >= args.Length)
                            throw new FormatException("Format_IndexOutOfRange");
                        while (index2 < length && (int)(ch = format[index2]) == 32)
                            ++index2;
                        flag = false;
                        int num = 0;
                        if ((int)ch == 44)
                        {
                            ++index2;
                            while (index2 < length && (int)format[index2] == 32)
                                ++index2;
                            if (index2 == length)
                                MyBuilder.FormatError();
                            ch = format[index2];
                            if ((int)ch == 45)
                            {
                                flag = true;
                                ++index2;
                                if (index2 == length)
                                    MyBuilder.FormatError();
                                ch = format[index2];
                            }
                            if ((int)ch < 48 || (int)ch > 57)
                                MyBuilder.FormatError();
                            do
                            {
                                num = num * 10 + (int)ch - 48;
                                ++index2;
                                if (index2 == length)
                                    MyBuilder.FormatError();
                                ch = format[index2];
                            }
                            while ((int)ch >= 48 && (int)ch <= 57 && num < 1000000);
                        }
                        while (index2 < length && (int)(ch = format[index2]) == 32)
                            ++index2;
                        object obj = args[index3];
                        MyBuilder stringBuilder = (MyBuilder)null;
                        if ((int)ch == 58)
                        {
                            int index4 = index2 + 1;
                            while (true)
                            {
                                if (index4 == length)
                                    MyBuilder.FormatError();
                                ch = format[index4];
                                ++index4;
                                if ((int)ch == 123)
                                {
                                    if (index4 < length && (int)format[index4] == 123)
                                        ++index4;
                                    else
                                        MyBuilder.FormatError();
                                }
                                else if ((int)ch == 125)
                                {
                                    if (index4 < length && (int)format[index4] == 125)
                                        ++index4;
                                    else
                                        break;
                                }
                                if (stringBuilder == null)
                                    stringBuilder = new MyBuilder();
                                stringBuilder.Append(ch);
                            }
                            index2 = index4 - 1;
                        }
                        if ((int)ch != 125)
                            MyBuilder.FormatError();
                        index1 = index2 + 1;
                        string format1 = (string)null;
                        string str = (string)null;
                        if (customFormatter != null)
                        {
                            if (stringBuilder != null)
                                format1 = ((object)stringBuilder).ToString();
                            str = customFormatter.Format(format1, obj, provider);
                        }
                        if (str == null)
                        {
                            IFormattable formattable = obj as IFormattable;
                            if (formattable != null)
                            {
                                if (format1 == null && stringBuilder != null)
                                    format1 = ((object)stringBuilder).ToString();
                                str = formattable.ToString(format1, provider);
                            }
                            else if (obj != null)
                                str = obj.ToString();
                        }
                        if (str == null)
                            str = string.Empty;
                        repeatCount = num - str.Length;
                        if (!flag && repeatCount > 0)
                            this.Append(' ', repeatCount);
                        this.Append(str);
                    }
                    else
                        goto label_76;
                }
                while (!flag || repeatCount <= 0);
                goto label_75;
            label_10:
                ++index1;
            label_12:
                this.Append(ch);
                continue;
            label_75:
                this.Append(' ', repeatCount);
            }
        label_76:
            return this;
        }

        public MyBuilder Replace(string oldValue, string newValue)
        {
            return this.Replace(oldValue, newValue, 0, this.Length);
        }

        public bool Equals(MyBuilder sb)
        {
            if (sb == null || this.Capacity != sb.Capacity || (this.MaxCapacity != sb.MaxCapacity || this.Length != sb.Length))
                return false;
            if (sb == this)
                return true;
            MyBuilder stringBuilder1 = this;
            int index1 = stringBuilder1.m_ChunkLength;
            MyBuilder stringBuilder2 = sb;
            int index2 = stringBuilder2.m_ChunkLength;
            do
            {
                --index1;
                --index2;
                for (; index1 < 0; index1 = stringBuilder1.m_ChunkLength + index1)
                {
                    stringBuilder1 = stringBuilder1.m_ChunkPrevious;
                    if (stringBuilder1 == null)
                        break;
                }
                for (; index2 < 0; index2 = stringBuilder2.m_ChunkLength + index2)
                {
                    stringBuilder2 = stringBuilder2.m_ChunkPrevious;
                    if (stringBuilder2 == null)
                        break;
                }
                if (index1 < 0)
                    return index2 < 0;
            }
            while (index2 >= 0 && (int)stringBuilder1.m_ChunkChars[index1] == (int)stringBuilder2.m_ChunkChars[index2]);
            return false;
        }

        public MyBuilder Replace(string oldValue, string newValue, int startIndex, int count)
        {
            int length1 = this.Length;
            if ((uint)startIndex > (uint)length1)
                throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_Index");
            if (count < 0 || startIndex > length1 - count)
                throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_Index");
            if (oldValue == null)
                throw new ArgumentNullException("oldValue");
            if (oldValue.Length == 0)
                throw new ArgumentException("Argument_EmptyName", "oldValue");
            if (newValue == null)
                newValue = "";
            int length2 = newValue.Length;
            int length3 = oldValue.Length;
            int[] replacements = (int[])null;
            int replacementsCount = 0;
            MyBuilder chunkForIndex = this.FindChunkForIndex(startIndex);
            int indexInChunk = startIndex - chunkForIndex.m_ChunkOffset;
            while (count > 0)
            {
                if (this.StartsWith(chunkForIndex, indexInChunk, count, oldValue))
                {
                    if (replacements == null)
                        replacements = new int[5];
                    else if (replacementsCount >= replacements.Length)
                    {
                        int[] numArray = new int[replacements.Length * 3 / 2 + 4];
                        Array.Copy((Array)replacements, (Array)numArray, replacements.Length);
                        replacements = numArray;
                    }
                    replacements[replacementsCount++] = indexInChunk;
                    indexInChunk += oldValue.Length;
                    count -= oldValue.Length;
                }
                else
                {
                    ++indexInChunk;
                    --count;
                }
                if (indexInChunk >= chunkForIndex.m_ChunkLength || count == 0)
                {
                    int num = indexInChunk + chunkForIndex.m_ChunkOffset;
                    this.ReplaceAllInChunk(replacements, replacementsCount, chunkForIndex, oldValue.Length, newValue);
                    int index = num + (newValue.Length - oldValue.Length) * replacementsCount;
                    replacementsCount = 0;
                    chunkForIndex = this.FindChunkForIndex(index);
                    indexInChunk = index - chunkForIndex.m_ChunkOffset;
                }
            }
            return this;
        }

        public MyBuilder Replace(char oldChar, char newChar)
        {
            return this.Replace(oldChar, newChar, 0, this.Length);
        }

        public MyBuilder Replace(char oldChar, char newChar, int startIndex, int count)
        {
            int length = this.Length;
            if ((uint)startIndex > (uint)length)
                throw new ArgumentOutOfRangeException("startIndex","ArgumentOutOfRange_Index");
            if (count < 0 || startIndex > length - count)
                throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_Index");
            int num = startIndex + count;
            MyBuilder stringBuilder = this;
            while (true)
            {
                int val2 = num - stringBuilder.m_ChunkOffset;
                int val1 = startIndex - stringBuilder.m_ChunkOffset;
                if (val2 >= 0)
                {
                    int index1 = Math.Max(val1, 0);
                    for (int index2 = Math.Min(stringBuilder.m_ChunkLength, val2); index1 < index2; ++index1)
                    {
                        if ((int)stringBuilder.m_ChunkChars[index1] == (int)oldChar)
                            stringBuilder.m_ChunkChars[index1] = newChar;
                    }
                }
                if (val1 < 0)
                    stringBuilder = stringBuilder.m_ChunkPrevious;
                else
                    break;
            }
            return this;
        }

        internal unsafe MyBuilder Append(char* value, int valueCount)
        {
            int num1 = valueCount + this.m_ChunkLength;
            if (num1 <= this.m_ChunkChars.Length)
            {
                MyBuilder.ThreadSafeCopy(value, this.m_ChunkChars, this.m_ChunkLength, valueCount);
                this.m_ChunkLength = num1;
            }
            else
            {
                int count = this.m_ChunkChars.Length - this.m_ChunkLength;
                if (count > 0)
                {
                    MyBuilder.ThreadSafeCopy(value, this.m_ChunkChars, this.m_ChunkLength, count);
                    this.m_ChunkLength = this.m_ChunkChars.Length;
                }
                int num2 = valueCount - count;
                this.ExpandByABlock(num2);
                MyBuilder.ThreadSafeCopy(value + count, this.m_ChunkChars, 0, num2);
                this.m_ChunkLength = num2;
            }
            return this;
        }

        internal unsafe void InternalCopy(IntPtr dest, int len)
        {
            if (len == 0)
                return;
            bool flag = true;
            byte* numPtr = (byte*)dest.ToPointer();
            MyBuilder stringBuilder = this.FindChunkForByte(len);
            do
            {
                int num = stringBuilder.m_ChunkOffset * 2;
                int len1 = stringBuilder.m_ChunkLength * 2;
                fixed (char* chPtr = &stringBuilder.m_ChunkChars[0])
                {
                    if (flag)
                    {
                        flag = false;
                        Memcpy(numPtr + num, (byte*)chPtr, len - num);
                    }
                    else
                        Memcpy(numPtr + num, (byte*)chPtr, len1);
                }
                stringBuilder = stringBuilder.m_ChunkPrevious;
            }
            while (stringBuilder != null);
        }

        [Conditional("_DEBUG")]
        private void VerifyClassInvariant()
        {
            MyBuilder stringBuilder1 = this;
            while (true)
            {
                MyBuilder stringBuilder2 = stringBuilder1.m_ChunkPrevious;
                if (stringBuilder2 != null)
                    stringBuilder1 = stringBuilder2;
                else
                    break;
            }
        }

        private unsafe void AppendHelper(string value)
        {
            fixed (char* chPtr = value)
                this.Append(chPtr, value.Length);
        }

        private static void FormatError()
        {
            throw new FormatException("Format_InvalidString");
        }

        private unsafe void Insert(int index, char* value, int valueCount)
        {
            if ((uint)index > (uint)this.Length)
                throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_Index");
            if (valueCount <= 0)
                return;
            MyBuilder chunk;
            int indexInChunk;
            this.MakeRoom(index, valueCount, out chunk, out indexInChunk, false);
            this.ReplaceInPlaceAtChunk(ref chunk, ref indexInChunk, value, valueCount);
        }

        private unsafe void ReplaceAllInChunk(int[] replacements, int replacementsCount, MyBuilder sourceChunk, int removeCount, string value)
        {
            if (replacementsCount <= 0)
                return;
            fixed (char* chPtr1 = value)
            {
                int count = (value.Length - removeCount) * replacementsCount;
                MyBuilder chunk = sourceChunk;
                int indexInChunk = replacements[0];
                if (count > 0)
                    this.MakeRoom(chunk.m_ChunkOffset + indexInChunk, count, out chunk, out indexInChunk, true);
                int index1 = 0;
                while (true)
                {
                    this.ReplaceInPlaceAtChunk(ref chunk, ref indexInChunk, chPtr1, value.Length);
                    int index2 = replacements[index1] + removeCount;
                    ++index1;
                    if (index1 < replacementsCount)
                    {
                        int num = replacements[index1];
                        if (count != 0)
                        {
                            fixed (char* chPtr2 = &sourceChunk.m_ChunkChars[index2])
                                this.ReplaceInPlaceAtChunk(ref chunk, ref indexInChunk, chPtr2, num - index2);
                        }
                        else
                            indexInChunk += num - index2;
                    }
                    else
                        break;
                }
                if (count < 0)
                    this.Remove(chunk.m_ChunkOffset + indexInChunk, -count, out chunk, out indexInChunk);
            }
        }

        private bool StartsWith(MyBuilder chunk, int indexInChunk, int count, string value)
        {
            for (int index = 0; index < value.Length; ++index)
            {
                if (count == 0)
                    return false;
                if (indexInChunk >= chunk.m_ChunkLength)
                {
                    chunk = this.Next(chunk);
                    if (chunk == null)
                        return false;
                    indexInChunk = 0;
                }
                if ((int)value[index] != (int)chunk.m_ChunkChars[indexInChunk])
                    return false;
                ++indexInChunk;
                --count;
            }
            return true;
        }

        private unsafe void ReplaceInPlaceAtChunk(ref MyBuilder chunk, ref int indexInChunk, char* value, int count)
        {
            if (count == 0)
                return;
            while (true)
            {
                int count1 = Math.Min(chunk.m_ChunkLength - indexInChunk, count);
                MyBuilder.ThreadSafeCopy(value, chunk.m_ChunkChars, indexInChunk, count1);
                indexInChunk += count1;
                if (indexInChunk >= chunk.m_ChunkLength)
                {
                    chunk = this.Next(chunk);
                    indexInChunk = 0;
                }
                count -= count1;
                if (count != 0)
                    value += count1;
                else
                    break;
            }
        }

        private static unsafe void ThreadSafeCopy(char* sourcePtr, char[] destination, int destinationIndex, int count)
        {
            if (count <= 0)
                return;
            if ((uint)destinationIndex > (uint)destination.Length || destinationIndex + count > destination.Length)
                throw new ArgumentOutOfRangeException("destinationIndex", "ArgumentOutOfRange_Index");
            fixed (char* dmem = &destination[destinationIndex])
                wstrcpy(dmem, sourcePtr, count);
        }

        private static unsafe void ThreadSafeCopy(char[] source, int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            if (count <= 0)
                return;
            if ((uint)sourceIndex > (uint)source.Length || sourceIndex + count > source.Length)
                throw new ArgumentOutOfRangeException("sourceIndex", "ArgumentOutOfRange_Index");
            fixed (char* sourcePtr = &source[sourceIndex])
                MyBuilder.ThreadSafeCopy(sourcePtr, destination, destinationIndex, count);
        }

        private MyBuilder FindChunkForIndex(int index)
        {
            MyBuilder stringBuilder = this;
            while (stringBuilder.m_ChunkOffset > index)
                stringBuilder = stringBuilder.m_ChunkPrevious;
            return stringBuilder;
        }

        private MyBuilder FindChunkForByte(int byteIndex)
        {
            MyBuilder stringBuilder = this;
            while (stringBuilder.m_ChunkOffset * 2 > byteIndex)
                stringBuilder = stringBuilder.m_ChunkPrevious;
            return stringBuilder;
        }

        private MyBuilder Next(MyBuilder chunk)
        {
            if (chunk == this)
                return (MyBuilder)null;
            else
                return this.FindChunkForIndex(chunk.m_ChunkOffset + chunk.m_ChunkLength);
        }

        private void ExpandByABlock(int minBlockCharCount)
        {
            if (minBlockCharCount + this.Length > this.m_MaxCapacity)
                throw new ArgumentOutOfRangeException("requiredLength", "ArgumentOutOfRange_SmallCapacity");
            int length = Math.Max(minBlockCharCount, Math.Min(this.Length, 8000));
            this.m_ChunkPrevious = new MyBuilder(this);
            this.m_ChunkOffset += this.m_ChunkLength;
            this.m_ChunkLength = 0;
            if (this.m_ChunkOffset + length < length)
            {
                this.m_ChunkChars = (char[])null;
                throw new OutOfMemoryException();
            }
            else
                this.m_ChunkChars = new char[length];
        }

        private unsafe void MakeRoom(int index, int count, out MyBuilder chunk, out int indexInChunk, bool doneMoveFollowingChars)
        {
            if (count + this.Length > this.m_MaxCapacity)
                throw new ArgumentOutOfRangeException("requiredLength", "ArgumentOutOfRange_SmallCapacity");
            chunk = this;
            while (chunk.m_ChunkOffset > index)
            {
                chunk.m_ChunkOffset += count;
                chunk = chunk.m_ChunkPrevious;
            }
            indexInChunk = index - chunk.m_ChunkOffset;
            if (!doneMoveFollowingChars && chunk.m_ChunkLength <= 32 && chunk.m_ChunkChars.Length - chunk.m_ChunkLength >= count)
            {
                int index1 = chunk.m_ChunkLength;
                while (index1 > indexInChunk)
                {
                    --index1;
                    chunk.m_ChunkChars[index1 + count] = chunk.m_ChunkChars[index1];
                }
                chunk.m_ChunkLength += count;
            }
            else
            {
                MyBuilder stringBuilder = new MyBuilder(Math.Max(count, 16), chunk.m_MaxCapacity, chunk.m_ChunkPrevious);
                stringBuilder.m_ChunkLength = count;
                int count1 = Math.Min(count, indexInChunk);
                if (count1 > 0)
                {
                    fixed (char* sourcePtr = chunk.m_ChunkChars)
                    {
                        MyBuilder.ThreadSafeCopy(sourcePtr, stringBuilder.m_ChunkChars, 0, count1);
                        int count2 = indexInChunk - count1;
                        if (count2 >= 0)
                        {
                            MyBuilder.ThreadSafeCopy(sourcePtr + count1, chunk.m_ChunkChars, 0, count2);
                            indexInChunk = count2;
                        }
                    }
                }
                chunk.m_ChunkPrevious = stringBuilder;
                chunk.m_ChunkOffset += count;
                if (count1 >= count)
                    return;
                chunk = stringBuilder;
                indexInChunk = count1;
            }
        }

        private void Remove(int startIndex, int count, out MyBuilder chunk, out int indexInChunk)
        {
            int num = startIndex + count;
            chunk = this;
            MyBuilder stringBuilder = (MyBuilder)null;
            int sourceIndex = 0;
            while (true)
            {
                if (num - chunk.m_ChunkOffset >= 0)
                {
                    if (stringBuilder == null)
                    {
                        stringBuilder = chunk;
                        sourceIndex = num - stringBuilder.m_ChunkOffset;
                    }
                    if (startIndex - chunk.m_ChunkOffset >= 0)
                        break;
                }
                else
                    chunk.m_ChunkOffset -= count;
                chunk = chunk.m_ChunkPrevious;
            }
            indexInChunk = startIndex - chunk.m_ChunkOffset;
            int destinationIndex = indexInChunk;
            int count1 = stringBuilder.m_ChunkLength - sourceIndex;
            if (stringBuilder != chunk)
            {
                destinationIndex = 0;
                chunk.m_ChunkLength = indexInChunk;
                stringBuilder.m_ChunkPrevious = chunk;
                stringBuilder.m_ChunkOffset = chunk.m_ChunkOffset + chunk.m_ChunkLength;
                if (indexInChunk == 0)
                {
                    stringBuilder.m_ChunkPrevious = chunk.m_ChunkPrevious;
                    chunk = stringBuilder;
                }
            }
            stringBuilder.m_ChunkLength -= sourceIndex - destinationIndex;
            if (destinationIndex == sourceIndex)
                return;
            MyBuilder.ThreadSafeCopy(stringBuilder.m_ChunkChars, sourceIndex, stringBuilder.m_ChunkChars, destinationIndex, count1);
        }

        internal static unsafe void wstrcpy(char* dmem, char* smem, int charCount)
        {
            if (charCount <= 0)
                return;
            if (((int)dmem & 2) != 0)
            {
                *dmem = *smem;
                ++dmem;
                ++smem;
                --charCount;
            }
            while (charCount >= 8)
            {
                *(int*)dmem = (int)*(uint*)smem;
                *(int*)(dmem + 2) = (int)*(uint*)(smem + 2);
                *(int*)(dmem + 4) = (int)*(uint*)(smem + 4);
                *(int*)(dmem + 6) = (int)*(uint*)(smem + 6);
                dmem += 8;
                smem += 8;
                charCount -= 8;
            }
            if ((charCount & 4) != 0)
            {
                *(int*)dmem = (int)*(uint*)smem;
                *(int*)(dmem + 2) = (int)*(uint*)(smem + 2);
                dmem += 4;
                smem += 4;
            }
            if ((charCount & 2) != 0)
            {
                *(int*)dmem = (int)*(uint*)smem;
                dmem += 2;
                smem += 2;
            }
            if ((charCount & 1) == 0)
                return;
            *dmem = *smem;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal extern static string FastAllocateString(int length);

        internal static unsafe void Memcpy(byte[] dest, int destIndex, byte* src, int srcIndex, int len)
        {
            if (len == 0)
                return;
            fixed (byte* numPtr = dest)
                Memcpy(numPtr + destIndex, src + srcIndex, len);
        }

        internal static unsafe void Memcpy(byte* pDest, int destIndex, byte[] src, int srcIndex, int len)
        {
            if (len == 0)
                return;
            fixed (byte* numPtr = src)
                Memcpy(pDest + destIndex, numPtr + srcIndex, len);
        }

        internal static unsafe void Memcpy(char* pDest, int destIndex, char* pSrc, int srcIndex, int len)
        {
            if (len == 0)
                return;
            Memcpy((byte*)(pDest + destIndex), (byte*)(pSrc + srcIndex), len * 2);
        }

        internal static unsafe void Memcpy(byte* dest, byte* src, int len)
        {
            if (len >= 16)
            {
                do
                {
                    *(int*)dest = *(int*)src;
                    *(int*)(dest + 4) = *(int*)(src + 4);
                    *(int*)(dest + 8) = *(int*)(src + 8);
                    *(int*)(dest + 12) = *(int*)(src + 12);
                    dest += 16;
                    src += 16;
                }
                while ((len -= 16) >= 16);
            }
            if (len <= 0)
                return;
            if ((len & 8) != 0)
            {
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
                dest += 8;
                src += 8;
            }
            if ((len & 4) != 0)
            {
                *(int*)dest = *(int*)src;
                dest += 4;
                src += 4;
            }
            if ((len & 2) != 0)
            {
                *(short*)dest = *(short*)src;
                dest += 2;
                src += 2;
            }
            if ((len & 1) == 0)
                return;
            *dest++ = *src++;
        }
    }
}
