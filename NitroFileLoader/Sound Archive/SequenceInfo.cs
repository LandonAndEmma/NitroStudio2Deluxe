using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace NitroFileLoader {
    public class SequenceInfo : IReadable, IWriteable {
        public string Name;
        public int Index;
        public bool ForceIndividualFile;
        public Sequence File;
        public BankInfo Bank;
        public PlayerInfo Player;
        public byte Volume = 100;
        public byte ChannelPriority = 0x40;
        public byte PlayerPriority = 0x40;
        public uint ReadingFileId;
        public ushort ReadingBankId;
        public byte ReadingPlayerId;
        public void Read(FileReader r) {
            ReadingFileId = r.ReadUInt32();
            ReadingBankId = r.ReadUInt16();
            Volume = r.ReadByte();
            ChannelPriority = r.ReadByte();
            PlayerPriority = r.ReadByte();
            ReadingPlayerId = r.ReadByte();
            r.ReadUInt16();
        }
        public void Write(FileWriter w) {
            w.Write(ReadingFileId);
            w.Write((ushort)(Bank != null ? Bank.Index : ReadingBankId));
            w.Write(Volume);
            w.Write(ChannelPriority);
            w.Write(PlayerPriority);
            w.Write((byte)(Player != null ? Player.Index : ReadingPlayerId));
            w.Write((ushort)0);
        }
    }
}
