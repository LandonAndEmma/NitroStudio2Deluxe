using GotaSoundIO.Sound.Formats;
using NAudio.Wave;

using System;
using System.IO;

namespace GotaSoundIO.Sound.Playback
{
    public class StreamPlayer : IDisposable
    {
        public IAudioOutput SoundOut;
        public bool Loop;
        private RiffWave Riff;

        public StreamPlayer()
        {
            SoundOut = CreateSoundOut();
        }

        /// <summary>
        /// OpenAL works on every platform, so there is no OS check here any more. A machine with
        /// no usable audio device still gets working position and length plumbing.
        /// </summary>
        private static IAudioOutput CreateSoundOut()
        {
            try
            {
                return new OpenAlOutput();
            }
            catch
            {
                return new NullAudioOutput();
            }
        }

        public LoopStream LoopStream;
        public WaveFileReader WaveFileReader;
        public MemoryStream MemoryStream;

        public void LoadStream(SoundFile s)
        {
            Riff = new RiffWave();
            Riff.FromOtherStreamFile(s);
            MemoryStream = new MemoryStream(Riff.Write());
            WaveFileReader = new WaveFileReader(MemoryStream);
            SoundOut.Dispose();
            SoundOut = CreateSoundOut();
            // The loop point comes from the file when it has one, otherwise the whole thing
            // loops from the start. The Loop toggle is deliberately not part of this: it only
            // decides how many times to loop, so it can be changed during playback. A loop end
            // past the audio (or a zero end on a file that claims to loop) falls back to the
            // sample count, since a short end would cut playback off immediately.
            uint samples = (uint)s.Audio.NumSamples;
            bool hasLoopPoint = Riff.Loops && s.LoopEnd > s.LoopStart && s.LoopStart < samples;
            LoopStream = new LoopStream(
                this,
                WaveFileReader,
                hasLoopPoint ? s.LoopStart : 0,
                hasLoopPoint ? Math.Min(s.LoopEnd, samples) : samples
            );
            try
            {
                SoundOut.Init(LoopStream);
            }
            catch
            {
                SoundOut = new NullAudioOutput();
            }
        }

        public uint GetPosition()
        {
            return LoopStream == null ? 0 : LoopStream.CurrentSample;
        }

        public void SetPosition(uint pos)
        {
            _ = LoopStream?.CurrentSample = pos;
        }

        public uint GetLength()
        {
            return LoopStream == null ? 0 : LoopStream.GetLengthInSamples;
        }

        public void Play()
        {
            SoundOut.Stop();
            // Pressing Play after a fade has finished should start the track again rather than
            // sit at the end with nothing left to read.
            if (LoopStream is not null && LoopStream.Ended)
            {
                LoopStream.Restart();
            }
            SoundOut.Play();
        }

        public void Pause()
        {
            SoundOut.Pause();
        }

        public void Stop()
        {
            SoundOut.Stop();
        }

        public void Dispose()
        {
            SoundOut.Stop();
            SoundOut.Dispose();
        }
    }
}
