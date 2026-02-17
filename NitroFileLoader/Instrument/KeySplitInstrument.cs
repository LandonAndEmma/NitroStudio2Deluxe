using GotaSequenceLib;
using GotaSoundIO.IO;
using System.Collections.Generic;
using System.Linq;

namespace NitroFileLoader.Instrument
{
    public class KeySplitInstrument : Instrument
    {
        public override void Read(FileReader r)
        {
            List<byte> indices = [];
            for (int i = 0; i < 8; i++)
            {
                byte b = r.ReadByte();
                if (b != 0)
                {
                    indices.Add(b);
                }
            }
            for (int i = 0; i < indices.Count; i++)
            {
                InstrumentType t = (InstrumentType)r.ReadUInt16();
                NoteInfo.Add(r.Read<NoteInfo>());
                NoteInfo.Last().Key = (Notes)indices[i];
                NoteInfo.Last().InstrumentType = t;
            }
        }

        public override void Write(FileWriter w)
        {
            byte[] indices = NoteInfo.Select(x => (byte)x.Key).ToArray();
            w.Write(indices);
            w.Write(new byte[8 - indices.Length]);
            foreach (NoteInfo v in NoteInfo)
            {
                w.Write((ushort)v.InstrumentType);
                w.Write(v);
            }
        }

        public override InstrumentType Type()
        {
            return InstrumentType.KeySplit;
        }

        public override uint MaxInstruments()
        {
            return 8;
        }
    }
}
