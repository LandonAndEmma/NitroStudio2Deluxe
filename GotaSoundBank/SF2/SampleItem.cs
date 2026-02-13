using GotaSoundIO.IO;
using GotaSoundIO.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GotaSoundBank.SF2 {
    public class SampleItem : IReadable, IWriteable {
        public string Name = "";
        public byte OriginalPitch = 60;
        public sbyte PitchCorrection;
        public ushort Link;
        public bool IsRomType;
        public SF2LinkTypes LinkType;
        public RiffWave Wave;
        public void Read(FileReader r) {
            Name = r.ReadFixedString(20);
            uint startSample = r.ReadUInt32();
            uint endSample = r.ReadUInt32();
            long bak = r.Position;
            r.Position = r.CurrentOffset;
            r.Position += startSample * 2;
            Wave = new RiffWave() { Audio = new AudioData() { Channels = new List<List<GotaSoundIO.Sound.Encoding.IAudioEncoding>>() { new List<GotaSoundIO.Sound.Encoding.IAudioEncoding>() { new PCM16() } } } };
            Wave.Audio.Channels[0][0].ReadRaw(r, (uint)((endSample * 2 + r.CurrentOffset - r.Position) / 2), (uint)(endSample * 2 + r.CurrentOffset - r.Position));
            r.Position = bak;
            Wave.LoopStart = r.ReadUInt32();
            Wave.LoopEnd = r.ReadUInt32();
            if (Wave.LoopEnd != 0) {
                Wave.LoopStart -= startSample;
                Wave.LoopEnd -= startSample;
            }
            Wave.Loops = Wave.LoopEnd > 0;
            Wave.SampleRate = r.ReadUInt32();
            OriginalPitch = r.ReadByte();
            PitchCorrection = r.ReadSByte();
            Link = r.ReadUInt16();
            ushort type = r.ReadUInt16();
            LinkType = (SF2LinkTypes)(type & 0b1111);
            IsRomType = (type & 0b1000000000000000) > 0;
        }
        public void Write(FileWriter w) {
            long waveTableStart = w.StructureOffsets.Pop();
            w.WriteFixedString(Name, 20);
            uint startSample = (uint)((w.CurrentOffset - waveTableStart) / 2);
            w.Write(startSample);
            w.Write((uint)(startSample + Wave.Audio.NumSamples));
            long bak = w.Position;
            w.Position = w.CurrentOffset;
            Wave.Audio.Write(w);
            w.Position = bak;
            w.Write((uint)(Wave.Loops ? Wave.LoopStart + startSample : 0));
            w.Write((uint)(Wave.Loops ? Wave.LoopEnd + startSample : 0));
            w.Write(Wave.SampleRate);
            w.Write(OriginalPitch);
            w.Write(PitchCorrection);
            w.Write(Link);
            ushort val = (ushort)LinkType;
            if (IsRomType) { val |= 0b1000000000000000; }
            w.Write(val);
        }
    }
}
