using GotaSoundIO.IO;
using System.Linq;

namespace NitroFileLoader.Instrument
{
    public class DirectInstrument : Instrument
    {
        public override InstrumentType Type()
        {
            return NoteInfo[0].InstrumentType;
        }

        public override uint MaxInstruments()
        {
            return 1;
        }

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
