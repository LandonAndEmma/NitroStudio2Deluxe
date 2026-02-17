using System;

namespace GotaSoundIO.IO
{
    public abstract class ByteConverter
    {
        protected static readonly Exception BufferException = new(
            "Buffer null or too small."
        );
        public static ByteConverter LittleEndian { get; } =
            new ByteConverterLittleEndian();
        public static ByteConverter BigEndian { get; } =
            new ByteConverterBigEndian();
        public static ByteConverter System { get; } =
            BitConverter.IsLittleEndian ? ByteConverter.LittleEndian : ByteConverter.BigEndian;
        public abstract ByteOrder ByteOrder { get; }

        public static ByteConverter GetConverter(ByteOrder byteOrder)
        {
            return byteOrder == ByteOrder.System
                ? ByteConverter.System
                : byteOrder == ByteOrder.BigEndian
                ? ByteConverter.BigEndian
                : byteOrder == ByteOrder.LittleEndian
                ? ByteConverter.LittleEndian
                : throw new ArgumentException(
                string.Format("Invalid {0}.", "ByteOrder"),
                nameof(byteOrder)
            );
        }

        public void GetBytes(decimal value, byte[] buffer, int startIndex = 0)
        {
            if (buffer != null && buffer.Length - startIndex < 16)
            {
                throw ByteConverter.BufferException;
            }

            int[] bits = decimal.GetBits(value);
            for (int index1 = 0; index1 < 4; ++index1)
            {
                int index2 = startIndex + (index1 * 4);
                int num = bits[index1];
                buffer[index2] = (byte)num;
                buffer[index2 + 1] = (byte)(num >> 8);
                buffer[index2 + 2] = (byte)(num >> 16);
                buffer[index2 + 3] = (byte)(num >> 24);
            }
        }

        public abstract void GetBytes(double value, byte[] buffer, int startIndex = 0);
        public abstract void GetBytes(short value, byte[] buffer, int startIndex = 0);
        public abstract void GetBytes(int value, byte[] buffer, int startIndex = 0);
        public abstract void GetBytes(long value, byte[] buffer, int startIndex = 0);
        public abstract void GetBytes(float value, byte[] buffer, int startIndex = 0);
        public abstract void GetBytes(ushort value, byte[] buffer, int startIndex = 0);
        public abstract void GetBytes(uint value, byte[] buffer, int startIndex = 0);
        public abstract void GetBytes(ulong value, byte[] buffer, int startIndex = 0);

        public decimal ToDecimal(byte[] buffer, int startIndex = 0)
        {
            if (buffer != null && buffer.Length - startIndex < 16)
            {
                throw ByteConverter.BufferException;
            }

            int[] bits = new int[4];
            for (int index1 = 0; index1 < 4; ++index1)
            {
                int index2 = startIndex + (index1 * 4);
                bits[index1] =
                    buffer[index2]
                    | (buffer[index2 + 1] << 8)
                    | (buffer[index2 + 2] << 16)
                    | (buffer[index2 + 3] << 24);
            }
            return new decimal(bits);
        }

        public abstract double ToDouble(byte[] buffer, int startIndex = 0);
        public abstract short ToInt16(byte[] buffer, int startIndex = 0);
        public abstract int ToInt32(byte[] buffer, int startIndex = 0);
        public abstract long ToInt64(byte[] buffer, int startIndex = 0);
        public abstract float ToSingle(byte[] buffer, int startIndex = 0);
        public abstract ushort ToUInt16(byte[] buffer, int startIndex = 0);
        public abstract uint ToUInt32(byte[] buffer, int startIndex = 0);
        public abstract ulong ToUInt64(byte[] buffer, int startIndex = 0);
    }
}
