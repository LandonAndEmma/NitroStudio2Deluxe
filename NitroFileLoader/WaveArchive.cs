using GotaSoundIO;
using GotaSoundIO.IO;
using GotaSoundIO.Sound.Formats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NitroFileLoader
{
    public class WaveArchive : IOFile
    {
        public List<Wave> Waves = [];

        public override void Read(FileReader r)
        {
            r.OpenFile<NHeader>(out _);
            r.OpenBlock(0, out _, out _, false);
            _ = r.ReadUInt32();
            uint size = r.ReadUInt32();
            _ = r.ReadUInt32s(8);
            Table<uint> offs = r.Read<Table<uint>>();
            Waves = [];
            for (int i = 0; i < offs.Count; i++)
            {
                uint len = i == offs.Count - 1 ? size - (offs[i] - 0x10) : offs[i + 1] - offs[i];
                r.Jump(offs[i], true);
                Waves.Add(Wave.ReadShortened(r, len));
            }
        }

        public override void Write(FileWriter w)
        {
            w.InitFile<NHeader>("SWAR", ByteOrder.LittleEndian, null, 1);
            w.InitBlock("DATA");
            w.Write(new uint[8]);
            w.Write((uint)Waves.Count());
            long bak = w.Position;
            w.Write(new uint[Waves.Count()]);
            for (int i = 0; i < Waves.Count(); i++)
            {
                long bak2 = w.Position;
                w.Position = bak + (i * 4);
                w.Write((uint)(bak2 - w.FileOffset));
                w.Position = bak2;
                Waves[i].WriteShortened(w);
            }
            w.Pad(4);
            w.CloseBlock();
            w.CloseFile();
        }

        public RiffWave[] GetWaves()
        {
            RiffWave[] w = new RiffWave[Waves.Count];
            for (int i = 0; i < Waves.Count; i++)
            {
                w[i] = new RiffWave();
                w[i].FromOtherStreamFile(Waves[i]);
            }
            return w;
        }
    }
}
