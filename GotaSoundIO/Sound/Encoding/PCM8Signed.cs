using GotaSoundIO.IO;
using GotaSoundIO.Sound.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
namespace GotaSoundIO.Sound {
    public class PCM8Signed : IAudioEncoding {
        private sbyte[] Data;
        public int SampleCount() => Data.Length;
        public int DataSize() => SampleCount();
        public int SamplesFromBlockSize(int blockSize) => blockSize;
        public object RawData() => Data;
        public void ReadRaw(FileReader r, uint numSamples, uint dataSize) {
            Data = r.ReadSBytes((int)dataSize);
        }
        public void WriteRaw(FileWriter w) {
            foreach (var s in Data) {
                w.Write(s);
            }
        }
        public void FromFloatPCM(float[] pcm, object encodingData = null, int loopStart = -1, int loopEnd = -1) {
            Data = pcm.Select(x => (sbyte)(x * sbyte.MaxValue)).ToArray();
        }
        public float[] ToFloatPCM(object decodingData = null) => Data.Select(x => (float)x / sbyte.MaxValue).ToArray();
        public void Trim(int totalSamples) {
            Data = Data.SubArray(0, totalSamples);
        }
        public List<IAudioEncoding> ChangeBlockSize(List<IAudioEncoding> blocks, int newBlockSize) {
            List<IAudioEncoding> newData = new List<IAudioEncoding>();
            List<sbyte> samples = new List<sbyte>();
            foreach (var b in blocks) {
                samples.AddRange((sbyte[])b.RawData());
            }
            sbyte[] s = samples.ToArray();
            if (newBlockSize == -1) {
                newData.Add(new PCM8Signed() { Data = s });
            }
            else {
                int samplesPerBlock = newBlockSize;
                int currSample = 0;
                while (currSample < samples.Count) {
                    int numToCopy = Math.Min(samples.Count - currSample, samplesPerBlock);
                    newData.Add(new PCM8Signed() { Data = s.SubArray(currSample, numToCopy) });
                    currSample += numToCopy;
                }
            }
            return newData;
        }
        public T GetProperty<T>(string propertyName) { return default; }
        public void SetProperty<T>(T value, string propertyName) {}
        public IAudioEncoding Duplicate() {
            PCM8Signed ret = new PCM8Signed() { Data = new sbyte[Data.Length] };
            Array.Copy(Data, ret.Data, Data.Length);
            return ret;
        }
    }
}
