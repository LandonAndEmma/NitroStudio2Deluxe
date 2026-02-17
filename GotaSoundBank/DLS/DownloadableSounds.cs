using GotaSoundBank.SF2;
using GotaSoundIO.IO;
using GotaSoundIO.IO.RIFF;
using GotaSoundIO.Sound.Formats;
using System.Collections.Generic;
using System.Linq;

namespace GotaSoundBank.DLS
{
    public class DownloadableSounds : IOFile
    {
        public List<Instrument> Instruments = [];
        public List<RiffWave> Waves = [];

        public DownloadableSounds() { }

        public DownloadableSounds(string filePath)
            : base(filePath) { }

        public DownloadableSounds(SoundFont sf2)
        {
            List<string> waveMd5s = [];
            Dictionary<int, string> newWaveIds = [];
            Dictionary<int, sbyte> tunings = [];
            Dictionary<int, byte> origPitches = [];
            Dictionary<int, uint> channelFlags = [];
            Waves = [];
            foreach (SF2.Instrument i in sf2.Instruments)
            {
                foreach (Zone z in i.GetAllZones())
                {
                    foreach (Generator g in z.Generators.Where(x => x.Gen == SF2Generators.SampleID))
                    {
                        SampleItem s = sf2.Samples[g.Amount.UAmount];
                        RiffWave wav = new()
                        {
                            Loops = s.Wave.Loops,
                            LoopStart = s.Wave.LoopStart,
                            LoopEnd = s.Wave.LoopEnd,
                            SampleRate = s.Wave.SampleRate
                        };
                        wav.Audio.Channels =
                            [];
                        switch (s.LinkType)
                        {
                            case SF2LinkTypes.Left:
                                wav.Audio.Channels.Add(s.Wave.Audio.Channels[0]);
                                try
                                {
                                    wav.Audio.Channels.Add(
                                        sf2
                                            .Samples.Where(x =>
                                                x.Link == s.Link && x.LinkType == SF2LinkTypes.Right
                                            )
                                            .FirstOrDefault()
                                            .Wave.Audio.Channels[0]
                                    );
                                }
                                catch { }
                                if (!channelFlags.ContainsKey(g.Amount.UAmount))
                                {
                                    channelFlags.Add(g.Amount.UAmount, 0b11);
                                }
                                break;
                            case SF2LinkTypes.Right:
                                try
                                {
                                    wav.Audio.Channels.Add(
                                        sf2
                                            .Samples.Where(x =>
                                                x.Link == s.Link && x.LinkType == SF2LinkTypes.Left
                                            )
                                            .FirstOrDefault()
                                            .Wave.Audio.Channels[0]
                                    );
                                }
                                catch { }
                                wav.Audio.Channels.Add(s.Wave.Audio.Channels[0]);
                                if (!channelFlags.ContainsKey(g.Amount.UAmount))
                                {
                                    channelFlags.Add(g.Amount.UAmount, 0b11);
                                }
                                break;
                            case SF2LinkTypes.Mono:
                                wav.Audio.Channels.Add(s.Wave.Audio.Channels[0]);
                                if (!channelFlags.ContainsKey(g.Amount.UAmount))
                                {
                                    channelFlags.Add(g.Amount.UAmount, 0b1);
                                }
                                break;
                            case SF2LinkTypes.Linked:
                                foreach (SampleItem w in sf2.Samples)
                                {
                                    if (w.LinkType == SF2LinkTypes.Linked && w.Link == s.Link)
                                    {
                                        wav.Audio.Channels.Add(w.Wave.Audio.Channels[0]);
                                        if (!channelFlags.ContainsKey(g.Amount.UAmount))
                                        {
                                            channelFlags.Add(g.Amount.UAmount, 0);
                                        }
                                        channelFlags[g.Amount.UAmount] <<= 1;
                                        channelFlags[g.Amount.UAmount] |= 0b1;
                                    }
                                }
                                break;
                        }
                        string md5 = wav.Md5Sum;
                        if (!newWaveIds.ContainsKey(g.Amount.UAmount))
                        {
                            newWaveIds.Add(g.Amount.UAmount, md5);
                            tunings.Add(
                                g.Amount.UAmount,
                                sf2.Samples[g.Amount.UAmount].PitchCorrection
                            );
                            origPitches.Add(
                                g.Amount.UAmount,
                                sf2.Samples[g.Amount.UAmount].OriginalPitch
                            );
                        }
                        if (!waveMd5s.Contains(md5))
                        {
                            waveMd5s.Add(md5);
                            Waves.Add(wav);
                        }
                    }
                }
            }
            Instruments = [];
            int instId = 0;
            foreach (SF2.Instrument inst in sf2.Instruments)
            {
                Instrument i = new()
                {
                    Name = inst.Name,
                    Regions = [],
                    BankId = 0
                };
                foreach (Preset p in sf2.Presets)
                {
                    foreach (Zone z in p.GetAllZones())
                    {
                        foreach (Generator g in z.Generators)
                        {
                            if (g.Gen == SF2Generators.Instrument && g.Amount.Amount == instId)
                            {
                                i.BankId = p.Bank;
                                i.InstrumentId = p.PresetNumber;
                            }
                        }
                    }
                }
                instId++;
                foreach (Zone z in inst.Zones)
                {
                    Region reg = GetInstrumentRegion(z, inst.GlobalZone);
                    if (reg != null)
                    {
                        i.Regions.Add(reg);
                    }
                }
                if (i.Regions.Count < 1)
                {
                    Region reg = GetInstrumentRegion(inst.GlobalZone, null);
                    if (reg != null)
                    {
                        i.Regions.Add(reg);
                    }
                }
                Instruments.Add(i);
            }
            Region GetInstrumentRegion(Zone z, Zone g)
            {
                if (z == null)
                {
                    return null;
                }
                Region r = new();
                int sampleNumRaw = SF2Value(SF2Generators.SampleID).UAmount;
                int sampleNum = waveMd5s.IndexOf(newWaveIds[sampleNumRaw]);
                r.WaveId = (uint)sampleNum;
                r.Tuning = (short)(tunings[sampleNumRaw] % 12 * 65536);
                r.RootNote = (byte)(origPitches[sampleNumRaw] + (tunings[sampleNumRaw] / 12));
                r.ChannelFlags = channelFlags[sampleNumRaw];
                r.Loops = Waves[sampleNum].Loops;
                r.LoopStart = Waves[sampleNum].LoopStart;
                r.LoopLength = Waves[sampleNum].LoopEnd - Waves[sampleNum].LoopStart;
                Articulator art = new()
                {
                    Connections = []
                };
                List<Connection> c = art.Connections;
                foreach (Generator gen in z.Generators)
                {
                    DestinationConnection d = DestinationConnection.Center;
                    switch (gen.Gen)
                    {
                        case SF2Generators.OverridingRootKey:
                            r.RootNote = (byte)(gen.Amount.Amount + (tunings[sampleNumRaw] / 12));
                            continue;
                        case SF2Generators.KeyRange:
                            r.NoteLow = gen.Amount.LowByte;
                            r.NoteHigh = gen.Amount.HighByte;
                            continue;
                        case SF2Generators.VelRange:
                            r.VelocityLow = gen.Amount.LowByte;
                            r.VelocityHigh = gen.Amount.HighByte;
                            continue;
                        case SF2Generators.ChorusEffectsSend:
                            d = DestinationConnection.Chorus;
                            break;
                        case SF2Generators.AttackVolEnv:
                            d = DestinationConnection.EG1AttackTime;
                            break;
                        case SF2Generators.DecayVolEnv:
                            d = DestinationConnection.EG1DecayTime;
                            break;
                        case SF2Generators.DelayVolEnv:
                            d = DestinationConnection.EG1DelayTime;
                            break;
                        case SF2Generators.HoldVolEnv:
                            d = DestinationConnection.EG1HoldTime;
                            break;
                        case SF2Generators.ReleaseVolEnv:
                            d = DestinationConnection.EG1ReleaseTime;
                            break;
                        case SF2Generators.SustainVolEnv:
                            d = DestinationConnection.EG1SustainLevel;
                            c.Add(
                                new Connection()
                                {
                                    DestinationConnection = d,
                                    Scale = (int)((1 - (gen.Amount.Amount / 1000d)) * 1000 * 65536),
                                }
                            );
                            continue;
                        case SF2Generators.Keynum:
                            d = DestinationConnection.KeyNumber;
                            break;
                        case SF2Generators.Pan:
                            d = DestinationConnection.Pan;
                            break;
                        case SF2Generators.FreqModLFO:
                            d = DestinationConnection.LFOFrequency;
                            break;
                        case SF2Generators.DelayModLFO:
                            d = DestinationConnection.LFOStartDelayTime;
                            break;
                        default:
                            continue;
                    }
                    c.Add(
                        new Connection()
                        {
                            DestinationConnection = d,
                            Scale = gen.Amount.Amount * 65536,
                        }
                    );
                }
                SF2GeneratorAmount SF2Value(SF2Generators gen)
                {
                    SF2GeneratorAmount ret = new();
                    if (g != null)
                    {
                        Generator a = g.Generators.Where(x => x.Gen == gen).FirstOrDefault();
                        if (a != null)
                        {
                            ret.Amount += a.Amount.Amount;
                        }
                    }
                    Generator b = z.Generators.Where(x => x.Gen == gen).FirstOrDefault();
                    if (b != null)
                    {
                        ret.Amount = b.Amount.Amount;
                    }
                    return ret;
                }
                r.Articulators = [art];
                return r;
            }
        }

        public override void Read(FileReader r2)
        {
            using RiffReader r = new(r2.BaseStream);
            r.OpenChunk(r.GetChunk("colh"));
            uint numInsts = r.ReadUInt32();
            Waves = [];
            foreach (Chunk c in (r.GetChunk("wvpl") as ListChunk).Chunks)
            {
                r.OpenChunk(c);
                r.BaseStream.Position -= 8;
                int len = r.ReadInt32() + 8;
                r.BaseStream.Position -= 8;
                RiffWave wav = new();
                wav.Read(r.ReadBytes(len));
                Waves.Add(wav);
            }
            foreach (ListChunk c in (r.GetChunk("lins") as ListChunk).Chunks)
            {
                r.OpenChunk(c);
                Instrument inst = new();
                r.OpenChunk(c.GetChunk("insh"));
                _ = r.ReadUInt32();
                inst.BankId = r.ReadUInt32();
                inst.InstrumentId = r.ReadUInt32();
                foreach (ListChunk g in (c.GetChunk("lrgn") as ListChunk).Chunks)
                {
                    Region reg = new();
                    r.OpenChunk(g.GetChunk("rgnh"));
                    reg.NoteLow = r.ReadUInt16();
                    reg.NoteHigh = r.ReadUInt16();
                    reg.VelocityLow = r.ReadUInt16();
                    reg.VelocityHigh = r.ReadUInt16();
                    reg.DoublePlayback = r.ReadUInt16() > 0;
                    reg.KeyGroup = (byte)r.ReadUInt16();
                    reg.Layer = r.ReadUInt16();
                    r.OpenChunk(g.GetChunk("wsmp"));
                    _ = r.ReadUInt32();
                    reg.RootNote = (byte)r.ReadUInt16();
                    reg.Tuning = r.ReadInt16();
                    reg.Gain = r.ReadInt32();
                    uint flags = r.ReadUInt32();
                    reg.NoTruncation = (flags & 0b1) > 0;
                    reg.NoCompression = (flags & 0b10) > 0;
                    reg.Loops = r.ReadUInt32() > 0;
                    if (reg.Loops)
                    {
                        _ = r.ReadUInt32();
                        reg.LoopAndRelease = r.ReadUInt32() > 0;
                        reg.LoopStart = r.ReadUInt32();
                        reg.LoopLength = r.ReadUInt32();
                    }
                    r.OpenChunk(g.GetChunk("wlnk"));
                    uint flg = r.ReadUInt16();
                    reg.PhaseMaster = (flg & 0b1) > 0;
                    reg.MultiChannel = (flg & 0b10) > 0;
                    reg.PhaseGroup = r.ReadUInt16();
                    reg.ChannelFlags = r.ReadUInt32();
                    reg.WaveId = r.ReadUInt32();
                    Waves[(int)reg.WaveId].Loops = reg.Loops;
                    if (reg.Loops)
                    {
                        Waves[(int)reg.WaveId].LoopStart = reg.LoopStart;
                        Waves[(int)reg.WaveId].LoopEnd =
                            reg.LoopLength == 0
                                ? (uint)Waves[(int)reg.WaveId].Audio.NumSamples
                                : reg.LoopStart + reg.LoopLength;
                    }
                    Chunk lar = g.GetChunk("lar2");
                    lar ??= g.GetChunk("lar1");
                    foreach (Chunk art in (g.GetChunk("lar2") as ListChunk).Chunks)
                    {
                        Articulator a = new();
                        r.OpenChunk(art);
                        _ = r.ReadUInt32();
                        uint numCons = r.ReadUInt32();
                        for (uint i = 0; i < numCons; i++)
                        {
                            Connection con = new()
                            {
                                SourceConnection = (SourceConnection)r.ReadUInt16(),
                                ControlConnection = r.ReadUInt16(),
                                DestinationConnection = (DestinationConnection)r.ReadUInt16(),
                                TransformConnection = (TransformConnection)r.ReadUInt16(),
                                Scale = r.ReadInt32()
                            };
                            a.Connections.Add(con);
                        }
                        reg.Articulators.Add(a);
                    }
                    inst.Regions.Add(reg);
                }
                Chunk info = c.GetChunk("INFO");
                if (info != null)
                {
                    Chunk inam = (info as ListChunk).GetChunk("INAM");
                    if (inam != null)
                    {
                        r.OpenChunk(inam);
                        r.BaseStream.Position -= 4;
                        uint siz = r.ReadUInt32();
                        inst.Name = new string(
                            r.ReadChars((int)siz).Where(x => x != 0).ToArray()
                        );
                    }
                }
                Instruments.Add(inst);
            }
        }

        public override void Write(FileWriter w2)
        {
            using RiffWriter w = new(w2.BaseStream);
            w.InitFile("DLS ");
            w.StartChunk("colh");
            w.Write((uint)Instruments.Count);
            w.EndChunk();
            w.StartListChunk("lins");
            foreach (Instrument inst in Instruments)
            {
                w.StartListChunk("ins ");
                w.StartChunk("insh");
                w.Write((uint)inst.Regions.Count);
                w.Write(inst.BankId);
                w.Write(inst.InstrumentId);
                w.EndChunk();
                w.StartListChunk("lrgn");
                foreach (Region r in inst.Regions)
                {
                    w.StartListChunk("rgn2");
                    w.StartChunk("rgnh");
                    w.Write(r.NoteLow);
                    w.Write(r.NoteHigh);
                    w.Write(r.VelocityLow);
                    w.Write(r.VelocityHigh);
                    w.Write((ushort)(r.DoublePlayback ? 1 : 0));
                    w.Write((ushort)r.KeyGroup);
                    w.Write(r.Layer);
                    w.EndChunk();
                    w.StartChunk("wsmp");
                    w.Write((uint)0x14);
                    w.Write((ushort)r.RootNote);
                    w.Write(r.Tuning);
                    w.Write(r.Gain);
                    uint flags = 0;
                    if (r.NoTruncation)
                    {
                        flags |= 0b1;
                    }
                    if (r.NoCompression)
                    {
                        flags |= 0b10;
                    }
                    w.Write(flags);
                    w.Write((uint)(r.Loops ? 1 : 0));
                    if (r.Loops)
                    {
                        w.Write((uint)0x10);
                        w.Write((uint)(r.LoopAndRelease ? 1 : 0));
                        w.Write(r.LoopStart);
                        w.Write(r.LoopLength);
                    }
                    w.EndChunk();
                    w.StartChunk("wlnk");
                    ushort flg = 0;
                    if (r.PhaseMaster)
                    {
                        flg |= 0b1;
                    }
                    if (r.MultiChannel)
                    {
                        flg |= 0b10;
                    }
                    w.Write(flg);
                    w.Write(r.PhaseGroup);
                    w.Write(r.ChannelFlags);
                    w.Write(r.WaveId);
                    w.EndChunk();
                    w.StartListChunk("lar2");
                    foreach (Articulator a in r.Articulators)
                    {
                        w.StartChunk("art2");
                        w.Write((uint)8);
                        w.Write((uint)a.Connections.Count);
                        foreach (Connection c in a.Connections)
                        {
                            w.Write((ushort)c.SourceConnection);
                            w.Write(c.ControlConnection);
                            w.Write((ushort)c.DestinationConnection);
                            w.Write((ushort)c.TransformConnection);
                            w.Write(c.Scale);
                        }
                        w.EndChunk();
                    }
                    w.EndChunk();
                    w.EndChunk();
                }
                w.EndChunk();
                if (inst.Name != "")
                {
                    w.StartListChunk("INFO");
                    w.StartChunk("INAM");
                    w.Write(inst.Name.ToCharArray());
                    w.Write("\0".ToCharArray());
                    int len = inst.Name.Length + 1;
                    while (len % 4 != 0)
                    {
                        w.Write((byte)0);
                        len++;
                    }
                    w.EndChunk();
                    w.EndChunk();
                }
                w.EndChunk();
            }
            w.EndChunk();
            w.StartChunk("ptbl");
            w.Write((uint)8);
            w.Write((uint)Waves.Count);
            long ptblStart = w.BaseStream.Position;
            w.Write(new byte[Waves.Count * 4]);
            w.EndChunk();
            w.StartListChunk("wvpl");
            long waveTableStart = w.BaseStream.Position;
            int waveNum = 0;
            foreach (RiffWave wav in Waves)
            {
                long wBak = w.BaseStream.Position;
                w.BaseStream.Position = ptblStart + (waveNum++ * 4);
                w.Write((uint)(wBak - waveTableStart));
                w.BaseStream.Position = wBak;
                w.WriteWave(wav);
            }
            w.EndChunk();
            w.StartListChunk("INFO");
            w.StartChunk("INAM");
            w.Write("Instrument Set".ToCharArray());
            w.EndChunk();
            w.EndChunk();
            w.CloseFile();
        }

        public void AssignLoops()
        {
            foreach (Instrument i in Instruments)
            {
                foreach (Region reg in i.Regions)
                {
                    if (reg.Loops)
                    {
                        Waves[(int)reg.WaveId].Loops = reg.Loops;
                        Waves[(int)reg.WaveId].LoopStart = reg.LoopStart;
                        Waves[(int)reg.WaveId].LoopEnd =
                            reg.LoopLength == 0
                                ? (uint)Waves[(int)reg.WaveId].Audio.NumSamples
                                : reg.LoopStart + reg.LoopLength;
                    }
                }
            }
        }
    }
}
