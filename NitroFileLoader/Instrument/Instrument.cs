using GotaSequenceLib;
using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NitroFileLoader.Instrument
{
    public abstract class Instrument : IReadable, IWriteable
    {
        public List<NoteInfo> NoteInfo = [];
        public int Index;
        public long GetOrder => Order + (Math.Max((int)Type() - 5, 0) * 100000000);
        public long Order;
        public abstract void Read(FileReader r);
        public abstract void Write(FileWriter w);
        public abstract InstrumentType Type();
        public abstract uint MaxInstruments();

        public NoteInfo GetNoteInfo(Notes note)
        {
            switch (Type())
            {
                case InstrumentType.PCM:
                case InstrumentType.PSG:
                case InstrumentType.Noise:
                    return NoteInfo[0];
                case InstrumentType.DrumSet:
                case InstrumentType.KeySplit:
                    if (
                        (
                            Type() == InstrumentType.DrumSet
                            && (byte)note < (this as DrumSetInstrument).Min
                        )
                        || (byte)note
                            > NoteInfo.Select(x => (byte)x.Key).ElementAt(NoteInfo.Count - 1)
                    )
                    {
                        return null;
                    }
                    for (int i = 0; i < NoteInfo.Count; i++)
                    {
                        if ((byte)note <= (byte)NoteInfo[i].Key)
                        {
                            return NoteInfo[i];
                        }
                    }
                    return null;
            }
            return null;
        }
    }

    public enum InstrumentType : byte
    {
        Blank,
        PCM,
        PSG,
        Noise,
        DirectPCM,
        Null,
        DrumSet = 16,
        KeySplit = 17,
    }
}
