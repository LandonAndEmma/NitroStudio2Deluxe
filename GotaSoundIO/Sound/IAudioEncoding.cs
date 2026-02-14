using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;

namespace GotaSoundIO.Sound.Encoding
{
    public interface IAudioEncoding
    {
        int SampleCount();
        int DataSize();
        int SamplesFromBlockSize(int blockSize);
        object RawData();
        void ReadRaw(FileReader r, uint numSamples, uint dataSize);
        void WriteRaw(FileWriter w);
        void FromFloatPCM(
            float[] pcm,
            object encodingData = null,
            int loopStart = -1,
            int loopEnd = -1
        );
        float[] ToFloatPCM(object decodingData = null);
        void Trim(int totalSamples);
        List<IAudioEncoding> ChangeBlockSize(List<IAudioEncoding> blocks, int newBlockSize);
        T GetProperty<T>(string propertyName);
        void SetProperty<T>(T value, string propertyName);
        IAudioEncoding Duplicate();
    }
}
