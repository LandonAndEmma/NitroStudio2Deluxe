using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;

namespace GotaSoundIO
{
    public struct UInt24 : IReadable, IWriteable
    {
        public const int MaxValue = 16777215;
        public const int MinValue = 0;
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
            ret |= (Data[0] << 16);
            return ret;
        }

        private static UInt24 FromInt(int val)
        {
            UInt24 ret = new UInt24();
            ret.NullCheck();
            ret.Data[2] = (byte)((val >> 0) & 0xFF);
            ret.Data[1] = (byte)((val >> 8) & 0xFF);
            ret.Data[0] = (byte)((val >> 16) & 0xFF);
            return ret;
        }

        private uint GetUInt()
        {
            return (uint)GetInt();
        }

        public static implicit operator int(UInt24 val) => val.GetInt();

        public static implicit operator uint(UInt24 val) => val.GetUInt();

        public static explicit operator UInt24(int val) => UInt24.FromInt(val);

        public static explicit operator UInt24(uint val) => UInt24.FromInt((int)val);

        public static explicit operator UInt24(float val) => UInt24.FromInt((int)val);

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
