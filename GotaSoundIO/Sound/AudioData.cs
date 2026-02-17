using GotaSoundIO.IO;
using GotaSoundIO.Sound.Encoding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GotaSoundIO.Sound
{
    public class AudioData
    {
        public Type EncodingType
        {
            get => (field) ?? (Channels.Count() > 0 ? Channels[0][0].GetType() : null);
            set;
        }

        public int BlockSize { get; private set; } = -1;
        public int BlockSamples { get; private set; } = -1;
        public int LastBlockPaddingSize;
        public int NumSamples =>
            Channels.Count == 0 ? 0 : Channels[0].Select(x => x.SampleCount()).Sum();
        public int DataSize =>
            Channels.Count == 0
                ? 0
                : (
                    Channels[0].Select(x => x.DataSize()).Sum()
                    + (LastBlockPaddingSize * (Channels.Count - 1))
                );
        public int NumBlocks => Channels.Count == 0 ? 0 : Channels[0].Count;
        public int LastBlockSize => Channels.Count == 0 ? 0 : Channels[0].Last().DataSize();
        public int LastBlockSamples => Channels.Count == 0 ? 0 : Channels[0].Last().SampleCount();
        public int LastBlockWithPaddingSize => LastBlockSize + LastBlockPaddingSize;
        public List<List<IAudioEncoding>> Channels = [];

        public AudioData Duplicate()
        {
            AudioData a = new()
            {
                EncodingType = EncodingType,
                BlockSize = BlockSize,
                BlockSamples = BlockSamples,
                LastBlockPaddingSize = LastBlockPaddingSize,
                Channels = []
            };
            for (int i = 0; i < Channels.Count; i++)
            {
                List<IAudioEncoding> chan = [];
                for (int j = 0; j < Channels[i].Count; j++)
                {
                    chan.Add(Channels[i][j].Duplicate());
                }
                a.Channels.Add(chan);
            }
            return a;
        }

        public void Convert(
            Type targetEncoding,
            int targetBlockSize,
            int loopStart = -1,
            int loopEnd = -1
        )
        {
            EncodingType = targetEncoding;
            for (int i = 0; i < Channels.Count; i++)
            {
                object decodingData = null;
                List<float> samples = [];
                foreach (IAudioEncoding b in Channels[i])
                {
                    samples.AddRange(b.ToFloatPCM(decodingData));
                }
                float[] s = samples.ToArray();
                Channels[i].Clear();
                IAudioEncoding tmp = (IAudioEncoding)Activator.CreateInstance(targetEncoding);
                if (targetBlockSize == -1)
                {
                    BlockSize = BlockSamples = -1;
                    tmp.FromFloatPCM(s, null, loopStart, loopEnd);
                    Channels[i].Add(tmp);
                    continue;
                }
                object encodingData = null;
                BlockSize = targetBlockSize;
                int samplesPerBlock = tmp.SamplesFromBlockSize(targetBlockSize);
                BlockSamples = samplesPerBlock;
                int numBlocks = samples.Count / samplesPerBlock;
                if (samples.Count % samplesPerBlock != 0)
                {
                    numBlocks++;
                }
                for (int j = 0; j < numBlocks; j++)
                {
                    int numSamples =
                        (j == numBlocks - 1) ? (samples.Count % samplesPerBlock) : samplesPerBlock;
                    if (numSamples == 0)
                    {
                        numSamples = samplesPerBlock;
                    }
                    int lS = -1,
                        lE = -1;
                    if (loopStart != -1)
                    {
                        if (
                            loopStart >= samplesPerBlock * j
                            && loopStart < (samplesPerBlock * j) + numSamples
                        )
                        {
                            lS = loopStart - (samplesPerBlock * j);
                        }
                    }
                    if (loopEnd != -1)
                    {
                        if (
                            loopEnd >= samplesPerBlock * j
                            && loopEnd < (samplesPerBlock * j) + numSamples
                        )
                        {
                            lE = loopEnd - (samplesPerBlock * j);
                        }
                    }
                    IAudioEncoding block = (IAudioEncoding)Activator.CreateInstance(targetEncoding);
                    block.FromFloatPCM(
                        s.SubArray(j * samplesPerBlock, numSamples),
                        encodingData,
                        lS,
                        lE
                    );
                    Channels[i].Add(block);
                }
            }
        }

        public void ChangeBlockSize(int targetBlockSize)
        {
            if (BlockSize == targetBlockSize)
            {
                return;
            }
            IAudioEncoding tmp = (IAudioEncoding)Activator.CreateInstance(EncodingType);
            BlockSize = targetBlockSize;
            for (int i = 0; i < Channels.Count; i++)
            {
                Channels[i] = tmp.ChangeBlockSize(Channels[i], targetBlockSize);
            }
            BlockSamples = targetBlockSize == -1 ? -1 : Channels.Count == 0 ? 0 : Channels[0][0].SampleCount();
        }

        public void Read(
            FileReader r,
            Type encodingType,
            int numChannels,
            int dataSize,
            int numSamples,
            int dataPadding
        )
        {
            EncodingType = encodingType;
            BlockSize = -1;
            BlockSamples = -1;
            LastBlockPaddingSize = dataPadding;
            Channels.Clear();
            for (int i = 0; i < numChannels; i++)
            {
                Channels.Add([]);
                IAudioEncoding data = (IAudioEncoding)Activator.CreateInstance(EncodingType);
                data.ReadRaw(r, (uint)numSamples, (uint)dataSize);
                Channels.Last().Add(data);
                _ = r.ReadBytes(dataPadding);
            }
        }

        public void Read(
            FileReader r,
            Type encodingType,
            int numChannels,
            uint numBlocks,
            uint blockSize,
            uint blockSamples,
            uint lastBlockSize,
            uint lastBlockSamples,
            uint lastBlockPaddingSize
        )
        {
            EncodingType = encodingType;
            BlockSize = (int)blockSize;
            BlockSamples = (int)blockSamples;
            LastBlockPaddingSize = (int)lastBlockPaddingSize;
            Channels.Clear();
            for (int i = 0; i < numChannels; i++)
            {
                Channels.Add([]);
            }
            for (uint i = 0; i < numBlocks - 1; i++)
            {
                for (int j = 0; j < numChannels; j++)
                {
                    IAudioEncoding data = (IAudioEncoding)Activator.CreateInstance(EncodingType);
                    data.ReadRaw(r, blockSamples, blockSize);
                    Channels[j].Add(data);
                }
            }
            for (int i = 0; i < numChannels; i++)
            {
                IAudioEncoding data = (IAudioEncoding)Activator.CreateInstance(EncodingType);
                data.ReadRaw(r, lastBlockSamples, lastBlockSize);
                Channels[i].Add(data);
                _ = r.ReadBytes(LastBlockPaddingSize);
            }
        }

        public void Write(FileWriter w)
        {
            for (int i = 0; i < NumBlocks - 1; i++)
            {
                for (int j = 0; j < Channels.Count; j++)
                {
                    Channels[j][i].WriteRaw(w);
                }
            }
            for (int j = 0; j < Channels.Count; j++)
            {
                Channels[j].Last().WriteRaw(w);
                w.Write(new byte[LastBlockPaddingSize]);
            }
        }

        public byte[] GetSeek()
        {
            if (EncodingType != typeof(DspAdpcm))
            {
                return null;
            }
            using MemoryStream o = new();
            using FileWriter w = new(o);
            for (int i = 0; i < NumBlocks; i++)
            {
                for (int j = 0; j < Channels.Count; j++)
                {
                    w.Write((Channels[j][i] as DspAdpcm).Context.yn1);
                    w.Write((Channels[j][i] as DspAdpcm).Context.yn2);
                }
            }
            return o.ToArray();
        }

        public void SetSeek(byte[] seekInfo)
        {
            if (EncodingType != typeof(DspAdpcm))
            {
                return;
            }
            using MemoryStream src = new(seekInfo);
            using FileReader r = new(src);
            for (int i = 0; i < NumBlocks; i++)
            {
                for (int j = 0; j < Channels.Count; j++)
                {
                    (Channels[j][i] as DspAdpcm).Context.yn1 = r.ReadInt16();
                    (Channels[j][i] as DspAdpcm).Context.yn2 = r.ReadInt16();
                }
            }
        }

        public void Trim(int totalSamples)
        {
            int samplesToTrim = NumSamples - totalSamples;
            for (int i = 0; i < Channels.Count; i++)
            {
                int toTrim = samplesToTrim;
                while (toTrim > 0)
                {
                    int cutSamples = Math.Min(Channels[i].Last().SampleCount(), toTrim);
                    Channels[i].Last().Trim(totalSamples);
                    toTrim -= cutSamples;
                    if (Channels[i].Last().SampleCount() == 0)
                    {
                        _ = Channels[i].Remove(Channels[i].Last());
                    }
                }
            }
        }

        public void MixToMono(bool[] mutes = null)
        {
            if (Channels.Count < 2 && mutes == null)
            {
                return;
            }
            mutes ??= new bool[Channels.Count];
            if (Channels.Count == 0 || mutes.Where(x => x == false).Count() == 0)
            {
                return;
            }
            double divisor = 1 / Math.Sqrt(mutes.Where(x => x == false).Count());
            Convert(typeof(PCM32Float), BlockSamples);
            List<IAudioEncoding> newData = [];
            for (int i = 0; i < NumBlocks; i++)
            {
                List<float> samples = [];
                IAudioEncoding block = new PCM32Float();
                for (int j = 0; j < Channels[0][i].SampleCount(); j++)
                {
                    double sample = 0;
                    for (int k = 0; k < Channels.Count; k++)
                    {
                        if (!mutes[k])
                        {
                            sample += Channels[k][i].ToFloatPCM()[j];
                        }
                    }
                    sample /= divisor;
                    if (sample > 1)
                    {
                        sample = 1;
                    }
                    if (sample < -1)
                    {
                        sample = -1;
                    }
                    samples.Add((float)sample);
                }
                block.FromFloatPCM(samples.ToArray());
                newData.Add(block);
            }
            Channels.Clear();
            Channels.Add(newData);
        }

        public void MixToStereo(
            bool[] mutes = null,
            bool[] isRightChannel = null,
            bool[] isBoth = null
        )
        {
            if (Channels.Count < 3 && mutes == null)
            {
                return;
            }
            mutes ??= new bool[Channels.Count];
            if (isRightChannel == null)
            {
                isRightChannel = new bool[Channels.Count];
                for (int i = 0; i < Channels.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        isRightChannel[i] = true;
                    }
                }
            }
            if (isBoth == null)
            {
                isBoth = new bool[Channels.Count];
                if (Channels.Count % 2 != 0)
                {
                    isBoth[Channels.Count - 1] = true;
                }
            }
            if (Channels.Count == 0 || mutes.Where(x => false).Count() == 0)
            {
                return;
            }
            List<int> lefts = [];
            List<int> rights = [];
            for (int i = 0; i < mutes.Length; i++)
            {
                if (!mutes[i])
                {
                    if (isBoth[i])
                    {
                        lefts.Add(i);
                        rights.Add(i);
                    }
                    else if (isRightChannel[i])
                    {
                        rights.Add(i);
                    }
                    else
                    {
                        lefts.Add(i);
                    }
                }
            }
            Convert(typeof(PCM32Float), BlockSamples);
            double divL = 1 / Math.Sqrt(lefts.Count);
            double divR = 1 / Math.Sqrt(rights.Count);
            List<IAudioEncoding> left = [];
            List<IAudioEncoding> right = [];
            for (int i = 0; i < NumBlocks; i++)
            {
                List<float> samplesL = [];
                List<float> samplesR = [];
                IAudioEncoding blockL = new PCM32Float();
                IAudioEncoding blockR = new PCM32Float();
                for (int j = 0; j < Channels[0][i].SampleCount(); j++)
                {
                    double sampleL = 0;
                    double sampleR = 0;
                    for (int k = 0; k < Channels.Count; k++)
                    {
                        if (!mutes[k])
                        {
                            if (lefts.Contains(k))
                            {
                                sampleL += Channels[k][i].ToFloatPCM()[j];
                            }
                            if (rights.Contains(k))
                            {
                                sampleR += Channels[k][i].ToFloatPCM()[j];
                            }
                        }
                    }
                    sampleL /= divL;
                    if (sampleL > 1)
                    {
                        sampleL = 1;
                    }
                    if (sampleL < -1)
                    {
                        sampleL = -1;
                    }
                    samplesL.Add((float)sampleL);
                    sampleR /= divR;
                    if (sampleR > 1)
                    {
                        sampleR = 1;
                    }
                    if (sampleR < -1)
                    {
                        sampleR = -1;
                    }
                    samplesR.Add((float)sampleR);
                }
                blockL.FromFloatPCM(samplesL.ToArray());
                blockR.FromFloatPCM(samplesR.ToArray());
                left.Add(blockL);
                right.Add(blockR);
            }
            Channels.Clear();
            Channels.Add(left);
            Channels.Add(right);
        }
    }
}
