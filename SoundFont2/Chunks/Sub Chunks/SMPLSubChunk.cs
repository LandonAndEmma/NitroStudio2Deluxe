using System;
using Kermalis.EndianBinaryIO;

namespace Kermalis.SoundFont2
{
    public sealed class SMPLSubChunk : SF2Chunk
    {
        private short[] _samples;

        internal SMPLSubChunk(SF2 inSf2)
            : base(inSf2, "smpl")
        {
            _samples = Array.Empty<short>();
        }

        internal SMPLSubChunk(SF2 inSf2, EndianBinaryReader reader)
            : base(inSf2, reader)
        {
            _samples = new short[Size / sizeof(short)];
            reader.ReadInt16s(_samples);
        }

        internal uint AddSample(ReadOnlySpan<short> pcm16, bool bLoop, uint loopPos)
        {
            int start = _samples.Length;
            uint sampleIndex = (uint)start;
            int numNewSamples = start + pcm16.Length + (bLoop ? 8 : 0) + 46;
            Array.Resize(ref _samples, numNewSamples);
            pcm16.CopyTo(_samples.AsSpan(start));
            start += pcm16.Length;
            if (bLoop)
            {
                uint max = (uint)pcm16.Length - loopPos;
                for (uint i = 0; i < 8; i++)
                {
                    _samples[start++] = pcm16[(int)(loopPos + (i % max))];
                }
            }
            Size = (uint)_samples.Length * sizeof(short);
            _sf2.UpdateSize();
            return sampleIndex;
        }

        internal override void Write(EndianBinaryWriter writer)
        {
            base.Write(writer);
            writer.WriteInt16s(_samples);
        }

        public override string ToString()
        {
            return $"Sample Data Chunk";
        }
    }
}
