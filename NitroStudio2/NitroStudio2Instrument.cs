using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;
using GotaSoundIO.Sound;
using NitroFileLoader;

namespace NitroStudio2
{
    public class NitroStudio2Instrument : IOFile
    {
        public Instrument Inst;
        public List<WaveEntry> Waves;

        public NitroStudio2Instrument() { }

        public NitroStudio2Instrument(
            Instrument inst,
            SoundArchive s,
            ushort war0,
            ushort war1,
            ushort war2,
            ushort war3
        )
        {
            Inst = inst;
            if (s == null)
            {
                return;
            }
            Waves = new List<WaveEntry>();
            foreach (var n in inst.NoteInfo)
            {
                WaveEntry w = new WaveEntry();
                w.WaveId = n.WaveId;
                if (n.InstrumentType != InstrumentType.PCM)
                {
                    continue;
                }
                switch (n.WarId)
                {
                    case 0:
                        n.WarId = w.WarId = war0;
                        break;
                    case 1:
                        n.WarId = w.WarId = war1;
                        break;
                    case 2:
                        n.WarId = w.WarId = war2;
                        break;
                    case 3:
                        n.WarId = w.WarId = war3;
                        break;
                }
                if (n.WarId != 0xFFFF)
                {
                    var war = s.WaveArchives.Where(x => x.Index == (int)n.WarId).FirstOrDefault();
                    if (war != null)
                    {
                        w.Wave = war.File.Waves[n.WaveId];
                    }
                }
                Waves.Add(w);
            }
        }

        public override void Read(FileReader r)
        {
            r.ReadUInt32();
            switch (r.ReadByte())
            {
                case 0:
                    Inst = new DirectInstrument();
                    break;
                case 1:
                    Inst = new DrumSetInstrument();
                    break;
                case 2:
                    Inst = new KeySplitInstrument();
                    break;
            }
            Inst.Read(r);
            if (Inst as DirectInstrument != null)
            {
                Inst.NoteInfo[0].InstrumentType = (InstrumentType)r.ReadByte();
            }
            Waves = null;
            if (!r.ReadBoolean())
            {
                return;
            }
            Waves = new List<WaveEntry>();
            uint numWaves = r.ReadUInt32();
            for (uint i = 0; i < numWaves; i++)
            {
                Waves.Add(new WaveEntry() { WaveId = r.ReadUInt16(), WarId = r.ReadUInt16() });
                if (r.ReadBoolean())
                {
                    Waves[Waves.Count - 1].Wave = new Wave();
                    Waves[Waves.Count - 1].Wave = (Wave)r.ReadFile<Wave>();
                }
            }
        }

        public void WriteInstrument(
            Bank bnk,
            int instrumentId,
            SoundArchive a,
            ushort war0,
            ushort war1,
            ushort war2,
            ushort war3
        )
        {
            var repl = bnk.Instruments.Where(x => x.Index == instrumentId).FirstOrDefault();
            Inst.Index = instrumentId;
            if (a == null)
            {
                return;
            }
            WaveArchiveInfo[] wars = new WaveArchiveInfo[4];
            if (war0 != 0xFFFF)
            {
                wars[0] = a.WaveArchives.Where(x => x.Index == (int)war0).FirstOrDefault();
            }
            if (war1 != 0xFFFF)
            {
                wars[1] = a.WaveArchives.Where(x => x.Index == (int)war1).FirstOrDefault();
            }
            if (war2 != 0xFFFF)
            {
                wars[2] = a.WaveArchives.Where(x => x.Index == (int)war2).FirstOrDefault();
            }
            if (war3 != 0xFFFF)
            {
                wars[3] = a.WaveArchives.Where(x => x.Index == (int)war3).FirstOrDefault();
            }
            if (wars.Where(x => x != null).Count() < 1)
            {
                return;
            }
            foreach (var r in Inst.NoteInfo)
            {
                if (r.InstrumentType != InstrumentType.PCM)
                {
                    continue;
                }
                if (Waves == null)
                {
                    continue;
                }
                var e = Waves
                    .Where(x => x.WarId == r.WarId && x.WaveId == r.WaveId)
                    .FirstOrDefault();
                if (e == null)
                {
                    continue;
                }
                if (e.Wave == null)
                {
                    continue;
                }
                string md5 = e.Wave.Md5Sum;
                bool found = false;
                for (int i = 0; i < wars.Length; i++)
                {
                    if (wars[i] != null)
                    {
                        for (int j = 0; j < wars[i].File.Waves.Count; j++)
                        {
                            if (!found && wars[i].File.Waves[j].Md5Sum == md5)
                            {
                                r.WaveId = (ushort)j;
                                r.WarId = (ushort)i;
                                found = true;
                            }
                        }
                    }
                }
                if (!found)
                {
                    RiffWave riff = new RiffWave();
                    riff.FromOtherStreamFile(e.Wave);
                    WaveMapper mapper = new WaveMapper(
                        new List<RiffWave>() { riff },
                        wars.Where(x => x != null).ToList(),
                        true
                    );
                    mapper.MinimizeBox = false;
                    mapper.ShowDialog();
                    if (mapper.WarMap == null)
                    {
                        return;
                    }
                    a.WaveArchives.Where(x => x.Index == mapper.WarMap[0])
                        .FirstOrDefault()
                        .File.Waves.Add(e.Wave);
                    r.WaveId = (ushort)(
                        a.WaveArchives.Where(x => x.Index == mapper.WarMap[0])
                            .FirstOrDefault()
                            .File.Waves.Count() - 1
                    );
                    r.WarId = (ushort)
                        wars.ToList()
                            .IndexOf(
                                a.WaveArchives.Where(x => x.Index == mapper.WarMap[0])
                                    .FirstOrDefault()
                            );
                }
            }
            bnk.Instruments[bnk.Instruments.IndexOf(repl)] = Inst;
        }

        public override void Write(FileWriter w)
        {
            w.Write("NS2I".ToCharArray());
            switch (Inst.Type())
            {
                case InstrumentType.DrumSet:
                    w.Write((byte)1);
                    break;
                case InstrumentType.KeySplit:
                    w.Write((byte)2);
                    break;
                default:
                    w.Write((byte)0);
                    break;
            }
            w.Write(Inst);
            if (Inst as DirectInstrument != null)
            {
                w.Write((byte)Inst.NoteInfo[0].InstrumentType);
            }
            w.Write(Waves != null);
            if (Waves == null)
            {
                return;
            }
            w.Write((uint)Waves.Count);
            foreach (var v in Waves)
            {
                w.Write(v.WaveId);
                w.Write(v.WarId);
                w.Write(v.Wave != null);
                if (v.Wave != null)
                {
                    w.WriteFile(v.Wave);
                }
            }
        }

        public class WaveEntry
        {
            public ushort WarId;
            public ushort WaveId;
            public Wave Wave;
        }
    }
}
