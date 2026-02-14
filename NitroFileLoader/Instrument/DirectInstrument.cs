using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;

namespace NitroFileLoader
{
    public class DirectInstrument : Instrument
    {
        public override InstrumentType Type() => NoteInfo[0].InstrumentType;

        public override uint MaxInstruments() => 1;

        public override void Read(FileReader r)
        {
            NoteInfo.Add(r.Read<NoteInfo>());
            NoteInfo.Last().Key = GotaSequenceLib.Notes.gn9;
        }

        public override void Write(FileWriter w)
        {
            w.Write(NoteInfo[0]);
        }
    }
}
