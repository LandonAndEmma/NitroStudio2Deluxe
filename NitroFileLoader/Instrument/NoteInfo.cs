using GotaSequenceLib;
using GotaSequenceLib.Playback;
using GotaSoundIO.IO;

namespace NitroFileLoader.Instrument
{
    public class NoteInfo : IReadable, IWriteable
    {
        public Notes Key;
        public InstrumentType InstrumentType = InstrumentType.PCM;
        public ushort WaveId;
        public ushort WarId;
        public byte BaseNote = 60;
        public byte Attack = 127;
        public byte Decay = 127;
        public byte Sustain = 127;
        public byte Release = 127;
        public byte Pan = 0x40;

        public void Read(FileReader r)
        {
            WaveId = r.ReadUInt16();
            WarId = r.ReadUInt16();
            BaseNote = r.ReadByte();
            Attack = r.ReadByte();
            Decay = r.ReadByte();
            Sustain = r.ReadByte();
            Release = r.ReadByte();
            Pan = r.ReadByte();
        }

        public void Write(FileWriter w)
        {
            w.Write(WaveId);
            w.Write(WarId);
            w.Write(BaseNote);
            w.Write(Attack);
            w.Write(Decay);
            w.Write(Sustain);
            w.Write(Release);
            w.Write(Pan);
        }

        public NotePlayBackInfo ToNotePlayBackInfo()
        {
            return new NotePlayBackInfo()
            {
                Attack = Attack,
                Decay = Decay,
                InstrumentType = TrueType(),
                BaseKey = BaseNote,
                Pan = Pan,
                Release = Release,
                Sustain = Sustain,
                WarId = WarId,
                WaveId = WaveId,
            };
        }

        public GotaSequenceLib.Playback.InstrumentType TrueType()
        {
            return InstrumentType switch
            {
                InstrumentType.PSG => GotaSequenceLib.Playback.InstrumentType.PSG,
                InstrumentType.Noise => GotaSequenceLib.Playback.InstrumentType.Noise,
                _ => GotaSequenceLib.Playback.InstrumentType.PCM,
            };
        }

        public override bool Equals(object obj)
        {
            return obj is NoteInfo n && n.Attack == Attack
                && n.BaseNote == BaseNote
                && n.Decay == Decay
                && n.InstrumentType == InstrumentType
                && n.Pan == Pan
                && n.Release == Release
                && n.Sustain == Sustain
                && n.WarId == WarId
                && n.WaveId == WaveId;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + Attack.GetHashCode();
                hash = (hash * 31) + BaseNote.GetHashCode();
                hash = (hash * 31) + Decay.GetHashCode();
                hash = (hash * 31) + InstrumentType.GetHashCode();
                hash = (hash * 31) + Pan.GetHashCode();
                hash = (hash * 31) + Release.GetHashCode();
                hash = (hash * 31) + Sustain.GetHashCode();
                hash = (hash * 31) + WarId.GetHashCode();
                hash = (hash * 31) + WaveId.GetHashCode();
                return hash;
            }
        }

        public NoteInfo Duplicate()
        {
            return new NoteInfo()
            {
                Attack = Attack,
                BaseNote = BaseNote,
                Decay = Decay,
                InstrumentType = InstrumentType,
                Key = Key,
                Pan = Pan,
                Release = Release,
                Sustain = Sustain,
                WarId = WarId,
                WaveId = WaveId,
            };
        }
    }
}
