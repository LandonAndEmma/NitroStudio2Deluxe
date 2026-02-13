using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GotaSoundIO.Sound.Playback {
    public class LoopStream : WaveStream {
        WaveStream sourceStream;
        public uint LoopStart;
        public uint LoopEnd;
        private StreamPlayer player;
        public LoopStream(StreamPlayer player, WaveStream sourceStream, bool loops, uint loopStart, uint loopEnd) {
            this.player = player;
            this.sourceStream = sourceStream;
            Loops = loops;
            LoopStart = loopStart;
            LoopEnd = loopEnd;
        }
        public bool Loops { get; set; }
        public override WaveFormat WaveFormat {
            get { return sourceStream.WaveFormat; }
        }
        public override long Length {
            get { return sourceStream.Length; }
        }
        public override long Position {
            get { return sourceStream.Position; }
            set { sourceStream.Position = value; }
        }
        public uint CurrentSample {
            get { return (uint)(sourceStream.Position / WaveFormat.Channels / (WaveFormat.BitsPerSample / 8)); }
            set { sourceStream.Position = value * WaveFormat.Channels * (WaveFormat.BitsPerSample / 8); }
        }
        public uint GetLengthInSamples { 
            get => (uint)(sourceStream.Length / WaveFormat.Channels / (WaveFormat.BitsPerSample / 8));
        }
        public override int Read(byte[] buffer, int offset, int count) {             
            int totalBytesRead = 0;
            while (totalBytesRead < count) {
                int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0 || sourceStream.Position > LoopEnd * WaveFormat.Channels * WaveFormat.BitsPerSample / 8 || sourceStream.Position > sourceStream.Length) {
                    if (sourceStream.Position == 0 || !(Loops && player.Loop)) {
                        break;
                    }
                    if (Loops && player.Loop) {
                        if (CurrentSample >= LoopEnd) {
                            CurrentSample = LoopStart;
                        }
                    }
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }
    }
}
