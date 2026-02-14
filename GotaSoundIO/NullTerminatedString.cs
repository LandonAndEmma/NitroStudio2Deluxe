using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;

namespace GotaSoundIO
{
    public class NullTerminatedString : IReadable, IWriteable, IComparable<NullTerminatedString>
    {
        public string Data;
        public byte[] ByteData => Encoding.UTF8.GetBytes(Data);

        public NullTerminatedString() { }

        public NullTerminatedString(string str)
        {
            Data = str;
        }

        public void Read(FileReader r)
        {
            Data = r.ReadNullTerminated();
        }

        public void Write(FileWriter w)
        {
            w.WriteNullTerminated(Data);
        }

        public static bool[] GetBits(byte[] byteData)
        {
            bool[] bits = new bool[byteData.Length * 8];
            for (int i = 0; i < byteData.Length; i++)
            {
                bits[i * 8 + 7] = (byteData[i] & 0b1) > 0;
                bits[i * 8 + 6] = (byteData[i] & 0b10) > 0;
                bits[i * 8 + 5] = (byteData[i] & 0b100) > 0;
                bits[i * 8 + 4] = (byteData[i] & 0b1000) > 0;
                bits[i * 8 + 3] = (byteData[i] & 0b10000) > 0;
                bits[i * 8 + 2] = (byteData[i] & 0b100000) > 0;
                bits[i * 8 + 1] = (byteData[i] & 0b1000000) > 0;
                bits[i * 8 + 0] = (byteData[i] & 0b10000000) > 0;
            }
            return bits;
        }

        public int CompareTo(NullTerminatedString other)
        {
            var bits = GetBits(ByteData);
            var bitsOther = GetBits(other.ByteData);
            for (int i = 0; i < bits.Length; i++)
            {
                if (i >= bitsOther.Length)
                {
                    return 1;
                }
                if (bits[i] && !bitsOther[i])
                {
                    return 1;
                }
                else if (!bits[i] && bitsOther[i])
                {
                    return -1;
                }
            }
            if (bits.Length == bitsOther.Length)
            {
                return 0;
            }
            return -1;
        }
    }
}
