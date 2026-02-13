using GotaSoundIO.IO;
using GotaSoundIO.Sound.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
namespace GotaSoundIO.Sound {
    public class PCM8 : IAudioEncoding {
        private byte[] Data;
        public int SampleCount() => Data.Length;
        public int DataSize() => SampleCount();
        public int SamplesFromBlockSize(int blockSize) => blockSize;
        public object RawData() => Data;
        public void ReadRaw(FileReader r, uint numSamples, uint dataSize) {
            Data = r.ReadBytes((int)dataSize);
        }
        public void WriteRaw(FileWriter w) {
            w.Write(Data);
        }
        public void FromFloatPCM(float[] pcm, object encodingData = null, int loopStart = -1, int loopEnd = -1) {
            Data = pcm.Select(x => (byte)(x * sbyte.MaxValue + 128)).ToArray();
        }
        public float[] ToFloatPCM(object decodingData = null) => Data.Select(x => (float)(x - 128) / sbyte.MaxValue).ToArray();
        public void Trim(int totalSamples) {
            Data = Data.SubArray(0, totalSamples);
        }
        public List<IAudioEncoding> ChangeBlockSize(List<IAudioEncoding> blocks, int newBlockSize) {
            List<IAudioEncoding> newData = new List<IAudioEncoding>();
            List<byte> samples = new List<byte>();
            foreach (var b in blocks) {
                samples.AddRange((byte[])b.RawData());
            }
            byte[] s = samples.ToArray();
            if (newBlockSize == -1) {
                newData.Add(new PCM8() { Data = s });
            }
            else {
                int samplesPerBlock = newBlockSize;
                int currSample = 0;
                while (currSample < samples.Count) {
                    int numToCopy = Math.Min(samples.Count - currSample, samplesPerBlock);
                    newData.Add(new PCM8() { Data = s.SubArray(currSample, numToCopy) });
                    currSample += numToCopy;
                }
            }
            return newData;
        }
        public T GetProperty<T>(string propertyName) { return default; }
        public void SetProperty<T>(T value, string propertyName) {}
        public IAudioEncoding Duplicate() {
            PCM8 ret = new PCM8() { Data = new byte[Data.Length] };
            Array.Copy(Data, ret.Data, Data.Length);
            return ret;
        }
    }
}
