using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using NAudio.Wave;

namespace GotaSoundIO.Sound.Playback
{
    public class StreamPlayer : IDisposable
    {
        public IWavePlayer SoundOut;
        public bool Loop;
        private RiffWave Riff;

        public StreamPlayer()
        {
            SoundOut = new WaveOut();
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
            SoundOut = new WaveOut();
            LoopStream = new LoopStream(
                this,
                WaveFileReader,
                Riff.Loops && Loop,
                s.LoopStart,
                (Riff.Loops && Loop) ? s.LoopEnd : (uint)s.Audio.NumSamples
            );
            try
            {
                SoundOut.Init(LoopStream);
            }
            catch (NAudio.MmException)
            {
                SoundOut = new NullWavePlayer();
            }
        }

        public uint GetPosition()
        {
            return LoopStream == null ? 0 : LoopStream.CurrentSample;
        }

        public void SetPosition(uint pos)
        {
            if (LoopStream != null)
            {
                LoopStream.CurrentSample = pos;
            }
        }

        public uint GetLength()
        {
            return LoopStream == null ? 0 : LoopStream.GetLengthInSamples;
        }

        public void Play()
        {
            SoundOut.Stop();
            SoundOut.Play();
        }

        public void Pause()
        {
            if (SoundOut.PlaybackState == PlaybackState.Paused)
            {
                if (SoundOut as WaveOut != null)
                {
                    (SoundOut as WaveOut).Resume();
                }
            }
            else if (SoundOut.PlaybackState == PlaybackState.Playing)
            {
                SoundOut.Pause();
            }
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
