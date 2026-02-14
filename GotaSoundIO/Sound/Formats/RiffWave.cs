using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;
using GotaSoundIO.IO.RIFF;

namespace GotaSoundIO.Sound
{
    public class RiffWave : SoundFile
    {
        public override Type[] SupportedEncodings() => new Type[] { typeof(PCM16), typeof(PCM8) };

        public override string Name() => "WAV";

        public override string[] Extensions() => new string[] { "WAV" };

        public override string Description() => "A standard PCM wav file.";

        public override bool SupportsTracks() => false;

        public override Type PreferredEncoding() => null;

        public RiffWave() { }

        public RiffWave(string filePath)
            : base(filePath) { }

        public override void Read(FileReader r)
        {
            using (RiffReader rr = new RiffReader(r.BaseStream))
            {
                rr.OpenChunk(rr.Chunks.Where(x => x.Magic.Equals("fmt ")).FirstOrDefault());
                if (rr.ReadUInt16() != 1)
                {
                    throw new Exception("Unexpected standard WAV data format.");
                }
                int numChannels = rr.ReadUInt16();
                SampleRate = rr.ReadUInt32();
                rr.ReadUInt32();
                rr.ReadUInt16();
                ushort bitsPerSample = rr.ReadUInt16();
                LoopStart = 0;
                LoopEnd = 0;
                Loops = false;
                if (bitsPerSample != 8 && bitsPerSample != 16)
                {
                    throw new Exception("This tool only accepts 8-bit or 16-bit WAV files.");
                }
                var smpl = rr.Chunks.Where(x => x.Magic.Equals("smpl")).FirstOrDefault();
                if (smpl != null)
                {
                    rr.OpenChunk(smpl);
                    rr.ReadUInt32s(7);
                    Loops = rr.ReadUInt32() > 0;
                    if (Loops)
                    {
                        rr.ReadUInt32s(3);
                        LoopStart = r.ReadUInt32();
                        LoopEnd = r.ReadUInt32();
                    }
                }
                rr.OpenChunk(rr.Chunks.Where(x => x.Magic.Equals("data")).FirstOrDefault());
                uint dataSize = rr.Chunks.Where(x => x.Magic.Equals("data")).FirstOrDefault().Size;
                uint numBlocks = (uint)(dataSize / numChannels / (bitsPerSample / 8));
                r.Position = rr.Position;
                Audio.Read(
                    r,
                    bitsPerSample == 16 ? typeof(PCM16) : typeof(PCM8),
                    numChannels,
                    numBlocks,
                    (uint)bitsPerSample / 8,
                    1,
                    (uint)bitsPerSample / 8,
                    1,
                    0
                );
                Audio.ChangeBlockSize(-1);
            }
        }

        public override void Write(FileWriter w)
        {
            using (RiffWriter rw = new RiffWriter(w.BaseStream))
            {
                rw.InitFile("WAVE");
                rw.StartChunk("fmt ");
                rw.Write((ushort)1);
                rw.Write((ushort)Audio.Channels.Count);
                rw.Write(SampleRate);
                uint bitsPerSample = Audio.EncodingType.Equals(typeof(PCM16)) ? 16u : 8u;
                rw.Write((uint)(SampleRate * Audio.Channels.Count * (bitsPerSample / 8)));
                rw.Write((ushort)(bitsPerSample / 8 * Audio.Channels.Count));
                rw.Write((ushort)bitsPerSample);
                rw.EndChunk();
                if (Loops)
                {
                    rw.StartChunk("smpl");
                    rw.Write(new uint[2]);
                    rw.Write((uint)(1d / SampleRate * 1000000000));
                    rw.Write((uint)60);
                    rw.Write(new uint[3]);
                    rw.Write((uint)1);
                    rw.Write(new uint[3]);
                    rw.Write(LoopStart);
                    rw.Write((ulong)0);
                    rw.EndChunk();
                }
                Audio.ChangeBlockSize((int)bitsPerSample / 8);
                rw.StartChunk("data");
                w.Position = rw.Position;
                Audio.Write(w);
                rw.Position = w.Position;
                while (rw.Position % 2 != 0)
                {
                    rw.Write((byte)0);
                }
                rw.EndChunk();
                rw.CloseFile();
                Audio.ChangeBlockSize(-1);
            }
        }
    }
}
