using NAudio.Wave;
using System;

namespace GotaSoundIO.Sound.Playback
{
    public class NullWavePlayer : IWavePlayer
    {
        public float Volume { get; set; } = 1f;
        public IWaveProvider OutputWaveProvider { get; set; }
        public WaveFormat OutputWaveFormat => OutputWaveProvider?.WaveFormat;
        public PlaybackState PlaybackState => throw new NotImplementedException();
        private PlaybackState m_PlaybackState;
#pragma warning disable CS0067
        public event EventHandler<StoppedEventArgs> PlaybackStopped;
#pragma warning restore CS0067
        public void Play()
        {
            m_PlaybackState = PlaybackState.Playing;
        }

        public void Stop()
        {
            m_PlaybackState = PlaybackState.Stopped;
        }

        public void Pause()
        {
            m_PlaybackState = m_PlaybackState == PlaybackState.Paused ? PlaybackState.Playing : PlaybackState.Paused;
        }

        public void Init(IWaveProvider waveProvider)
        {
            OutputWaveProvider = waveProvider;
        }

        public void Dispose() { }
    }
}
