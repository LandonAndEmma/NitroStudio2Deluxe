using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GotaSoundIO.Sound.Encoding
{
    public class PCM32Signed : IAudioEncoding
    {
        private int[] Data;

        public int SampleCount()
        {
            return Data.Length;
        }

        public int DataSize()
        {
            return SampleCount() * 4;
        }

        public int SamplesFromBlockSize(int blockSize)
        {
            return blockSize / 4;
        }

        public object RawData()
        {
            return Data;
        }

        public void ReadRaw(FileReader r, uint numSamples, uint dataSize)
        {
            Data = r.ReadInt32s((int)(dataSize / 4));
        }

        public void WriteRaw(FileWriter w)
        {
            w.Write(Data);
        }

        public void FromFloatPCM(
            float[] pcm,
            object encodingData = null,
            int loopStart = -1,
            int loopEnd = -1
        )
        {
            Data = pcm.Select(x => (int)(x * int.MaxValue)).ToArray();
        }

        public float[] ToFloatPCM(object decodingData = null)
        {
            return Data.Select(x => (float)x / int.MaxValue).ToArray();
        }

        public void Trim(int totalSamples)
        {
            Data = Data.SubArray(0, totalSamples);
        }

        public List<IAudioEncoding> ChangeBlockSize(List<IAudioEncoding> blocks, int newBlockSize)
        {
            List<IAudioEncoding> newData = [];
            List<int> samples = [];
            foreach (IAudioEncoding b in blocks)
            {
                samples.AddRange((int[])b.RawData());
            }
            int[] s = samples.ToArray();
            if (newBlockSize == -1)
            {
                newData.Add(new PCM32Signed() { Data = s });
            }
            else
            {
                int samplesPerBlock = newBlockSize / 4;
                int currSample = 0;
                while (currSample < samples.Count)
                {
                    int numToCopy = Math.Min(samples.Count - currSample, samplesPerBlock);
                    newData.Add(new PCM32Signed() { Data = s.SubArray(currSample, numToCopy) });
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
            PCM32Signed ret = new() { Data = new int[Data.Length] };
            Array.Copy(Data, ret.Data, Data.Length);
            return ret;
        }
    }
}
