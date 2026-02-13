using GotaSoundIO.IO;
using GotaSoundIO.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace NitroFileLoader {
    public class Wave : SoundFile {
        public override Type[] SupportedEncodings() => new Type[] { typeof(ImaAdpcm), typeof(PCM16), typeof(PCM8Signed) };
        public override string Name() => "SWAV";
        public override string[] Extensions() => new string[] { "SWAV" };
        public override string Description() => "A SWAV used in Nintendo DS games.";
        public override bool SupportsTracks() => false;
        public override Type PreferredEncoding() => typeof(ImaAdpcm);
        private ushort BackupNTime;
        public override void Read(FileReader r) {
            r.OpenFile<NHeader>(out _);
            r.OpenBlock(0, out _, out _, false);
            r.ReadUInt32();
            uint dataSize = r.ReadUInt32() - 8;
            var w = ReadShortened(r, dataSize);
            Loops = w.Loops;
            LoopStart = w.LoopStart;
            LoopEnd = w.LoopEnd;
            SampleRate = w.SampleRate;
            Audio = w.Audio;
        }
        public static Wave ReadShortened(FileReader r, uint length) {
            Wave w = new Wave();
            PcmFormat pcmFormat = (PcmFormat)r.ReadByte();
            w.Loops = r.ReadBoolean();
            int numChannels = 1;
            w.SampleRate = r.ReadUInt16();
            w.BackupNTime = r.ReadUInt16();
            w.LoopStart = r.ReadUInt16();
            r.ReadUInt32(); 
            uint dataSize = length - 12;
            w.LoopEnd = dataSize * 2;
            w.LoopStart = Offset2Samples(w.LoopStart * 4, pcmFormat);
            w.LoopEnd = Offset2Samples(dataSize, pcmFormat);
            Type format = null;
            switch (pcmFormat) {
                case PcmFormat.SignedPCM8:
                    format = typeof(PCM8Signed);
                    break;
                case PcmFormat.PCM16:
                    format = typeof(PCM16);
                    break;
                case PcmFormat.Encoded:
                    format = typeof(ImaAdpcm);
                    break;
            }
            w.Audio.Read(r, format, numChannels, (int)dataSize, (int)w.LoopEnd, 0);
            return w;
        }
        public void WriteShortened(FileWriter w) {
            PcmFormat pcmFormat = PcmFormat.Encoded;
            if (Audio.EncodingType.Equals(typeof(PCM8Signed))) {
                w.Write((byte)PcmFormat.SignedPCM8);
                pcmFormat = PcmFormat.SignedPCM8;
            } else if (Audio.EncodingType.Equals(typeof(PCM16))) {
                w.Write((byte)PcmFormat.PCM16);
                pcmFormat = PcmFormat.PCM16;
            } else if (Audio.EncodingType.Equals(typeof(ImaAdpcm))) {
                w.Write((byte)PcmFormat.Encoded);
                pcmFormat = PcmFormat.Encoded;
            } else {
                throw new Exception("Invalid channel format!");
            }
            w.Write(Loops);
            w.Write((ushort)SampleRate);
            if (SampleRate == 0) {
                throw new InvalidWaveException("Wave has invalid sample rate (0). This wave cannot be serialized.");
            }
            ushort nTimeSampleRate = (ushort)(16756991 / SampleRate);
            if (BackupNTime != 0) { w.Write(BackupNTime); } else { w.Write(nTimeSampleRate); }
            if (Loops) { w.Write((ushort)(Sample2Offset(LoopStart, pcmFormat) / 4)); } else { w.Write((ushort)(pcmFormat == PcmFormat.Encoded ? 1 : 0)); }
            if (Loops) { w.Write((uint)((Audio.DataSize - Sample2Offset(LoopStart, pcmFormat)) / 4)); } else { w.Write((uint)((Audio.DataSize - Sample2Offset((uint)(pcmFormat == PcmFormat.Encoded ? 1 : 0), pcmFormat)) / 4)); }
            Audio.Write(w);
        }
        public override void Write(FileWriter w) {
            w.InitFile<NHeader>("SWAV", ByteOrder.LittleEndian, null, 1);
            w.InitBlock("DATA");
            WriteShortened(w);
            w.CloseBlock();
            w.CloseFile();
        }
        public override void BeforeConversion() {
            Audio.MixToMono();
            Audio.ChangeBlockSize(-1);
        }
        public override void AfterConversion() {
            TrimAfterLoopEnd();
            LoopEnd = (uint)Audio.NumSamples;
        }
        public static uint Offset2Samples(uint offset, PcmFormat format) {
            uint samples = offset;
            switch (format) {
                case PcmFormat.SignedPCM8:
                    return samples;
                case PcmFormat.PCM16:
                    return samples / 2;
                case PcmFormat.Encoded:
                    return samples * 2 - 8;
            }
            return 0;
        }
        public static uint Sample2Offset(uint sample, PcmFormat format) {
            uint offset = sample;
            switch (format) {
                case PcmFormat.SignedPCM8:
                    return offset;
                case PcmFormat.PCM16:
                    return offset * 2;
                case PcmFormat.Encoded:
                    return (offset + 8) / 2;
            }
            return 0;
        }
    }
    public enum PcmFormat : byte {
        SignedPCM8, PCM16, Encoded
    }
    public class InvalidWaveException : Exception {
        public InvalidWaveException(string message) : base(message) { }
        public InvalidWaveException(string message, Exception innerException) : base(message, innerException) { }
    }
}
