using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;

namespace GotaSoundBank.SF2
{
    public class Modulator : IReadable, IWriteable
    {
        public SF2Modulators Source;
        public SF2Generators Destination;
        public short Amount;
        public SF2Modulators AmountSource;
        public SF2Transforms Transform;

        public void Read(FileReader r)
        {
            Source = (SF2Modulators)r.ReadUInt16();
            Destination = (SF2Generators)r.ReadUInt16();
            Amount = r.ReadInt16();
            AmountSource = (SF2Modulators)r.ReadUInt16();
            Transform = (SF2Transforms)r.ReadUInt16();
        }

        public void Write(FileWriter w)
        {
            w.Write((ushort)Source);
            w.Write((ushort)Destination);
            w.Write(Amount);
            w.Write((ushort)AmountSource);
            w.Write((ushort)Transform);
        }
    }
}
