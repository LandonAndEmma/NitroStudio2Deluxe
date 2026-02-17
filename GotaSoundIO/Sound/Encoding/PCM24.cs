using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GotaSoundIO.Sound.Encoding
{
    public class PCM24 : IAudioEncoding
    {
        private Int24[] Data;

        public int SampleCount()
        {
            return Data.Length;
        }

        public int DataSize()
        {
            return SampleCount() * 3;
        }

        public int SamplesFromBlockSize(int blockSize)
        {
            return blockSize / 3;
        }

        public object RawData()
        {
            return Data;
        }

        public void ReadRaw(FileReader r, uint numSamples, uint dataSize)
        {
            Data = new Int24[numSamples];
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = r.Read<Int24>();
            }
        }

        public void WriteRaw(FileWriter w)
        {
            foreach (Int24 d in Data)
            {
                w.Write(d);
            }
        }

        public void FromFloatPCM(
            float[] pcm,
            object encodingData = null,
            int loopStart = -1,
            int loopEnd = -1
        )
        {
            Data = pcm.Select(x => (Int24)(x * Int24.MaxValue)).ToArray();
        }

        public float[] ToFloatPCM(object decodingData = null)
        {
            return Data.Select(x => x / (float)Int24.MaxValue).ToArray();
        }

        public void Trim(int totalSamples)
        {
            Data = Data.SubArray(0, totalSamples);
        }

        public List<IAudioEncoding> ChangeBlockSize(List<IAudioEncoding> blocks, int newBlockSize)
        {
            List<IAudioEncoding> newData = [];
            List<Int24> samples = [];
            foreach (IAudioEncoding b in blocks)
            {
                samples.AddRange((Int24[])b.RawData());
            }
            Int24[] s = samples.ToArray();
            if (newBlockSize == -1)
            {
                newData.Add(new PCM24() { Data = s });
            }
            else
            {
                int samplesPerBlock = newBlockSize / 2;
                int currSample = 0;
                while (currSample < samples.Count)
                {
                    int numToCopy = Math.Min(samples.Count - currSample, samplesPerBlock);
                    newData.Add(new PCM24() { Data = s.SubArray(currSample, numToCopy) });
                    currSample += numToCopy;
                }
            }
            return newData;
        }

        public T GetProperty<T>(string propertyName)
        {
            return default;
        }

        public void SetProperty<T>(T value, string propertyName) { }

        public IAudioEncoding Duplicate()
        {
            PCM24 ret = new() { Data = new Int24[Data.Length] };
            Array.Copy(Data, ret.Data, Data.Length);
            return ret;
        }
    }
}
