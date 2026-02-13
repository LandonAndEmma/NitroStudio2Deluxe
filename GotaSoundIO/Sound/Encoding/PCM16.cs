using GotaSoundIO.IO;
using GotaSoundIO.Sound.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
namespace GotaSoundIO.Sound {
    public class PCM16 : IAudioEncoding {
        private short[] Data;
        public int SampleCount() => Data.Length;
        public int DataSize() => SampleCount() * 2;
        public int SamplesFromBlockSize(int blockSize) => blockSize / 2;
        public object RawData() => Data;
        public void ReadRaw(FileReader r, uint numSamples, uint dataSize) {
            Data = r.ReadInt16s((int)(dataSize / 2));
        }
        public void WriteRaw(FileWriter w) {
            w.Write(Data);
        }
        public void FromFloatPCM(float[] pcm, object encodingData = null, int loopStart = -1, int loopEnd = -1) {
            Data = pcm.Select(x => (short)(x * short.MaxValue)).ToArray();
        }
        public float[] ToFloatPCM(object decodingData = null) => Data.Select(x => (float)x / short.MaxValue).ToArray();
        public void Trim(int totalSamples) {
            Data = Data.SubArray(0, totalSamples);
        }
        public List<IAudioEncoding> ChangeBlockSize(List<IAudioEncoding> blocks, int newBlockSize) {
            List<IAudioEncoding> newData = new List<IAudioEncoding>();
            List<short> samples = new List<short>();
            foreach (var b in blocks) {
                samples.AddRange((short[])b.RawData());
            }
            short[] s = samples.ToArray();
            if (newBlockSize == -1) {
                newData.Add(new PCM16() { Data = s });
            }
            else {
                int samplesPerBlock = newBlockSize / 2;
                int currSample = 0;
                while (currSample < samples.Count) {
                    int numToCopy = Math.Min(samples.Count - currSample, samplesPerBlock);
                    newData.Add(new PCM16() { Data = s.SubArray(currSample, numToCopy) });
                    currSample += numToCopy;
                }
            }
            return newData;
        }
        public T GetProperty<T>(string propertyName) { return default; }
        public void SetProperty<T>(T value, string propertyName) {}
        public IAudioEncoding Duplicate() {
            PCM16 ret = new PCM16() { Data = new short[Data.Length] };
            Array.Copy(Data, ret.Data, Data.Length);
            return ret;
        }
    }
}
