using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace GotaSoundIO.IO
{
    public static class StreamExtensions
    {
        private static readonly DateTime _cTimeBase = new(1970, 1, 1);

        [field: ThreadStatic]
        private static byte[] Buffer
        {
            get
            {
                field ??= new byte[16];
                return field;
            }
        }

        [field: ThreadStatic]
        private static char[] CharBuffer
        {
            get
            {
                field ??= new char[16];
                return field;
            }
        }

        public static long Align(this Stream stream, int alignment, bool grow = false)
        {
            if (alignment <= 0)
            {
                throw new ArgumentOutOfRangeException("Alignment must be bigger than 0.");
            }

            long num = stream.Seek(
                ((-stream.Position % alignment) + alignment) % alignment,
                SeekOrigin.Current
            );
            if (grow && num > stream.Length)
            {
                stream.SetLength(num);
            }

            return num;
        }

        public static bool IsEndOfStream(this Stream stream)
        {
            return stream.Position >= stream.Length;
        }

        private static void ValidateEnumValue(Type enumType, object value)
        {
            if (!EnumExtensions.IsValid(enumType, value))
            {
                throw new InvalidDataException(
                    string.Format(
                        "Read value {0} is not defined in the enum type {1}.",
                        value,
                        enumType
                    )
                );
            }
        }

        public static bool ReadBoolean(
            this Stream stream,
            BooleanDataFormat format = BooleanDataFormat.Byte
        )
        {
            return format switch
            {
                BooleanDataFormat.Byte => (uint)stream.ReadByte() > 0U,
                BooleanDataFormat.Word => (uint)stream.ReadInt16(null) > 0U,
                BooleanDataFormat.Dword => (uint)stream.ReadInt32(null) > 0U,
                _ => throw new ArgumentException(
                                        string.Format("Invalid {0}.", "BooleanDataFormat"),
                                        nameof(format)
                                    ),
            };
        }

        public static bool[] ReadBooleans(
            this Stream stream,
            int count,
            BooleanDataFormat format = BooleanDataFormat.Byte
        )
        {
            bool[] flagArray = new bool[count];
            lock (stream)
            {
                switch (format)
                {
                    case BooleanDataFormat.Byte:
                        for (int index = 0; index < count; ++index)
                        {
                            flagArray[index] = (uint)stream.ReadByte() > 0U;
                        }

                        break;
                    case BooleanDataFormat.Word:
                        for (int index = 0; index < count; ++index)
                        {
                            flagArray[index] = (uint)stream.ReadInt16(null) > 0U;
                        }

                        break;
                    case BooleanDataFormat.Dword:
                        for (int index = 0; index < count; ++index)
                        {
                            flagArray[index] = (uint)stream.ReadInt32(null) > 0U;
                        }

                        break;
                    default:
                        throw new ArgumentException(
                            string.Format("Invalid {0}.", "BooleanDataFormat"),
                            nameof(format)
                        );
                }
            }
            return flagArray;
        }

        public static byte Read1Byte(this Stream stream)
        {
            return (byte)stream.ReadByte();
        }

        public static byte[] ReadBytes(this Stream stream, int count)
        {
            byte[] buffer = new byte[count];
            stream.ReadExactly(buffer, 0, count);
            return buffer;
        }

        public static DateTime ReadDateTime(
            this Stream stream,
            DateTimeDataFormat format = DateTimeDataFormat.NetTicks,
            ByteConverter converter = null
        )
        {
            return format switch
            {
                DateTimeDataFormat.NetTicks => new DateTime(stream.ReadInt64(converter)),
                DateTimeDataFormat.CTime => StreamExtensions._cTimeBase.AddSeconds(
                                        stream.ReadUInt32(converter)
                                    ),
                DateTimeDataFormat.CTime64 => StreamExtensions._cTimeBase.AddSeconds(
                                        stream.ReadInt64(converter)
                                    ),
                _ => throw new ArgumentException(
                                        string.Format("Invalid {0}.", "DateTimeDataFormat"),
                                        nameof(format)
                                    ),
            };
        }

        public static DateTime[] ReadDateTimes(
            this Stream stream,
            int count,
            DateTimeDataFormat format = DateTimeDataFormat.NetTicks,
            ByteConverter converter = null
        )
        {
            DateTime[] dateTimeArray = new DateTime[count];
            lock (stream)
            {
                switch (format)
                {
                    case DateTimeDataFormat.NetTicks:
                        for (int index = 0; index < count; ++index)
                        {
                            dateTimeArray[index] = new DateTime(stream.ReadInt64(converter));
                        }

                        break;
                    case DateTimeDataFormat.CTime:
                        for (int index = 0; index < count; ++index)
                        {
                            dateTimeArray[index] = StreamExtensions._cTimeBase.AddSeconds(
                                stream.ReadUInt32(converter)
                            );
                        }

                        break;
                    case DateTimeDataFormat.CTime64:
                        for (int index = 0; index < count; ++index)
                        {
                            dateTimeArray[index] = StreamExtensions._cTimeBase.AddSeconds(
                                stream.ReadInt64(converter)
                            );
                        }

                        break;
                    default:
                        throw new ArgumentException(
                            string.Format("Invalid {0}.", "BooleanDataFormat"),
                            nameof(format)
                        );
                }
            }
            return dateTimeArray;
        }

        public static decimal ReadDecimal(this Stream stream, ByteConverter converter = null)
        {
            StreamExtensions.FillBuffer(stream, 16);
            return (converter ?? ByteConverter.System).ToDecimal(StreamExtensions.Buffer, 0);
        }

        public static decimal[] ReadDecimals(
            this Stream stream,
            int count,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            decimal[] numArray = new decimal[count];
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                for (int index = 0; index < count; ++index)
                {
                    StreamExtensions.FillBuffer(stream, 16);
                    numArray[index] = converter.ToDecimal(buffer, 0);
                }
            }
            return numArray;
        }

        public static double ReadDouble(this Stream stream, ByteConverter converter = null)
        {
            StreamExtensions.FillBuffer(stream, 8);
            return (converter ?? ByteConverter.System).ToDouble(StreamExtensions.Buffer, 0);
        }

        public static double[] ReadDoubles(
            this Stream stream,
            int count,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            double[] numArray = new double[count];
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                for (int index = 0; index < count; ++index)
                {
                    StreamExtensions.FillBuffer(stream, 8);
                    numArray[index] = converter.ToDouble(buffer, 0);
                }
            }
            return numArray;
        }

        public static T ReadEnum<T>(
            this Stream stream,
            bool strict = false,
            ByteConverter converter = null
        )
            where T : struct, IComparable, IFormattable
        {
            return (T)StreamExtensions.ReadEnum(stream, typeof(T), strict, converter);
        }

        public static T[] ReadEnums<T>(
            this Stream stream,
            int count,
            bool strict = false,
            ByteConverter converter = null
        )
            where T : struct, IComparable, IFormattable
        {
            converter ??= ByteConverter.System;
            T[] objArray = new T[count];
            Type enumType = typeof(T);
            lock (stream)
            {
                for (int index = 0; index < count; ++index)
                {
                    objArray[index] = (T)
                        StreamExtensions.ReadEnum(stream, enumType, strict, converter);
                }
            }
            return objArray;
        }

        public static short ReadInt16(this Stream stream, ByteConverter converter = null)
        {
            StreamExtensions.FillBuffer(stream, 2);
            return (converter ?? ByteConverter.System).ToInt16(StreamExtensions.Buffer, 0);
        }

        public static short[] ReadInt16s(
            this Stream stream,
            int count,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            short[] numArray = new short[count];
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                for (int index = 0; index < count; ++index)
                {
                    StreamExtensions.FillBuffer(stream, 2);
                    numArray[index] = converter.ToInt16(buffer, 0);
                }
            }
            return numArray;
        }

        public static int ReadInt32(this Stream stream, ByteConverter converter = null)
        {
            StreamExtensions.FillBuffer(stream, 4);
            return (converter ?? ByteConverter.System).ToInt32(StreamExtensions.Buffer, 0);
        }

        public static int[] ReadInt32s(
            this Stream stream,
            int count,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            int[] numArray = new int[count];
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                for (int index = 0; index < count; ++index)
                {
                    StreamExtensions.FillBuffer(stream, 4);
                    numArray[index] = converter.ToInt32(buffer, 0);
                }
            }
            return numArray;
        }

        public static long ReadInt64(this Stream stream, ByteConverter converter = null)
        {
            StreamExtensions.FillBuffer(stream, 8);
            return (converter ?? ByteConverter.System).ToInt64(StreamExtensions.Buffer, 0);
        }

        public static long[] ReadInt64s(
            this Stream stream,
            int count,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            long[] numArray = new long[count];
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                for (int index = 0; index < count; ++index)
                {
                    StreamExtensions.FillBuffer(stream, 8);
                    numArray[index] = converter.ToInt64(buffer, 0);
                }
            }
            return numArray;
        }

        public static sbyte ReadSByte(this Stream stream)
        {
            return (sbyte)stream.ReadByte();
        }

        public static sbyte[] ReadSBytes(this Stream stream, int count)
        {
            sbyte[] numArray = new sbyte[count];
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                for (int index = 0; index < count; ++index)
                {
                    StreamExtensions.FillBuffer(stream, 1);
                    numArray[index] = (sbyte)buffer[0];
                }
            }
            return numArray;
        }

        public static float ReadSingle(this Stream stream, ByteConverter converter = null)
        {
            StreamExtensions.FillBuffer(stream, 4);
            return (converter ?? ByteConverter.System).ToSingle(StreamExtensions.Buffer, 0);
        }

        public static float[] ReadSingles(
            this Stream stream,
            int count,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            float[] numArray = new float[count];
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                for (int index = 0; index < count; ++index)
                {
                    StreamExtensions.FillBuffer(stream, 4);
                    numArray[index] = converter.ToSingle(buffer, 0);
                }
            }
            return numArray;
        }

        public static string ReadString(
            this Stream stream,
            StringDataFormat format = StringDataFormat.DynamicByteCount,
            Encoding encoding = null,
            ByteConverter converter = null
        )
        {
            encoding ??= Encoding.UTF8;
            converter ??= ByteConverter.System;
            return format switch
            {
                StringDataFormat.DynamicByteCount => StreamExtensions.ReadStringWithLength(
                                        stream,
                                        StreamExtensions.Read7BitEncodedInt32(stream),
                                        false,
                                        encoding
                                    ),
                StringDataFormat.ByteCharCount => StreamExtensions.ReadStringWithLength(
                                        stream,
                                        stream.ReadByte(),
                                        true,
                                        encoding
                                    ),
                StringDataFormat.Int16CharCount => StreamExtensions.ReadStringWithLength(
                                        stream,
                                        stream.ReadInt16(converter),
                                        true,
                                        encoding
                                    ),
                StringDataFormat.Int32CharCount => StreamExtensions.ReadStringWithLength(
                                        stream,
                                        stream.ReadInt32(converter),
                                        true,
                                        encoding
                                    ),
                StringDataFormat.ZeroTerminated => StreamExtensions.ReadStringZeroPostfix(stream, encoding),
                _ => throw new ArgumentException(
                                        string.Format("Invalid {0}.", "StringDataFormat"),
                                        nameof(format)
                                    ),
            };
        }

        public static string[] ReadStrings(
            this Stream stream,
            int count,
            StringDataFormat format = StringDataFormat.DynamicByteCount,
            Encoding encoding = null,
            ByteConverter converter = null
        )
        {
            encoding ??= Encoding.UTF8;
            converter ??= ByteConverter.System;
            string[] strArray = new string[count];
            lock (stream)
            {
                switch (format)
                {
                    case StringDataFormat.DynamicByteCount:
                        for (int index = 0; index < count; ++index)
                        {
                            strArray[index] = StreamExtensions.ReadStringWithLength(
                                stream,
                                StreamExtensions.Read7BitEncodedInt32(stream),
                                false,
                                encoding
                            );
                        }

                        break;
                    case StringDataFormat.ByteCharCount:
                        for (int index = 0; index < count; ++index)
                        {
                            strArray[index] = StreamExtensions.ReadStringWithLength(
                                stream,
                                stream.ReadByte(),
                                true,
                                encoding
                            );
                        }

                        break;
                    case StringDataFormat.Int16CharCount:
                        for (int index = 0; index < count; ++index)
                        {
                            strArray[index] = StreamExtensions.ReadStringWithLength(
                                stream,
                                stream.ReadInt16(converter),
                                true,
                                encoding
                            );
                        }

                        break;
                    case StringDataFormat.Int32CharCount:
                        for (int index = 0; index < count; ++index)
                        {
                            strArray[index] = StreamExtensions.ReadStringWithLength(
                                stream,
                                stream.ReadInt32(converter),
                                true,
                                encoding
                            );
                        }

                        break;
                    case StringDataFormat.ZeroTerminated:
                        for (int index = 0; index < count; ++index)
                        {
                            strArray[index] = StreamExtensions.ReadStringZeroPostfix(
                                stream,
                                encoding
                            );
                        }

                        break;
                    default:
                        throw new ArgumentException(
                            string.Format("Invalid {0}.", "StringDataFormat"),
                            nameof(format)
                        );
                }
            }
            return strArray;
        }

        public static string ReadString(this Stream stream, int length, Encoding encoding = null)
        {
            return StreamExtensions.ReadStringWithLength(
                stream,
                length,
                true,
                encoding ?? Encoding.UTF8
            );
        }

        public static string[] ReadStrings(
            this Stream stream,
            int count,
            int length,
            Encoding encoding = null
        )
        {
            encoding ??= Encoding.UTF8;
            string[] strArray = new string[count];
            lock (stream)
            {
                for (int index = 0; index < count; ++index)
                {
                    strArray[index] = StreamExtensions.ReadStringWithLength(
                        stream,
                        length,
                        true,
                        encoding
                    );
                }
            }
            return strArray;
        }

        public static ushort ReadUInt16(this Stream stream, ByteConverter converter = null)
        {
            StreamExtensions.FillBuffer(stream, 2);
            return (converter ?? ByteConverter.System).ToUInt16(StreamExtensions.Buffer, 0);
        }

        public static ushort[] ReadUInt16s(
            this Stream stream,
            int count,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            ushort[] numArray = new ushort[count];
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                for (int index = 0; index < count; ++index)
                {
                    StreamExtensions.FillBuffer(stream, 2);
                    numArray[index] = converter.ToUInt16(buffer, 0);
                }
            }
            return numArray;
        }

        public static uint ReadUInt32(this Stream stream, ByteConverter converter = null)
        {
            StreamExtensions.FillBuffer(stream, 4);
            return (converter ?? ByteConverter.System).ToUInt32(StreamExtensions.Buffer, 0);
        }

        public static uint[] ReadUInt32s(
            this Stream stream,
            int count,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            uint[] numArray = new uint[count];
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                for (int index = 0; index < count; ++index)
                {
                    StreamExtensions.FillBuffer(stream, 4);
                    numArray[index] = converter.ToUInt32(buffer, 0);
                }
            }
            return numArray;
        }

        public static ulong ReadUInt64(this Stream stream, ByteConverter converter = null)
        {
            StreamExtensions.FillBuffer(stream, 8);
            return (converter ?? ByteConverter.System).ToUInt64(StreamExtensions.Buffer, 0);
        }

        public static ulong[] ReadUInt64s(
            this Stream stream,
            int count,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            ulong[] numArray = new ulong[count];
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                for (int index = 0; index < count; ++index)
                {
                    StreamExtensions.FillBuffer(stream, 8);
                    numArray[index] = converter.ToUInt64(buffer, 0);
                }
            }
            return numArray;
        }

        private static void FillBuffer(Stream stream, int length)
        {
            if (stream.Read(StreamExtensions.Buffer, 0, length) < length)
            {
                throw new EndOfStreamException(
                    string.Format("Could not read {0} bytes.", length)
                );
            }
        }

        private static int Read7BitEncodedInt32(Stream stream)
        {
            int num1 = 0;
            for (int index = 0; index < 5; ++index)
            {
                int num2 = stream.ReadByte();
                if (num2 == -1)
                {
                    throw new EndOfStreamException("Incomplete 7-bit encoded integer.");
                }

                num1 |= (num2 & sbyte.MaxValue) << (index * 7);
                if ((num2 & 128) == 0)
                {
                    return num1;
                }
            }
            throw new InvalidDataException("Invalid 7-bit encoded integer.");
        }

        private static object ReadEnum(
            Stream stream,
            Type enumType,
            bool strict,
            ByteConverter converter
        )
        {
            converter ??= ByteConverter.System;
            Type underlyingType = Enum.GetUnderlyingType(enumType);
            int length = Marshal.SizeOf(underlyingType);
            StreamExtensions.FillBuffer(stream, length);
            object obj;
            if (underlyingType == typeof(byte))
            {
                obj = StreamExtensions.Buffer[0];
            }
            else if (underlyingType == typeof(sbyte))
            {
                obj = (sbyte)StreamExtensions.Buffer[0];
            }
            else if (underlyingType == typeof(short))
            {
                obj = converter.ToInt16(StreamExtensions.Buffer, 0);
            }
            else if (underlyingType == typeof(int))
            {
                obj = converter.ToInt32(StreamExtensions.Buffer, 0);
            }
            else if (underlyingType == typeof(long))
            {
                obj = converter.ToInt64(StreamExtensions.Buffer, 0);
            }
            else if (underlyingType == typeof(ushort))
            {
                obj = converter.ToUInt16(StreamExtensions.Buffer, 0);
            }
            else if (underlyingType == typeof(uint))
            {
                obj = converter.ToUInt32(StreamExtensions.Buffer, 0);
            }
            else
            {
                if (!(underlyingType == typeof(ulong)))
                {
                    throw new NotImplementedException(
                        string.Format("Unsupported enum type {0}.", underlyingType)
                    );
                }

                obj = converter.ToUInt64(StreamExtensions.Buffer, 0);
            }
            if (strict)
            {
                StreamExtensions.ValidateEnumValue(enumType, obj);
            }

            return obj;
        }

        private static string ReadStringWithLength(
            Stream stream,
            int length,
            bool lengthInChars,
            Encoding encoding
        )
        {
            if (length == 0)
            {
                return string.Empty;
            }

            Decoder decoder = encoding.GetDecoder();
            StringBuilder stringBuilder = new(length);
            int num1 = 0;
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                char[] charBuffer = StreamExtensions.CharBuffer;
                do
                {
                    do
                    {
                        int num2 = 0;
                        int charCount = 0;
                        while (charCount == 0)
                        {
                            int num3 = stream.Read(buffer, num2++, 1);
                            if (num3 == 0)
                            {
                                throw new EndOfStreamException(
                                    "Incomplete string data, missing requested length."
                                );
                            }

                            num1 += num3;
                            charCount = decoder.GetCharCount(buffer, 0, num2);
                            if (charCount > 0)
                            {
                                _ = decoder.GetChars(buffer, 0, num2, charBuffer, 0);
                                _ = stringBuilder.Append(charBuffer, 0, charCount);
                            }
                        }
                    } while (lengthInChars && stringBuilder.Length < length);
                    if (lengthInChars)
                    {
                        break;
                    }
                } while (num1 < length);
            }
            return stringBuilder.ToString();
        }

        private static string ReadStringZeroPostfix(Stream stream, Encoding encoding)
        {
            List<byte> byteList = [];
            bool flag = true;
            byte[] buffer = StreamExtensions.Buffer;
            lock (stream)
            {
                switch (encoding.GetByteCount("A"))
                {
                    case 1:
                        while (flag)
                        {
                            StreamExtensions.FillBuffer(stream, 1);
                            if (flag = buffer[0] > 0)
                            {
                                byteList.Add(buffer[0]);
                            }
                        }
                        break;
                    case 2:
                        while (flag)
                        {
                            StreamExtensions.FillBuffer(stream, 2);
                            if (flag = buffer[0] != 0 || buffer[1] > 0)
                            {
                                byteList.Add(buffer[0]);
                                byteList.Add(buffer[1]);
                            }
                        }
                        break;
                    default:
                        throw new NotImplementedException(
                            "Unhandled character byte count. Only 1- or 2-byte encodings are support at the moment."
                        );
                }
            }
            return encoding.GetString(byteList.ToArray());
        }

        public static void Write(
            this Stream stream,
            bool value,
            BooleanDataFormat format = BooleanDataFormat.Byte,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            switch (format)
            {
                case BooleanDataFormat.Byte:
                    stream.WriteByte(value ? (byte)1 : (byte)0);
                    break;
                case BooleanDataFormat.Word:
                    byte[] buffer1 = StreamExtensions.Buffer;
                    converter.GetBytes(value ? (short)1 : (short)0, buffer1, 0);
                    stream.Write(StreamExtensions.Buffer, 0, 2);
                    break;
                case BooleanDataFormat.Dword:
                    byte[] buffer2 = StreamExtensions.Buffer;
                    converter.GetBytes(value ? 1 : 0, buffer2, 0);
                    stream.Write(StreamExtensions.Buffer, 0, 4);
                    break;
                default:
                    throw new ArgumentException(
                        string.Format("Invalid {0}.", "BooleanDataFormat"),
                        nameof(format)
                    );
            }
        }

        public static void Write(
            this Stream stream,
            IEnumerable<bool> values,
            BooleanDataFormat format = BooleanDataFormat.Byte,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            lock (stream)
            {
                switch (format)
                {
                    case BooleanDataFormat.Byte:
                        using (IEnumerator<bool> enumerator = values.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                bool current = enumerator.Current;
                                stream.WriteByte(current ? (byte)1 : (byte)0);
                            }
                            break;
                        }
                    case BooleanDataFormat.Word:
                        byte[] buffer1 = StreamExtensions.Buffer;
                        using (IEnumerator<bool> enumerator = values.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                bool current = enumerator.Current;
                                converter.GetBytes(current ? (short)1 : (short)0, buffer1, 0);
                                stream.Write(StreamExtensions.Buffer, 0, 2);
                            }
                            break;
                        }
                    case BooleanDataFormat.Dword:
                        byte[] buffer2 = StreamExtensions.Buffer;
                        using (IEnumerator<bool> enumerator = values.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                bool current = enumerator.Current;
                                converter.GetBytes(current ? 1 : 0, buffer2, 0);
                                stream.Write(StreamExtensions.Buffer, 0, 4);
                            }
                            break;
                        }
                    default:
                        throw new ArgumentException(
                            string.Format("Invalid {0}.", "BooleanDataFormat"),
                            nameof(format)
                        );
                }
            }
        }

        public static void Write(this Stream stream, byte value)
        {
            stream.WriteByte(value);
        }

        public static void Write(this Stream stream, IEnumerable<byte> values)
        {
            lock (stream)
            {
                foreach (byte num in values)
                {
                    stream.WriteByte(num);
                }
            }
        }

        public static void Write(
            this Stream stream,
            DateTime value,
            DateTimeDataFormat format = DateTimeDataFormat.NetTicks,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            switch (format)
            {
                case DateTimeDataFormat.NetTicks:
                    stream.Write(value.Ticks, converter);
                    break;
                case DateTimeDataFormat.CTime:
                    stream.Write((uint)(new DateTime(1970, 1, 1) - value).TotalSeconds, converter);
                    break;
                case DateTimeDataFormat.CTime64:
                    stream.Write((ulong)(new DateTime(1970, 1, 1) - value).TotalSeconds, converter);
                    break;
                default:
                    throw new ArgumentException(
                        string.Format("Invalid {0}.", "DateTimeDataFormat"),
                        nameof(format)
                    );
            }
        }

        public static void Write(
            this Stream stream,
            IEnumerable<DateTime> values,
            DateTimeDataFormat format = DateTimeDataFormat.NetTicks,
            ByteConverter converter = null
        )
        {
            _ = converter ?? ByteConverter.System;
        }

        public static void Write(this Stream stream, decimal value, ByteConverter converter = null)
        {
            byte[] buffer = StreamExtensions.Buffer;
            (converter ?? ByteConverter.System).GetBytes(value, buffer, 0);
            stream.Write(buffer, 0, 16);
        }

        public static void Write(
            this Stream stream,
            IEnumerable<decimal> values,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                foreach (decimal num in values)
                {
                    converter.GetBytes(num, buffer, 0);
                    stream.Write(buffer, 0, 16);
                }
            }
        }

        public static void Write(this Stream stream, double value, ByteConverter converter = null)
        {
            byte[] buffer = StreamExtensions.Buffer;
            (converter ?? ByteConverter.System).GetBytes(value, buffer, 0);
            stream.Write(buffer, 0, 8);
        }

        public static void Write(
            this Stream stream,
            IEnumerable<double> values,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                foreach (double num in values)
                {
                    converter.GetBytes(num, buffer, 0);
                    stream.Write(buffer, 0, 8);
                }
            }
        }

        public static void WriteEnum<T>(
            this Stream stream,
            T value,
            bool strict = false,
            ByteConverter converter = null
        )
            where T : struct, IComparable, IFormattable
        {
            StreamExtensions.WriteEnum(stream, typeof(T), value, strict, converter);
        }

        public static void WriteEnums<T>(
            this Stream stream,
            IEnumerable<T> values,
            bool strict = false,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            Type enumType = typeof(T);
            lock (stream)
            {
                foreach (T obj in values)
                {
                    StreamExtensions.WriteEnum(stream, enumType, obj, strict, converter);
                }
            }
        }

        public static void Write(this Stream stream, short value, ByteConverter converter = null)
        {
            byte[] buffer = StreamExtensions.Buffer;
            (converter ?? ByteConverter.System).GetBytes(value, buffer, 0);
            stream.Write(buffer, 0, 2);
        }

        public static void Write(
            this Stream stream,
            IEnumerable<short> values,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                foreach (short num in values)
                {
                    converter.GetBytes(num, buffer, 0);
                    stream.Write(buffer, 0, 2);
                }
            }
        }

        public static void Write(this Stream stream, int value, ByteConverter converter = null)
        {
            byte[] buffer = StreamExtensions.Buffer;
            (converter ?? ByteConverter.System).GetBytes(value, buffer, 0);
            stream.Write(buffer, 0, 4);
        }

        public static void Write(
            this Stream stream,
            IEnumerable<int> values,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                foreach (int num in values)
                {
                    converter.GetBytes(num, buffer, 0);
                    stream.Write(buffer, 0, 4);
                }
            }
        }

        public static void Write(this Stream stream, long value, ByteConverter converter = null)
        {
            byte[] buffer = StreamExtensions.Buffer;
            (converter ?? ByteConverter.System).GetBytes(value, buffer, 0);
            stream.Write(buffer, 0, 8);
        }

        public static void Write(
            this Stream stream,
            IEnumerable<long> values,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                foreach (long num in values)
                {
                    converter.GetBytes(num, buffer, 0);
                    stream.Write(buffer, 0, 8);
                }
            }
        }

        public static void Write(this Stream stream, sbyte value)
        {
            byte[] buffer = StreamExtensions.Buffer;
            buffer[0] = (byte)value;
            stream.Write(buffer, 0, 1);
        }

        public static void Write(this Stream stream, IEnumerable<sbyte> values)
        {
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                foreach (sbyte num in values)
                {
                    buffer[0] = (byte)num;
                    stream.Write(buffer, 0, 1);
                }
            }
        }

        public static void Write(this Stream stream, float value, ByteConverter converter = null)
        {
            byte[] buffer = StreamExtensions.Buffer;
            (converter ?? ByteConverter.System).GetBytes(value, buffer, 0);
            stream.Write(buffer, 0, 4);
        }

        public static void Write(
            this Stream stream,
            IEnumerable<float> values,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                foreach (float num in values)
                {
                    converter.GetBytes(num, buffer, 0);
                    stream.Write(buffer, 0, 4);
                }
            }
        }

        public static void Write(
            this Stream stream,
            string value,
            StringDataFormat format = StringDataFormat.DynamicByteCount,
            Encoding encoding = null,
            ByteConverter converter = null
        )
        {
            encoding ??= Encoding.UTF8;
            converter ??= ByteConverter.System;
            byte[] bytes = encoding.GetBytes(value);
            lock (stream)
            {
                switch (format)
                {
                    case StringDataFormat.DynamicByteCount:
                        StreamExtensions.Write7BitEncodedInt(stream, bytes.Length);
                        stream.Write(bytes, 0, bytes.Length);
                        break;
                    case StringDataFormat.ByteCharCount:
                        stream.WriteByte((byte)value.Length);
                        stream.Write(bytes, 0, bytes.Length);
                        break;
                    case StringDataFormat.Int16CharCount:
                        converter.GetBytes((short)value.Length, StreamExtensions.Buffer, 0);
                        stream.Write(StreamExtensions.Buffer, 0, 2);
                        stream.Write(bytes, 0, bytes.Length);
                        break;
                    case StringDataFormat.Int32CharCount:
                        converter.GetBytes(value.Length, StreamExtensions.Buffer, 0);
                        stream.Write(StreamExtensions.Buffer, 0, 4);
                        stream.Write(bytes, 0, bytes.Length);
                        break;
                    case StringDataFormat.ZeroTerminated:
                        stream.Write(bytes, 0, bytes.Length);
                        switch (encoding.GetByteCount("A"))
                        {
                            case 1:
                                stream.WriteByte(0);
                                return;
                            case 2:
                                stream.WriteByte(0);
                                stream.WriteByte(0);
                                return;
                            default:
                                return;
                        }
                    case StringDataFormat.Raw:
                        stream.Write(bytes, 0, bytes.Length);
                        break;
                    default:
                        throw new ArgumentException(
                            string.Format("Invalid {0}.", "StringDataFormat"),
                            nameof(format)
                        );
                }
            }
        }

        public static void Write(
            this Stream stream,
            IEnumerable<string> values,
            StringDataFormat format = StringDataFormat.DynamicByteCount,
            Encoding encoding = null,
            ByteConverter converter = null
        )
        {
            encoding ??= Encoding.UTF8;
            converter ??= ByteConverter.System;
            lock (stream)
            {
                foreach (string str in values)
                {
                    stream.Write(str, format, encoding, converter);
                }
            }
        }

        public static void Write(this Stream stream, ushort value, ByteConverter converter = null)
        {
            byte[] buffer = StreamExtensions.Buffer;
            (converter ?? ByteConverter.System).GetBytes(value, buffer, 0);
            stream.Write(buffer, 0, 2);
        }

        public static void Write(
            this Stream stream,
            IEnumerable<ushort> values,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                foreach (ushort num in values)
                {
                    converter.GetBytes(num, buffer, 0);
                    stream.Write(buffer, 0, 2);
                }
            }
        }

        public static void Write(this Stream stream, uint value, ByteConverter converter = null)
        {
            byte[] buffer = StreamExtensions.Buffer;
            (converter ?? ByteConverter.System).GetBytes(value, buffer, 0);
            stream.Write(buffer, 0, 4);
        }

        public static void Write(
            this Stream stream,
            IEnumerable<uint> values,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                foreach (uint num in values)
                {
                    converter.GetBytes(num, buffer, 0);
                    stream.Write(buffer, 0, 4);
                }
            }
        }

        public static void Write(this Stream stream, ulong value, ByteConverter converter = null)
        {
            byte[] buffer = StreamExtensions.Buffer;
            (converter ?? ByteConverter.System).GetBytes(value, buffer, 0);
            stream.Write(buffer, 0, 8);
        }

        public static void Write(
            this Stream stream,
            IEnumerable<ulong> values,
            ByteConverter converter = null
        )
        {
            converter ??= ByteConverter.System;
            lock (stream)
            {
                byte[] buffer = StreamExtensions.Buffer;
                foreach (ulong num in values)
                {
                    converter.GetBytes(num, buffer, 0);
                    stream.Write(buffer, 0, 8);
                }
            }
        }

        private static void Write7BitEncodedInt(Stream stream, int value)
        {
            for (; value >= 128; value >>= 7)
            {
                stream.WriteByte((byte)(value | 128));
            }

            stream.WriteByte((byte)value);
        }

        private static void WriteEnum(
            Stream stream,
            Type enumType,
            object value,
            bool strict,
            ByteConverter converter
        )
        {
            converter ??= ByteConverter.System;
            Type underlyingType = Enum.GetUnderlyingType(enumType);
            byte[] buffer = StreamExtensions.Buffer;
            if (underlyingType == typeof(byte))
            {
                StreamExtensions.Buffer[0] = (byte)value;
            }
            else if (underlyingType == typeof(sbyte))
            {
                StreamExtensions.Buffer[0] = (byte)(sbyte)value;
            }
            else if (underlyingType == typeof(short))
            {
                converter.GetBytes((short)value, buffer, 0);
            }
            else if (underlyingType == typeof(int))
            {
                converter.GetBytes((int)value, buffer, 0);
            }
            else if (underlyingType == typeof(long))
            {
                converter.GetBytes((long)value, buffer, 0);
            }
            else if (underlyingType == typeof(ushort))
            {
                converter.GetBytes((ushort)value, buffer, 0);
            }
            else if (underlyingType == typeof(uint))
            {
                converter.GetBytes((uint)value, buffer, 0);
            }
            else
            {
                if (!(underlyingType == typeof(ulong)))
                {
                    throw new NotImplementedException(
                        string.Format("Unsupported enum type {0}.", underlyingType)
                    );
                }

                converter.GetBytes((ulong)value, buffer, 0);
            }
            if (strict)
            {
                StreamExtensions.ValidateEnumValue(enumType, value);
            }

            stream.Write(buffer, 0, Marshal.SizeOf(underlyingType));
        }
    }
}
