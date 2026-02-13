using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace NitroFileLoader {
    public class SDATHeader : FileHeader {
        public override void Read(FileReader r) {
            Magic = new string(r.ReadChars(4));
            r.ByteOrder = ByteOrder.BigEndian;
            r.ByteOrder = ByteOrder = r.ReadUInt16() == 0xFEFF ?ByteOrder.BigEndian : ByteOrder.LittleEndian;
            r.ReadUInt16(); 
            FileSize = r.ReadUInt32();
            HeaderSize = r.ReadUInt16();
            ushort numBlocks = r.ReadUInt16();
            BlockOffsets = new long[numBlocks];
            BlockSizes = new long[numBlocks];
            if (numBlocks == 3) { r.ReadUInt64(); }
            for (int i = 0; i < numBlocks; i++) {
                BlockOffsets[i] = r.ReadUInt32();
                BlockSizes[i] = r.ReadUInt32();
            }
            r.Align(0x20);
        }
        public override void Write(FileWriter w) {
            w.ByteOrder = ByteOrder.LittleEndian;
            w.Write(Magic.ToCharArray());
            w.Write((ushort)0xFEFF);
            w.Write((ushort)0x0100);
            w.Write((uint)FileSize);
            w.Write((ushort)HeaderSize);
            w.Write((ushort)BlockOffsets.Length);
            if (BlockOffsets.Length == 3) { w.Write((ulong)0); }
            for (int i = 0; i < BlockOffsets.Length; i++) {
                w.Write((uint)BlockOffsets[i]);
                w.Write((uint)BlockSizes[i]);
            }
            w.Align(0x20);
        }
    }
    public class NHeader : FileHeader {
        public override void Read(FileReader r) {
            Magic = new string(r.ReadChars(4));    
            r.ByteOrder = ByteOrder.BigEndian;
            r.ByteOrder = ByteOrder = r.ReadUInt16() == 0xFEFF ? ByteOrder.BigEndian : ByteOrder.LittleEndian;
            r.ReadUInt16(); 
            FileSize = r.ReadUInt32();
            HeaderSize = r.ReadUInt16();
            r.ReadUInt16();
            BlockOffsets = new long[] { 0x10 };
        }
        public override void Write(FileWriter w) {
            HeaderSize = 0x10;
            w.ByteOrder = ByteOrder.LittleEndian;
            w.Write(Magic.ToCharArray());
            w.Write((ushort)0xFEFF);
            w.Write((ushort)0x0100);
            w.Write((uint)FileSize);
            w.Write((ushort)HeaderSize);
            w.Write((ushort)1);
        }
    }
}
