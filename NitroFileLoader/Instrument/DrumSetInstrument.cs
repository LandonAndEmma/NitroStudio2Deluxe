using GotaSequenceLib;
using GotaSoundIO.IO;
using System;
using System.Linq;

namespace NitroFileLoader.Instrument
{
    public class DrumSetInstrument : Instrument
    {
        public byte Min;

        public override void Read(FileReader r)
        {
            byte min = r.ReadByte();
            byte max = r.ReadByte();
            int numInsts = max - min + 1;
            Min = min;
            NoteInfo lastInst = null;
            byte ind = min;
            for (int i = 0; i < numInsts; i++)
            {
                _ = (InstrumentType)r.ReadUInt16();
                NoteInfo n = r.Read<NoteInfo>();
                if (lastInst == null)
                {
                    lastInst = n;
                }
                else
                {
                    if (!n.Equals(lastInst))
                    {
                        NoteInfo.Add(lastInst);
                        NoteInfo.Last().Key = (Notes)(ind - 1);
                        lastInst = n;
                    }
                }
                if (ind == max)
                {
                    NoteInfo.Add(n);
                    NoteInfo.Last().Key = (Notes)ind;
                }
                ind++;
            }
        }

        public override void Write(FileWriter w)
        {
            Notes[] indices = NoteInfo.Select(x => x.Key).ToArray();
            w.Write(Min);
            w.Write((byte)indices.Last());
            for (int i = Min; i <= (byte)indices.Last(); i++)
            {
                int ind = 0;
                for (int j = indices.Count() - 1; j >= 0; j--)
                {
                    if (i <= (byte)indices[j])
                    {
                        ind = j;
                    }
                }
                w.Write(
                    (ushort)
                        NoteInfo.Where(x => x.Key == indices[ind]).FirstOrDefault().InstrumentType
                );
                w.Write(NoteInfo.Where(x => x.Key == indices[ind]).FirstOrDefault());
            }
        }

        public override InstrumentType Type()
        {
            return InstrumentType.DrumSet;
        }

        public override uint MaxInstruments()
        {
            return 0x80;
        }
    }
}
