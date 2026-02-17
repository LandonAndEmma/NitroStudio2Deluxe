using GotaSoundIO.IO;

namespace GotaSoundBank.SF2
{
    public class Generator : IReadable, IWriteable
    {
        public SF2Generators Gen;
        public SF2GeneratorAmount Amount;

        public void Read(FileReader r)
        {
            Gen = (SF2Generators)r.ReadUInt16();
            Amount = new SF2GeneratorAmount
            {
                Amount = r.ReadInt16()
            };
        }

        public void Write(FileWriter w)
        {
            w.Write((ushort)Gen);
            w.Write(Amount.Amount);
        }
    }
}
