using GotaSoundIO.IO;
using GotaSoundIO.Sound.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VGAudio.Codecs.GcAdpcm;
using VGAudio.Utilities;
namespace GotaSoundIO.Sound {
    public class DspAdpcm : IAudioEncoding {
        private byte[] Data;
        public DspAdpcmContext Context;
        public int SampleCount() => DspAdpcmMath.ByteCountToSampleCount(Data.Length);
        public int DataSize() => Data.Length;
        public int SamplesFromBlockSize(int blockSize) => DspAdpcmMath.ByteCountToSampleCount(blockSize);
        public object RawData() => Data;
        public void ReadRaw(FileReader r, uint numSamples, uint dataSize) {
            Data = r.ReadBytes((int)(dataSize));
        }
        public void WriteRaw(FileWriter w) {
            w.Write(Data);
        }
        public void FromFloatPCM(float[] pcm, object encodingData = null, int loopStart = -1, int loopEnd = -1) {
            short[] s = pcm.Select(x => ConvertFloat(x)).ToArray();
            DspAdpcmContext context = null;
            if (encodingData != null) {
                context = encodingData as DspAdpcmContext;
            }
            if (context == null) {
                context = new DspAdpcmContext();
                context.LoadCoeffs(GcAdpcmCoefficients.CalculateCoefficients(s));
            }
            Data = DspAdpcmEncoder.EncodeSamples(s, context, loopStart);
            encodingData = Context = context;
        }
        public float[] ToFloatPCM(object decodingData = null) {
            DspAdpcmContext context = null;
            if (decodingData != null) {
                context = decodingData as DspAdpcmContext;
            }
            if (context == null) {
                context = Context;
            }
            short[] pcm = new short[SampleCount()];
            DspAdpcmDecoder.Decode(Data, ref pcm, ref Context, (uint)pcm.Length);
            var ret = pcm.Select(x => (float)x / short.MaxValue).ToArray();
            decodingData = Context = context;
            return ret;
        }
        public void Trim(int totalSamples) {
            Data = Data.SubArray(0, DspAdpcmMath.SampleCountToByteCount(totalSamples));
        }
        public List<IAudioEncoding> ChangeBlockSize(List<IAudioEncoding> blocks, int newBlockSize) {
            List<IAudioEncoding> newData = new List<IAudioEncoding>();
            List<short> samples = new List<short>();
            foreach (var b in blocks) {
                samples.AddRange((short[])b.RawData());
            }
            short[] s = samples.ToArray();
            if (newBlockSize == -1) {
            }
            else {
                int samplesPerBlock = newBlockSize / 2;
                int currSample = 0;
                while (currSample < samples.Count) {
                    int numToCopy = Math.Min(samples.Count - currSample, samplesPerBlock);
                    currSample += numToCopy;
                }
            }
            return newData;
        }
        public T GetProperty<T>(string propertyName) {
            if (propertyName.ToLower().Equals("context")) {
                return (T)(object)Context;
            }
            return default;
        }
        public void SetProperty<T>(T value, string propertyName) {
            if (propertyName.ToLower().Equals("context")) {
                Context = (DspAdpcmContext)(object)value;
            }
        }
        public IAudioEncoding Duplicate() {
            DspAdpcm ret = new DspAdpcm() { Data = new byte[Data.Length] };
            Array.Copy(Data, ret.Data, Data.Length);
            ret.Context = new DspAdpcmContext() { coefs = Context.coefs, gain = Context.gain, loop_pred_scale = Context.loop_pred_scale, loop_yn1 = Context.loop_yn1, loop_yn2 = Context.loop_yn2, pred_scale = Context.pred_scale, yn1 = Context.yn1, yn2 = Context.yn2 };
            return ret;
        }
        private short ConvertFloat(float sample) => (short)(sample * short.MaxValue);
        public static DspAdpcmContext GetContext(List<IAudioEncoding> blocks, int loopStart = -1) {
            DspAdpcmContext ret = new DspAdpcmContext();
            if (blocks.Count == 0) { return ret; }
            ret.coefs = (blocks[0] as DspAdpcm).Context.coefs;
            ret.yn1 = (blocks[0] as DspAdpcm).Context.yn1;
            ret.yn2 = (blocks[0] as DspAdpcm).Context.yn2;
            ret.pred_scale = (blocks[0] as DspAdpcm).Context.pred_scale;
            ret.gain = (blocks[0] as DspAdpcm).Context.gain;
            if (loopStart != -1) {
                int samplesPerBlock = blocks[0].SampleCount();
                int blockNum = loopStart / samplesPerBlock;
                ret.loop_yn1 = (blocks[blockNum] as DspAdpcm).Context.loop_yn1;
                ret.loop_yn2 = (blocks[blockNum] as DspAdpcm).Context.loop_yn2;
                ret.loop_pred_scale = (blocks[blockNum] as DspAdpcm).Context.loop_pred_scale;
            }
            if (ret.loop_yn1 == 0 && ret.loop_yn2 == 0) {
                ret.loop_yn1 = (blocks[0] as DspAdpcm).Context.loop_yn1;
                ret.loop_yn2 = (blocks[0] as DspAdpcm).Context.loop_yn2;
                ret.loop_pred_scale = (blocks[0] as DspAdpcm).Context.loop_pred_scale;
            }
            return ret;
        }
    }
    public static class DspAdpcmMath {
        public static readonly int BytesPerFrame = 8;
        public static readonly int SamplesPerFrame = 14;
        public static readonly int NibblesPerFrame = 16;
        public static int NibbleCountToSampleCount(int nibbleCount) {
            int frames = nibbleCount / NibblesPerFrame;
            int extraNibbles = nibbleCount % NibblesPerFrame;
            int extraSamples = extraNibbles < 2 ? 0 : extraNibbles - 2;
            return SamplesPerFrame * frames + extraSamples;
        }
        public static int SampleCountToNibbleCount(int sampleCount) {
            int frames = sampleCount / SamplesPerFrame;
            int extraSamples = sampleCount % SamplesPerFrame;
            int extraNibbles = extraSamples == 0 ? 0 : extraSamples + 2;
            return NibblesPerFrame * frames + extraNibbles;
        }
        public static int NibbleToSample(int nibble) {
            int frames = nibble / NibblesPerFrame;
            int extraNibbles = nibble % NibblesPerFrame;
            int samples = SamplesPerFrame * frames;
            return samples + extraNibbles - 2;
        }
        public static int SampleToNibble(int sample) {
            int frames = sample / SamplesPerFrame;
            int extraSamples = sample % SamplesPerFrame;
            return NibblesPerFrame * frames + extraSamples + 2;
        }
        public static int SampleCountToByteCount(int sampleCount) => SampleCountToNibbleCount(sampleCount).DivideBy2RoundUp();
        public static int ByteCountToSampleCount(int byteCount) => NibbleCountToSampleCount(byteCount * 2);
    }
    public static class DspAdpcmDecoder {
        static sbyte[] NibbleToSbyte = { 0, 1, 2, 3, 4, 5, 6, 7, -8, -7, -6, -5, -4, -3, -2, -1 };
        static uint DivideByRoundUp(uint dividend, uint divisor) {
            return (dividend + divisor - 1) / divisor;
        }
        static sbyte GetHighNibble(byte value) {
            return NibbleToSbyte[(value >> 4) & 0xF];
        }
        static sbyte GetLowNibble(byte value) {
            return NibbleToSbyte[value & 0xF];
        }
        static short Clamp16(int value) {
            if (value > 32767) {
                return 32767;
            }
            if (value < -32678) {
                return -32678;
            }
            return (short)value;
        }
        public static void Decode(byte[] src, ref Int16[] dst, ref DspAdpcmContext cxt, UInt32 samples) {
            short hist1 = cxt.yn1;
            short hist2 = cxt.yn2;
            int dstIndex = 0;
            int srcIndex = 0;
            while (dstIndex < samples) {
                byte header = src[srcIndex++];
                UInt16 scale = (UInt16)(1 << (header & 0xF));
                byte coef_index = (byte)(header >> 4);
                short coef1 = cxt.coefs[coef_index][0];
                short coef2 = cxt.coefs[coef_index][1];
                for (UInt32 b = 0; b < 7; b++) {
                    byte byt = src[srcIndex++];
                    for (UInt32 s = 0; s < 2; s++) {
                        sbyte adpcm_nibble = ((s == 0) ? GetHighNibble(byt) : GetLowNibble(byt));
                        short sample = Clamp16(((adpcm_nibble * scale) << 11) + 1024 + ((coef1 * hist1) + (coef2 * hist2)) >> 11);
                        hist2 = hist1;
                        hist1 = sample;
                        dst[dstIndex++] = sample;
                        if (dstIndex >= samples) break;
                    }
                    if (dstIndex >= samples) break;
                }
            }
            cxt.yn1 = hist1;
            cxt.yn2 = hist2;
        }
    }
    public class DspAdpcmEncoder {
		public static byte[] EncodeSamples(short[] samples, DspAdpcmContext info, int loopStart) {
            byte[] dspAdpcm = GcAdpcmEncoder.Encode(samples, info.GetCoeffs(), new GcAdpcmParameters() { History1 = info.yn1, History2 = info.yn2, SampleCount = samples.Length });
            if (loopStart > 0) info.loop_yn1 = samples[loopStart - 1];
            if (loopStart > 1) info.loop_yn2 = samples[loopStart - 2];
            return dspAdpcm;
        }
    }
    public class DspAdpcmContext : IReadable, IWriteable {
        public short[] GetCoeffs() {
            List<short> c = new List<short>();
            foreach (var a in coefs) {
                c.AddRange(a);
            }
            return c.ToArray();
        }
        public void LoadCoeffs(short[] c) {
            coefs = new short[8][];
            coefs[0] = new short[] { c[0], c[1] };
            coefs[1] = new short[] { c[2], c[3] };
            coefs[2] = new short[] { c[4], c[5] };
            coefs[3] = new short[] { c[6], c[7] };
            coefs[4] = new short[] { c[8], c[9] };
            coefs[5] = new short[] { c[10], c[11] };
            coefs[6] = new short[] { c[12], c[13] };
            coefs[7] = new short[] { c[14], c[15] };
        }
        public void Read(FileReader r) {
            LoadCoeffs(r.ReadInt16s(16));
            gain = r.ReadUInt16();
            pred_scale = r.ReadUInt16();
            yn1 = r.ReadInt16();
            yn2 = r.ReadInt16();
            loop_pred_scale = r.ReadUInt16();
            loop_yn1 = r.ReadInt16();
            loop_yn2 = r.ReadInt16();
        }
        public void Write(FileWriter w) {
            w.Write(GetCoeffs());
            w.Write(gain);
            w.Write(pred_scale);
            w.Write(yn1);
            w.Write(yn2);
            w.Write(loop_pred_scale);
            w.Write(loop_yn1);
            w.Write(loop_yn2);
        }
        public Int16[][] coefs;
        public UInt16 gain;
        public UInt16 pred_scale;
        public Int16 yn1;
        public Int16 yn2;
        public UInt16 loop_pred_scale;
        public Int16 loop_yn1;
        public Int16 loop_yn2;
    }
}
