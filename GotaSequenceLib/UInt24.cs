using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GotaSequenceLib {
    public sealed class UInt24 : IReadable, IWriteable {
        public static uint MAX = 0xFFFFFF;
        public static uint MIN = 0;
        public uint Value {
            get => m_value;
            set {
                if (value <= MAX && value >= MIN) {
                    m_value = value;
                }
                else {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }
        private uint m_value;
        public UInt24() {}
        public UInt24(uint value) {
            Value = value;
        }
        #region Others
        public bool Equals(UInt24 other) {
            return m_value == other.m_value;
        }
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is UInt24 && Equals((UInt24)obj);
        }
        public override int GetHashCode() {
            return (int)m_value;
        }
        public override string ToString() {
            return m_value.ToString();
        }
        public void Read(FileReader r) {
            byte[] data = r.ReadBytes(3);
            if (r.ByteOrder == ByteOrder.BigEndian) {
                Value = (uint)((data[0] << 16) + (data[1] << 8) + data[2]);
            } else {
                Value = (uint)(data[0] + (data[1] << 8) + (data[2] << 16));
            }
        }
        public void Write(FileWriter w) {
            if (w.ByteOrder == ByteOrder.BigEndian) {
                w.Write((byte)((Value & 0xFF0000) >> 16));
                w.Write((byte)((Value & 0xFF00) >> 8));
                w.Write((byte)(Value & 0xFF));
            } else {
                w.Write((byte)(Value & 0xFF));
                w.Write((byte)((Value & 0xFF00) >> 8));
                w.Write((byte)((Value & 0xFF0000) >> 16));
            }
        }
        public static implicit operator uint(UInt24 val) {
            return val.m_value;
        }
        public static implicit operator UInt24(int val) {
            return (uint)val;
        }
        public static implicit operator UInt24(uint val) {
            return new UInt24(val & MAX);
        }
        public static UInt24 operator +(UInt24 left, UInt24 right) {
            uint val = left.Value + right.Value;
            if (val > MAX) {
                val -= MAX;
            }
            return val;
        }
        public static UInt24 operator -(UInt24 left, UInt24 right) {
            return left.Value - right.Value;
        }
        public static bool operator >(UInt24 left, UInt24 right) {
            return left.Value > right.Value;
        }
        public static bool operator <(UInt24 left, UInt24 right) {
            return left.Value < right.Value;
        }
        public static bool operator ==(UInt24 left, UInt24 right) {
            return left.Value == right.Value;
        }
        public static bool operator !=(UInt24 left, UInt24 right) {
            return left != right;
        }
        public static bool operator <=(UInt24 left, UInt24 right) {
            return left == right || left < right;
        }
        public static bool operator >=(UInt24 left, UInt24 right) {
            return left == right || left > right;
        }
        public static UInt24 operator ++(UInt24 val) {
            val.Value = val.Value + 1;
            return val;
        }
        public static UInt24 operator --(UInt24 val) {
            val.Value = val.Value - 1;
            return val;
        }
        #endregion
    }
}
