using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GotaSoundIO.Sound {
    public class DSP : SoundFile {
        public bool Extended;
        public uint BlockSize = 0x2000;
        public override Type[] SupportedEncodings() => new Type[] { typeof(DspAdpcm) };
        public override string Name() => "DSP";
        public override string[] Extensions() => new string[] { "DSP", "MDSP" };
        public override string Description() => "DSP-ADPCM mono file.";
        public override bool SupportsTracks() => false;
        public override Type PreferredEncoding() => typeof(DspAdpcm);
        public DSP() {}
        public DSP(string filePath) : base(filePath) {}
        public override void Read(FileReader r) {
            r.ByteOrder = ByteOrder.BigEndian;
            uint numSamples = r.ReadUInt32();
            r.ReadUInt32(); 
            SampleRate = r.ReadUInt32();
            Loops = r.ReadUInt16() > 0;
            r.ReadUInt16(); 
            LoopStart = r.ReadUInt32();
            LoopEnd = r.ReadUInt32();
            r.ReadUInt32(); 
            DspAdpcmContext context = r.Read<DspAdpcmContext>();
            ushort numChannels = r.ReadUInt16();
            BlockSize = (uint)(r.ReadUInt16() * 8);
            Extended = numChannels > 0;
            r.Align(0x60);
            for (int i = 1; i < numChannels; i++) {
                r.ReadBytes(0x1C);
                r.Align(0x60);
            }
            if (numChannels == 0) { numChannels = 1; }
            long dataLen = r.Length - r.Position;
            long channelLen = dataLen / numChannels;
            if (!Extended) { BlockSize = (uint)channelLen; }
            uint lastBlockSize = (uint)(channelLen % BlockSize);
            bool blockCarry = false;
            if (lastBlockSize == 0) { lastBlockSize = BlockSize; } else { blockCarry = true; }
            uint numBlocks = (uint)(channelLen / BlockSize + (blockCarry ? 1 : 0));
        }
        public override void Write(FileWriter w) {
            if (!Extended) {
                for (int i = Audio.Channels.Count - 1; i >= 1; i--) {
                    Audio.Channels.RemoveAt(i);
                }
            }
            w.ByteOrder = ByteOrder.BigEndian;
            for (int i = 0; i < Audio.Channels.Count; i++) {
                w.Write(Audio.NumSamples);
                w.Write(SampleRate);
                w.Write((ushort)(Loops ? 1 : 0));
                w.Write((ushort)0);
                w.Write(LoopStart);
                w.Write(LoopEnd);
                w.Write((uint)2);
                w.Write((Audio.Channels[i][0] as DspAdpcm).Context);
                if (Extended) {
                    w.Write((ushort)Audio.Channels.Count);
                    w.Write((ushort)(BlockSize / 8));
                }
                w.Align(0x60);
            }
            Audio.ChangeBlockSize((int)BlockSize);
            Audio.Write(w);
        }
    }
}
