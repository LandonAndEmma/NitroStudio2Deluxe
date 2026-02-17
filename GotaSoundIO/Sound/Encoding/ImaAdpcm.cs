using GotaSoundIO.IO;
using System;
using System.Collections.Generic;

namespace GotaSoundIO.Sound.Encoding
{
    public class ImaAdpcm : IAudioEncoding
    {
        private byte[] Data;
        private int Sample;
        private int Index;

        public int SampleCount()
        {
            return Data.Length * 2;
        }

        public int DataSize()
        {
            return Data.Length + 4;
        }

        public int SamplesFromBlockSize(int blockSize)
        {
            return (blockSize - 4) * 2;
        }

        public object RawData()
        {
            return Data;
        }

        public void ReadRaw(FileReader r, uint numSamples, uint dataSize)
        {
            Sample = r.ReadInt16();
            Index = r.ReadInt16();
            Data = r.ReadBytes((int)(dataSize - 4));
        }

        public void WriteRaw(FileWriter w)
        {
            w.Write((short)Sample);
            w.Write((short)Index);
            w.Write(Data);
        }

        public void FromFloatPCM(
            float[] pcm,
            object encodingData = null,
            int loopStart = -1,
            int loopEnd = -1
        )
        {
            ImaAdpcmEncoder e = new(pcm, out Sample, out Index);
            Data = e.Encode();
        }

        public float[] ToFloatPCM(object decodingData = null)
        {
            ImaAdpcmDecoder d = new(Sample, Index, Data, 0);
            return d.Decode();
        }

        public void Trim(int totalSamples)
        {
            Data = Data.SubArray(0, (totalSamples / 2) + (totalSamples % 2 == 1 ? 1 : 0));
        }

        public List<IAudioEncoding> ChangeBlockSize(List<IAudioEncoding> blocks, int newBlockSize)
        {
            AudioData a = new()
            {
                Channels = [blocks],
            };
            a.Convert(typeof(ImaAdpcm), newBlockSize);
            return a.Channels[0];
        }

        public T GetProperty<T>(string propertyName)
        {
            if (propertyName.ToLower().Equals("sample"))
            {
                return (T)(object)Sample;
            }
            else if (propertyName.ToLower().Equals("index"))
            {
                return (T)(object)Index;
            }
            return default;
        }

        public void SetProperty<T>(T value, string propertyName)
        {
            if (propertyName.ToLower().Equals("sample"))
            {
                Sample = (int)(object)value;
            }
            else if (propertyName.ToLower().Equals("index"))
            {
                Index = (int)(object)value;
            }
        }

        public IAudioEncoding Duplicate()
        {
            ImaAdpcm ret = new() { Data = new byte[Data.Length] };
            Array.Copy(Data, ret.Data, Data.Length);
            ret.Sample = Sample;
            ret.Index = Index;
            return ret;
        }
    }

    public class ImaAdpcmMath
    {
        public static readonly int[] IndexTable = new int[16]
        {
            -1,
            -1,
            -1,
            -1,
            2,
            4,
            6,
            8,
            -1,
            -1,
            -1,
            -1,
            2,
            4,
            6,
            8,
        };
        public static readonly int[] StepTable = new int[89]
        {
            7,
            8,
            9,
            10,
            11,
            12,
            13,
            14,
            16,
            17,
            19,
            21,
            23,
            25,
            28,
            31,
            34,
            37,
            41,
            45,
            50,
            55,
            60,
            66,
            73,
            80,
            88,
            97,
            107,
            118,
            130,
            143,
            157,
            173,
            190,
            209,
            230,
            253,
            279,
            307,
            337,
            371,
            408,
            449,
            494,
            544,
            598,
            658,
            724,
            796,
            876,
            963,
            1060,
            1166,
            1282,
            1411,
            1552,
            1707,
            1878,
            2066,
            2272,
            2499,
            2749,
            3024,
            3327,
            3660,
            4026,
            4428,
            4871,
            5358,
            5894,
            6484,
            7132,
            7845,
            8630,
            9493,
            10442,
            11487,
            12635,
            13899,
            15289,
            16818,
            18500,
            20350,
            22385,
            24623,
            27086,
            29794,
            short.MaxValue,
        };

        public static short ClampSample(int value)
        {
            if (value < -32767)
            {
                value = -32767;
            }

            if (value > short.MaxValue)
            {
                value = short.MaxValue;
            }

            return (short)value;
        }

        public static int ClampIndex(int value)
        {
            if (value < 0)
            {
                value = 0;
            }

            if (value > 88)
            {
                value = 88;
            }

            return value;
        }
    }

    public class ImaAdpcmDecoder
    {
        public int Sample;
        public int Index;
        public int Offset;
        public bool SecondNibble;
        private readonly byte[] Data;

        public ImaAdpcmDecoder(
            int sample,
            int index,
            byte[] data,
            int offset = 0,
            bool secondNibble = false
        )
        {
            Sample = sample;
            Index = index;
            Data = data;
            Offset = offset;
            SecondNibble = secondNibble;
        }

        public float[] Decode()
        {
            List<float> ret = [];
            while (Offset < Data.Length)
            {
                ret.Add((float)GetSample() / short.MaxValue);
            }
            return ret.ToArray();
        }

        private short GetSample()
        {
            short sample = GetSample((byte)((Data[Offset] >> (SecondNibble ? 4 : 0)) & 15));
            if (SecondNibble)
            {
                ++Offset;
            }

            SecondNibble = !SecondNibble;
            return sample;
        }

        private short GetSample(byte nibble)
        {
            Sample = ImaAdpcmMath.ClampSample(
                Sample
                    + ((
                        (ImaAdpcmMath.StepTable[Index] / 8)
                        + (ImaAdpcmMath.StepTable[Index] / 4 * (nibble & 1))
                        + (ImaAdpcmMath.StepTable[Index] / 2 * ((nibble >> 1) & 1))
                        + (ImaAdpcmMath.StepTable[Index] * ((nibble >> 2) & 1))
                    ) * (((nibble >> 3) & 1) == 1 ? -1 : 1))
            );
            Index = ImaAdpcmMath.ClampIndex(Index + ImaAdpcmMath.IndexTable[nibble & 7]);
            return (short)Sample;
        }
    }

    public class ImaAdpcmEncoder
    {
        private int Sample;
        private int Index;
        private readonly float[] Data;

        public ImaAdpcmEncoder(float[] data, out int sample, out int index)
        {
            Sample = sample = ConvertFloat(data[0]);
            Index = index = GetBestTableIndex((ConvertFloat(data[1]) - ConvertFloat(data[0])) * 8);
            Data = data;
        }

        public byte[] Encode()
        {
            byte[] data = new byte[(Data.Length / 2) + (Data.Length % 2 != 0 ? 1 : 0)];
            bool secondNibble = false;
            int dataPtr = 0;
            for (int i = 0; i < Data.Length; i++)
            {
                int config = GetBestConfig(Index, ConvertFloat(Data[i]) - Sample);
                Sample = ImaAdpcmMath.ClampSample(
                    Sample
                        + ((
                            (ImaAdpcmMath.StepTable[Index] / 8)
                            + (ImaAdpcmMath.StepTable[Index] / 4 * (config & 1))
                            + (ImaAdpcmMath.StepTable[Index] / 2 * ((config >> 1) & 1))
                            + (ImaAdpcmMath.StepTable[Index] * ((config >> 2) & 1))
                        ) * (((config >> 3) & 1) == 1 ? -1 : 1))
                );
                Index = ImaAdpcmMath.ClampIndex(Index + ImaAdpcmMath.IndexTable[config & 7]);
                if (!secondNibble)
                {
                    data[dataPtr] |= (byte)((config & 0xF) << 0);
                }
                else
                {
                    data[dataPtr] |= (byte)((config & 0xF) << 4);
                    dataPtr++;
                }
                secondNibble = !secondNibble;
            }
            return data;
        }

        private short ConvertFloat(float sample)
        {
            return (short)(sample * short.MaxValue);
        }

        private int GetBestTableIndex(int diff)
        {
            int num1 = int.MaxValue;
            int num2 = -1;
            for (int index = 0; index < ImaAdpcmMath.StepTable.Length; ++index)
            {
                int num3 = Math.Abs(Math.Abs(diff) - ImaAdpcmMath.StepTable[index]);
                if (num3 < num1)
                {
                    num1 = num3;
                    num2 = index;
                }
            }
            return num2;
        }

        private int GetBestConfig(int index, int diff)
        {
            int num1 = 0;
            if (diff < 0)
            {
                num1 |= 8;
            }

            diff = Math.Abs(diff);
            int num2 = ImaAdpcmMath.StepTable[index] / 8;
            if (Math.Abs(num2 - diff) >= ImaAdpcmMath.StepTable[index])
            {
                num1 |= 4;
                num2 += ImaAdpcmMath.StepTable[index];
            }
            if (Math.Abs(num2 - diff) >= ImaAdpcmMath.StepTable[index] / 2)
            {
                num1 |= 2;
                num2 += ImaAdpcmMath.StepTable[index] / 2;
            }
            if (Math.Abs(num2 - diff) >= ImaAdpcmMath.StepTable[index] / 4)
            {
                num1 |= 1;
                _ = num2 + (ImaAdpcmMath.StepTable[index] / 4);
            }
            return num1;
        }
    }
}
