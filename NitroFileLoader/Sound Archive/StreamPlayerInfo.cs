using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace NitroFileLoader {
    public class StreamPlayerInfo : IReadable, IWriteable {
        public string Name;
        public int Index;
        public bool IsStereo;
        public byte LeftChannel;
        public byte RightChannel;
        public void Read(FileReader r) {
            IsStereo = r.ReadByte() > 1;
            LeftChannel = r.ReadByte();
            RightChannel = r.ReadByte();
            r.ReadBytes(21);
        }
        public void Write(FileWriter w) {
            w.Write((byte)(IsStereo ? 2 : 1));
            w.Write(LeftChannel);
            w.Write((byte)(IsStereo ? RightChannel : 0xFF));
            for (int i = 0; i < 0xE; i++) {
                w.Write((byte)0xFF);
            }
            w.Write(new byte[7]);
        }
    }
}
