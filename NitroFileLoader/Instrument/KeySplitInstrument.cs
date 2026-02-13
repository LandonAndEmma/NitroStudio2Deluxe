using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSequenceLib;
using GotaSoundIO.IO;
namespace NitroFileLoader {
    public class KeySplitInstrument : Instrument {
        public override void Read(FileReader r) {
            List<byte> indices = new List<byte>();
            for (int i = 0; i < 8; i++) {
                byte b = r.ReadByte();
                if (b != 0) {
                    indices.Add(b);
                }
            }
            for (int i = 0; i < indices.Count; i++) {
                InstrumentType t = (InstrumentType)r.ReadUInt16();
                NoteInfo.Add(r.Read<NoteInfo>());
                NoteInfo.Last().Key = (Notes)indices[i];
                NoteInfo.Last().InstrumentType = t;
            }
        }      
        public override void Write(FileWriter w) {
            var indices = NoteInfo.Select(x => (byte)x.Key).ToArray();
            w.Write(indices);
            w.Write(new byte[8 - indices.Length]);
            foreach (var v in NoteInfo) {
                w.Write((ushort)v.InstrumentType);
                w.Write(v);
            }
        }
        public override InstrumentType Type() => InstrumentType.KeySplit;
        public override uint MaxInstruments() => 8;
    }
}
