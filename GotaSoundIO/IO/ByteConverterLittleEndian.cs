using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
namespace GotaSoundIO.IO {
    [SecuritySafeCritical]
    public sealed class ByteConverterLittleEndian : ByteConverter {
        public override ByteOrder ByteOrder {
            get {
                return ByteOrder.LittleEndian;
            }
        }
        [SecuritySafeCritical]
        public override unsafe void GetBytes(double value, byte[] buffer, int startIndex = 0) {
            ulong num = (ulong)*(long*)&value;
            buffer[startIndex] = (byte)num;
            buffer[startIndex + 1] = (byte)(num >> 8);
            buffer[startIndex + 2] = (byte)(num >> 16);
            buffer[startIndex + 3] = (byte)(num >> 24);
            buffer[startIndex + 4] = (byte)(num >> 32);
            buffer[startIndex + 5] = (byte)(num >> 40);
            buffer[startIndex + 6] = (byte)(num >> 48);
            buffer[startIndex + 7] = (byte)(num >> 56);
        }
        public override void GetBytes(short value, byte[] buffer, int startIndex = 0) {
            buffer[startIndex] = (byte)value;
            buffer[startIndex + 1] = (byte)((uint)value >> 8);
        }
        public override void GetBytes(int value, byte[] buffer, int startIndex = 0) {
            buffer[startIndex] = (byte)value;
            buffer[startIndex + 1] = (byte)(value >> 8);
            buffer[startIndex + 2] = (byte)(value >> 16);
            buffer[startIndex + 3] = (byte)(value >> 24);
        }
        public override void GetBytes(long value, byte[] buffer, int startIndex = 0) {
            buffer[startIndex] = (byte)value;
            buffer[startIndex + 1] = (byte)(value >> 8);
            buffer[startIndex + 2] = (byte)(value >> 16);
            buffer[startIndex + 3] = (byte)(value >> 24);
            buffer[startIndex + 4] = (byte)(value >> 32);
            buffer[startIndex + 5] = (byte)(value >> 40);
            buffer[startIndex + 6] = (byte)(value >> 48);
            buffer[startIndex + 7] = (byte)(value >> 56);
        }
        [SecuritySafeCritical]
        public override unsafe void GetBytes(float value, byte[] buffer, int startIndex = 0) {
            uint num = *(uint*)&value;
            buffer[startIndex] = (byte)num;
            buffer[startIndex + 1] = (byte)(num >> 8);
            buffer[startIndex + 2] = (byte)(num >> 16);
            buffer[startIndex + 3] = (byte)(num >> 24);
        }
        public override void GetBytes(ushort value, byte[] buffer, int startIndex = 0) {
            buffer[startIndex] = (byte)value;
            buffer[startIndex + 1] = (byte)((uint)value >> 8);
        }
        public override void GetBytes(uint value, byte[] buffer, int startIndex = 0) {
            buffer[startIndex] = (byte)value;
            buffer[startIndex + 1] = (byte)(value >> 8);
            buffer[startIndex + 2] = (byte)(value >> 16);
            buffer[startIndex + 3] = (byte)(value >> 24);
        }
        public override void GetBytes(ulong value, byte[] buffer, int startIndex = 0) {
            buffer[startIndex] = (byte)value;
            buffer[startIndex + 1] = (byte)(value >> 8);
            buffer[startIndex + 2] = (byte)(value >> 16);
            buffer[startIndex + 3] = (byte)(value >> 24);
            buffer[startIndex + 4] = (byte)(value >> 32);
            buffer[startIndex + 5] = (byte)(value >> 40);
            buffer[startIndex + 6] = (byte)(value >> 48);
            buffer[startIndex + 7] = (byte)(value >> 56);
        }
        [SecuritySafeCritical]
        public override unsafe double ToDouble(byte[] buffer, int startIndex = 0) {
            long i = ((long)buffer[startIndex] | (long)buffer[startIndex + 1] << 8 | (long)buffer[startIndex + 2] << 16 | (long)buffer[startIndex + 3] << 24 | (long)buffer[startIndex + 4] << 32 | (long)buffer[startIndex + 5] << 40 | (long)buffer[startIndex + 6] << 48 | (long)buffer[startIndex + 7] << 56);
            return *(double*)&i;
        }
        public override short ToInt16(byte[] buffer, int startIndex = 0) {
            return (short)((int)buffer[startIndex] | (int)buffer[startIndex + 1] << 8);
        }
        public override int ToInt32(byte[] buffer, int startIndex = 0) {
            return (int)buffer[startIndex] | (int)buffer[startIndex + 1] << 8 | (int)buffer[startIndex + 2] << 16 | (int)buffer[startIndex + 3] << 24;
        }
        public override long ToInt64(byte[] buffer, int startIndex = 0) {
            return (long)buffer[startIndex] | (long)buffer[startIndex + 1] << 8 | (long)buffer[startIndex + 2] << 16 | (long)buffer[startIndex + 3] << 24 | (long)buffer[startIndex + 4] << 32 | (long)buffer[startIndex + 5] << 40 | (long)buffer[startIndex + 6] << 48 | (long)buffer[startIndex + 7] << 56;
        }
        [SecuritySafeCritical]
        public override unsafe float ToSingle(byte[] buffer, int startIndex = 0) {
            int i = ((int)buffer[startIndex] | (int)buffer[startIndex + 1] << 8 | (int)buffer[startIndex + 2] << 16 | (int)buffer[startIndex + 3] << 24);
            return *(float*)&i;
        }
        public override ushort ToUInt16(byte[] buffer, int startIndex = 0) {
            return (ushort)((uint)buffer[startIndex] | (uint)buffer[startIndex + 1] << 8);
        }
        public override uint ToUInt32(byte[] buffer, int startIndex = 0) {
            return (uint)((int)buffer[startIndex] | (int)buffer[startIndex + 1] << 8 | (int)buffer[startIndex + 2] << 16 | (int)buffer[startIndex + 3] << 24);
        }
        public override ulong ToUInt64(byte[] buffer, int startIndex = 0) {
            return (ulong)((long)buffer[startIndex] | (long)buffer[startIndex + 1] << 8 | (long)buffer[startIndex + 2] << 16 | (long)buffer[startIndex + 3] << 24 | (long)buffer[startIndex + 4] << 32 | (long)buffer[startIndex + 5] << 40 | (long)buffer[startIndex + 6] << 48 | (long)buffer[startIndex + 7] << 56);
        }
    }
}
