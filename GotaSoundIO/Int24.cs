using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;

namespace GotaSoundIO
{
    public struct Int24 : IReadable, IWriteable
    {
        public const int MaxValue = 8388607;
        public const int MinValue = -8388608;
        private byte[] Data;

        private void NullCheck()
        {
            if (Data == null)
            {
                Data = new byte[3];
            }
        }

        private int GetInt()
        {
            NullCheck();
            int ret = 0;
            ret |= Data[2];
            ret |= (Data[1] << 8);
            ret |= ((Data[0] & 0x7F) << 16);
            if ((Data[0] & 0x80) > 0)
            {
                ret = MinValue + ret;
            }
            return ret;
        }

        private static Int24 FromInt(int val)
        {
            Int24 ret = new Int24();
            ret.NullCheck();
            if (val > MaxValue)
            {
                val = MaxValue;
            }
            if (val < MinValue)
            {
                val = MinValue;
            }
            uint un = (uint)val;
            if (val < 0)
            {
                un = (uint)(val - MinValue);
            }
            ret.Data[2] = (byte)((un >> 0) & 0xFF);
            ret.Data[1] = (byte)((un >> 8) & 0xFF);
            ret.Data[0] = (byte)((un >> 16) & 0x7F);
            if (val < 0)
            {
                ret.Data[0] |= 0x80;
            }
            return ret;
        }

        public static implicit operator int(Int24 val) => val.GetInt();

        public static explicit operator Int24(int val) => Int24.FromInt(val);

        public static explicit operator Int24(uint val) => Int24.FromInt((int)val);

        public static explicit operator Int24(float val) => Int24.FromInt((int)val);

        public void Read(FileReader r)
        {
            NullCheck();
            switch (r.ByteOrder)
            {
                case ByteOrder.LittleEndian:
                case ByteOrder.System:
                    Data[2] = r.ReadByte();
                    Data[1] = r.ReadByte();
                    Data[0] = r.ReadByte();
                    break;
                case ByteOrder.BigEndian:
                    Data = r.ReadBytes(3);
                    break;
            }
        }

        public void Write(FileWriter w)
        {
            NullCheck();
            switch (w.ByteOrder)
            {
                case ByteOrder.LittleEndian:
                case ByteOrder.System:
                    w.Write(Data[2]);
                    w.Write(Data[1]);
                    w.Write(Data[0]);
                    break;
                case ByteOrder.BigEndian:
                    w.Write(Data);
                    break;
            }
        }
    }
}
