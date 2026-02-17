using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GotaSoundIO.Sound.Encoding
{
    public class PCM16 : IAudioEncoding
    {
        private short[] Data;

        public int SampleCount()
        {
            return Data.Length;
        }

        public int DataSize()
        {
            return SampleCount() * 2;
        }

        public int SamplesFromBlockSize(int blockSize)
        {
            return blockSize / 2;
        }

        public object RawData()
        {
            return Data;
        }

        public void ReadRaw(FileReader r, uint numSamples, uint dataSize)
        {
            Data = r.ReadInt16s((int)(dataSize / 2));
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
            Data = pcm.Select(x => (short)(x * short.MaxValue)).ToArray();
        }

        public float[] ToFloatPCM(object decodingData = null)
        {
            return Data.Select(x => (float)x / short.MaxValue).ToArray();
        }

        public void Trim(int totalSamples)
        {
            Data = Data.SubArray(0, totalSamples);
        }

        public List<IAudioEncoding> ChangeBlockSize(List<IAudioEncoding> blocks, int newBlockSize)
        {
            List<IAudioEncoding> newData = [];
            List<short> samples = [];
            foreach (IAudioEncoding b in blocks)
            {
                samples.AddRange((short[])b.RawData());
            }
            short[] s = samples.ToArray();
            if (newBlockSize == -1)
            {
                newData.Add(new PCM16() { Data = s });
            }
            else
            {
                int samplesPerBlock = newBlockSize / 2;
                int currSample = 0;
                while (currSample < samples.Count)
                {
                    int numToCopy = Math.Min(samples.Count - currSample, samplesPerBlock);
                    newData.Add(new PCM16() { Data = s.SubArray(currSample, numToCopy) });
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
            PCM16 ret = new() { Data = new short[Data.Length] };
            Array.Copy(Data, ret.Data, Data.Length);
            return ret;
        }
    }
}
