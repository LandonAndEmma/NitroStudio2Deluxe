using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace NitroFileLoader {
    public class GroupEntry : IReadable, IWriteable {
        public object Entry;
        public GroupEntryType Type;
        public uint ReadingId;
        public bool LoadSequence;
        public bool LoadSequenceArchive;
        public bool LoadBank;
        public bool LoadWaveArchive;
        public void LoadFlags(byte flags) {
            LoadSequence = (flags & 0b1) > 0;
            LoadBank = (flags & 0b10) > 0;
            LoadWaveArchive = (flags & 0b100) > 0;
            LoadSequenceArchive = (flags & 0b1000) > 0;
        }
        public byte SaveFlags() {
            byte flags = 0;
            if (LoadSequence) { flags |= 0b1; }
            if (LoadBank) { flags |= 0b10; }
            if (LoadWaveArchive) { flags |= 0b100; }
            if (LoadSequenceArchive) { flags |= 0b1000; }
            return flags;
        }
        public void Read(FileReader r) {
            Type = (GroupEntryType)r.ReadByte();
            LoadFlags(r.ReadByte());
            r.ReadUInt16();
            ReadingId = r.ReadUInt32();
        }
        public void Write(FileWriter w) {
            w.Write((byte)Type);
            w.Write(SaveFlags());
            w.Write((ushort)0);
            switch (Type) {
                case GroupEntryType.Sequence:
                    w.Write((uint)(Entry as SequenceInfo).Index);
                    break;
                case GroupEntryType.Bank:
                    w.Write((uint)(Entry as BankInfo).Index);
                    break;
                case GroupEntryType.WaveArchive:
                    w.Write((uint)(Entry as WaveArchiveInfo).Index);
                    break;
                case GroupEntryType.SequenceArchive:
                    w.Write((uint)(Entry as SequenceArchiveInfo).Index);
                    break;
            }
        }
    }
    public enum GroupEntryType : byte {
        Sequence, Bank, WaveArchive, SequenceArchive
    }
}
