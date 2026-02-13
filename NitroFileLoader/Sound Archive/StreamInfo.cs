using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace NitroFileLoader {
    public class StreamInfo : IReadable, IWriteable {
        public string Name;
        public int Index;
        public bool ForceIndividualFile;
        public Stream File;
        public StreamPlayerInfo Player;
        public byte Volume = 100;
        public byte Priority = 0x40;
        public uint ReadingFileId;
        public byte ReadingPlayerId;
        public bool MonoToStereo;
        public void Read(FileReader r) {
            ReadingFileId = r.ReadUInt32();
            MonoToStereo = (ReadingFileId & 0xFF000000) > 0;
            ReadingFileId &= 0xFFFFFF;
            Volume = r.ReadByte();
            Priority = r.ReadByte();
            ReadingPlayerId = r.ReadByte();
            r.ReadBytes(5);
        }
        public void Write(FileWriter w) {
            w.Write((uint)((uint)ReadingFileId | (MonoToStereo ? 0x01000000U : 0)));
            w.Write(Volume);
            w.Write(Priority);
            w.Write((byte)(Player != null ? Player.Index : ReadingPlayerId));
            w.Write(new byte[5]);
        }
    }
}
