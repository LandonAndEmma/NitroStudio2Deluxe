using GotaSoundIO.IO;
using System;
using System.Linq;

namespace NitroFileLoader
{
    public class PlayerInfo : IReadable, IWriteable
    {
        public string Name;
        public int Index;
        public ushort SequenceMax;
        public bool[] ChannelFlags = new bool[16];
        public uint HeapSize;

        public void Read(FileReader r)
        {
            SequenceMax = r.ReadUInt16();
            ChannelFlags = r.ReadBitFlags(2);
            if (ChannelFlags.Where(x => x == false).Count() == 16)
            {
                ChannelFlags = new bool[]
                {
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                };
            }
            HeapSize = r.ReadUInt32();
        }

        public void Write(FileWriter w)
        {
            w.Write(SequenceMax);
            if (ChannelFlags.Where(x => x == true).Count() == 16)
            {
                w.Write((ushort)0);
            }
            else
            {
                w.WriteBitFlags(ChannelFlags, 2);
            }
            w.Write(HeapSize);
        }

        public ushort BitFlags()
        {
            ushort u = 0;
            for (int i = 0; i < ChannelFlags.Length; i++)
            {
                if (ChannelFlags[i])
                {
                    u |= (ushort)(0b1 << i);
                }
            }
            return u;
        }
    }
}
