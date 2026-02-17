using GotaSoundIO.IO;
using System;

namespace GotaSequenceLib
{
    public sealed class UInt24 : IReadable, IWriteable
    {
        public static uint MAX = 0xFFFFFF;
        public static uint MIN = 0;
        public uint Value
        {
            get; set => field = value <= MAX && value >= MIN ? value : throw new ArgumentOutOfRangeException();
        }

        public UInt24() { }

        public UInt24(uint value)
        {
            Value = value;
        }

        #region Others
        public bool Equals(UInt24 other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is not null && obj is UInt24 && Equals((UInt24)obj);
        }

        public override int GetHashCode()
        {
            return (int)Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public void Read(FileReader r)
        {
            byte[] data = r.ReadBytes(3);
            Value = r.ByteOrder == ByteOrder.BigEndian
                ? (uint)((data[0] << 16) + (data[1] << 8) + data[2])
                : (uint)(data[0] + (data[1] << 8) + (data[2] << 16));
        }

        public void Write(FileWriter w)
        {
            if (w.ByteOrder == ByteOrder.BigEndian)
            {
                w.Write((byte)((Value & 0xFF0000) >> 16));
                w.Write((byte)((Value & 0xFF00) >> 8));
                w.Write((byte)(Value & 0xFF));
            }
            else
            {
                w.Write((byte)(Value & 0xFF));
                w.Write((byte)((Value & 0xFF00) >> 8));
                w.Write((byte)((Value & 0xFF0000) >> 16));
            }
        }

        public static implicit operator uint(UInt24 val)
        {
            return val.Value;
        }

        public static implicit operator UInt24(int val)
        {
            return (uint)val;
        }

        public static implicit operator UInt24(uint val)
        {
            return new UInt24(val & MAX);
        }

        public static UInt24 operator +(UInt24 left, UInt24 right)
        {
            uint val = left.Value + right.Value;
            if (val > MAX)
            {
                val -= MAX;
            }
            return val;
        }

        public static UInt24 operator -(UInt24 left, UInt24 right)
        {
            return left.Value - right.Value;
        }

        public static bool operator >(UInt24 left, UInt24 right)
        {
            return left.Value > right.Value;
        }

        public static bool operator <(UInt24 left, UInt24 right)
        {
            return left.Value < right.Value;
        }

        public static bool operator ==(UInt24 left, UInt24 right)
        {
            return left.Value == right.Value;
        }

        public static bool operator !=(UInt24 left, UInt24 right)
        {
            return left != right;
        }

        public static bool operator <=(UInt24 left, UInt24 right)
        {
            return left == right || left < right;
        }

        public static bool operator >=(UInt24 left, UInt24 right)
        {
            return left == right || left > right;
        }

        public static UInt24 operator ++(UInt24 val)
        {
            val.Value++;
            return val;
        }

        public static UInt24 operator --(UInt24 val)
        {
            val.Value--;
            return val;
        }
        #endregion
    }
}
