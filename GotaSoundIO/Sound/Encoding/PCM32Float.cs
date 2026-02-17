using GotaSoundIO.IO;
using System;
using System.Collections.Generic;

namespace GotaSoundIO.Sound.Encoding
{
    public class PCM32Float : IAudioEncoding
    {
        private float[] Data;

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
            Data = r.ReadSingles((int)(dataSize / 4));
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
            Data = new float[pcm.Length];
            Array.Copy(pcm, Data, pcm.Length);
        }

        public float[] ToFloatPCM(object decodingData = null)
        {
            return Data;
        }

        public void Trim(int totalSamples)
        {
            Data = Data.SubArray(0, totalSamples);
        }

        public List<IAudioEncoding> ChangeBlockSize(List<IAudioEncoding> blocks, int newBlockSize)
        {
            List<IAudioEncoding> newData = [];
            List<float> samples = [];
            foreach (IAudioEncoding b in blocks)
            {
                samples.AddRange(b.ToFloatPCM());
            }
            float[] s = samples.ToArray();
            if (newBlockSize == -1)
            {
                newData.Add(new PCM32Float() { Data = s });
            }
            else
            {
                int samplesPerBlock = newBlockSize / 4;
                int currSample = 0;
                while (currSample < samples.Count)
                {
                    int numToCopy = Math.Min(samples.Count - currSample, samplesPerBlock);
                    newData.Add(new PCM32Float() { Data = s.SubArray(currSample, numToCopy) });
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
            PCM32Float ret = new() { Data = new float[Data.Length] };
            Array.Copy(Data, ret.Data, Data.Length);
            return ret;
        }
    }
}
