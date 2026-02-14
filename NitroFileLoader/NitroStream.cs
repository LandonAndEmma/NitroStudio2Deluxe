using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;
using GotaSoundIO.Sound;

namespace NitroFileLoader
{
    public class Stream : SoundFile
    {
        public override Type[] SupportedEncodings() =>
            new Type[] { typeof(ImaAdpcm), typeof(PCM16), typeof(PCM8Signed) };

        public override string Name() => "STRM";

        public override string[] Extensions() => new string[] { "STRM" };

        public override string Description() => "An STRM stream used in Nintendo DS games.";

        public override bool SupportsTracks() => false;

        public override Type PreferredEncoding() => typeof(ImaAdpcm);

        public override void Read(FileReader r)
        {
            r.OpenFile<NHeader>(out _);
            r.OpenBlock(0, out _, out _);
            PcmFormat pcmFormat = (PcmFormat)r.ReadByte();
            Loops = r.ReadBoolean();
            int numChannels = r.ReadByte();
            r.ReadByte();
            SampleRate = r.ReadUInt16();
            r.ReadUInt16();
            LoopStart = r.ReadUInt32();
            uint numSamples = r.ReadUInt32();
            if (Loops)
            {
                LoopEnd = numSamples;
            }
            r.OpenOffset("dataOffset");
            uint numBlocks = r.ReadUInt32();
            uint blockSize = r.ReadUInt32();
            uint blockSamples = r.ReadUInt32();
            uint lastBlockSize = r.ReadUInt32();
            uint lastBlockSamples = r.ReadUInt32();
            r.ReadBytes(32);
            Type encodingType = null;
            switch (pcmFormat)
            {
                case PcmFormat.SignedPCM8:
                    encodingType = typeof(PCM8Signed);
                    break;
                case PcmFormat.PCM16:
                    encodingType = typeof(PCM16);
                    break;
                case PcmFormat.Encoded:
                    encodingType = typeof(ImaAdpcm);
                    break;
            }
            r.JumpToOffset("dataOffset", true, true);
            Audio.Read(
                r,
                encodingType,
                numChannels,
                numBlocks,
                blockSize,
                blockSamples,
                lastBlockSize,
                lastBlockSamples,
                0
            );
        }

        public override void Write(FileWriter w)
        {
            w.InitFile<NHeader>("STRM", ByteOrder.LittleEndian, null, 2);
            long countOff = w.Position - 2;
            w.Write("HEAD".ToCharArray());
            w.Write((uint)0x50);
            uint blockSamples = (uint)Audio.NumSamples;
            uint blockSize = (uint)Audio.DataSize;
            if (Audio.EncodingType.Equals(typeof(PCM8Signed)))
            {
                w.Write((byte)PcmFormat.SignedPCM8);
            }
            else if (Audio.EncodingType.Equals(typeof(PCM16)))
            {
                w.Write((byte)PcmFormat.PCM16);
            }
            else if (Audio.EncodingType.Equals(typeof(ImaAdpcm)))
            {
                w.Write((byte)PcmFormat.Encoded);
                blockSize = (uint)Audio.BlockSize;
                blockSamples = (blockSize - 4) * 2;
            }
            else
            {
                throw new Exception("Invalid channel format!");
            }
            w.Write(Loops);
            w.Write((byte)Audio.Channels.Count());
            w.Write((byte)0);
            w.Write((ushort)SampleRate);
            w.Write((ushort)Math.Floor((decimal)523655.96875 * ((decimal)1 / (decimal)SampleRate)));
            w.Write(LoopStart);
            w.Write(Audio.NumSamples);
            w.Write((uint)0x68);
            w.Write(Audio.NumBlocks);
            w.Write(blockSize);
            w.Write(blockSamples);
            w.Write(Audio.LastBlockSize);
            w.Write(Audio.LastBlockSamples);
            w.Write(new byte[0x20]);
            long bak = w.Position;
            w.Write("DATA".ToCharArray());
            w.Write((uint)0);
            Audio.Write(w);
            w.Pad(4);
            long bak2 = w.Position;
            w.Position = bak + 4;
            w.Write((uint)(bak2 - bak));
            w.Position = bak2;
            w.CloseFile();
            bak = w.Position;
            w.Position = countOff;
            w.Write((ushort)2);
            w.Position = bak;
        }

        public override void BeforeConversion()
        {
            if (Audio.BlockSize == -1)
            {
                Audio.ChangeBlockSize(0x200);
            }
        }

        public override void AfterConversion()
        {
            AlignLoopToBlock((uint)Audio.BlockSamples);
            TrimAfterLoopEnd();
            LoopEnd = (uint)Audio.NumSamples;
        }
    }
}
