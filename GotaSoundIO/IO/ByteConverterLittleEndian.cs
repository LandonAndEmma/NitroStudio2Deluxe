using System;

namespace GotaSoundIO.IO
{
    public sealed class ByteConverterLittleEndian : ByteConverter
    {
        public override ByteOrder ByteOrder => ByteOrder.LittleEndian;

        public override void GetBytes(double value, byte[] buffer, int startIndex = 0)
        {
            byte[] doubleBytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(doubleBytes);
            Array.Copy(doubleBytes, 0, buffer, startIndex, 8);
        }

        public override void GetBytes(short value, byte[] buffer, int startIndex = 0)
        {
            buffer[startIndex] = (byte)value;
            buffer[startIndex + 1] = (byte)((uint)value >> 8);
        }

        public override void GetBytes(int value, byte[] buffer, int startIndex = 0)
        {
            buffer[startIndex] = (byte)value;
            buffer[startIndex + 1] = (byte)(value >> 8);
            buffer[startIndex + 2] = (byte)(value >> 16);
            buffer[startIndex + 3] = (byte)(value >> 24);
        }

        public override void GetBytes(long value, byte[] buffer, int startIndex = 0)
        {
            buffer[startIndex] = (byte)value;
            buffer[startIndex + 1] = (byte)(value >> 8);
            buffer[startIndex + 2] = (byte)(value >> 16);
            buffer[startIndex + 3] = (byte)(value >> 24);
            buffer[startIndex + 4] = (byte)(value >> 32);
            buffer[startIndex + 5] = (byte)(value >> 40);
            buffer[startIndex + 6] = (byte)(value >> 48);
            buffer[startIndex + 7] = (byte)(value >> 56);
        }

        public override void GetBytes(float value, byte[] buffer, int startIndex = 0)
        {
            byte[] floatBytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(floatBytes);
            Array.Copy(floatBytes, 0, buffer, startIndex, 4);
        }

        public override void GetBytes(ushort value, byte[] buffer, int startIndex = 0)
        {
            buffer[startIndex] = (byte)value;
            buffer[startIndex + 1] = (byte)((uint)value >> 8);
        }

        public override void GetBytes(uint value, byte[] buffer, int startIndex = 0)
        {
            buffer[startIndex] = (byte)value;
            buffer[startIndex + 1] = (byte)(value >> 8);
            buffer[startIndex + 2] = (byte)(value >> 16);
            buffer[startIndex + 3] = (byte)(value >> 24);
        }

        public override void GetBytes(ulong value, byte[] buffer, int startIndex = 0)
        {
            buffer[startIndex] = (byte)value;
            buffer[startIndex + 1] = (byte)(value >> 8);
            buffer[startIndex + 2] = (byte)(value >> 16);
            buffer[startIndex + 3] = (byte)(value >> 24);
            buffer[startIndex + 4] = (byte)(value >> 32);
            buffer[startIndex + 5] = (byte)(value >> 40);
            buffer[startIndex + 6] = (byte)(value >> 48);
            buffer[startIndex + 7] = (byte)(value >> 56);
        }

        public override double ToDouble(byte[] buffer, int startIndex = 0)
        {
            byte[] doubleBytes = new byte[8];
            Array.Copy(buffer, startIndex, doubleBytes, 0, 8);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(doubleBytes);
            return BitConverter.ToDouble(doubleBytes, 0);
        }

        public override short ToInt16(byte[] buffer, int startIndex = 0)
        {
            return (short)(buffer[startIndex] | (buffer[startIndex + 1] << 8));
        }

        public override int ToInt32(byte[] buffer, int startIndex = 0)
        {
            return buffer[startIndex]
                | (buffer[startIndex + 1] << 8)
                | (buffer[startIndex + 2] << 16)
                | (buffer[startIndex + 3] << 24);
        }

        public override long ToInt64(byte[] buffer, int startIndex = 0)
        {
            return buffer[startIndex]
                | ((long)buffer[startIndex + 1] << 8)
                | ((long)buffer[startIndex + 2] << 16)
                | ((long)buffer[startIndex + 3] << 24)
                | ((long)buffer[startIndex + 4] << 32)
                | ((long)buffer[startIndex + 5] << 40)
                | ((long)buffer[startIndex + 6] << 48)
                | ((long)buffer[startIndex + 7] << 56);
        }

        public override float ToSingle(byte[] buffer, int startIndex = 0)
        {
            byte[] floatBytes = new byte[4];
            Array.Copy(buffer, startIndex, floatBytes, 0, 4);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(floatBytes);
            return BitConverter.ToSingle(floatBytes, 0);
        }

        public override ushort ToUInt16(byte[] buffer, int startIndex = 0)
        {
            return (ushort)(buffer[startIndex] | ((uint)buffer[startIndex + 1] << 8));
        }

        public override uint ToUInt32(byte[] buffer, int startIndex = 0)
        {
            return (uint)(
                buffer[startIndex]
                | (buffer[startIndex + 1] << 8)
                | (buffer[startIndex + 2] << 16)
                | (buffer[startIndex + 3] << 24)
            );
        }

        public override ulong ToUInt64(byte[] buffer, int startIndex = 0)
        {
            return (ulong)(
                buffer[startIndex]
                | ((long)buffer[startIndex + 1] << 8)
                | ((long)buffer[startIndex + 2] << 16)
                | ((long)buffer[startIndex + 3] << 24)
                | ((long)buffer[startIndex + 4] << 32)
                | ((long)buffer[startIndex + 5] << 40)
                | ((long)buffer[startIndex + 6] << 48)
                | ((long)buffer[startIndex + 7] << 56)
            );
        }
    }
}
