using GotaSoundIO.IO;

namespace GotaSoundIO
{
    public class FlagParameters : IReadable, IWriteable
    {
        private readonly uint?[] Parameters = new uint?[32];
        public uint? this[int bit]
        {
            get => Parameters[bit]; set => Parameters[bit] = value;
        }

        public void Read(FileReader r)
        {
            uint mask = r.ReadUInt32();
            for (int i = 0; i < 32; i++)
            {
                Parameters[i] = (mask & (0b1 << i)) > 0 ? r.ReadUInt32() : null;
            }
        }

        public void Write(FileWriter w)
        {
            uint mask = 0;
            for (int i = 0; i < 32; i++)
            {
                if (Parameters[i] != null)
                {
                    mask |= (uint)(0b1 << i);
                }
            }
            w.Write(mask);
            foreach (uint? p in Parameters)
            {
                if (p != null)
                {
                    w.Write(p.Value);
                }
            }
        }
    }
}
