using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GotaSoundIO.Sound.Playback {
    public class NullWavePlayer : IWavePlayer {
        public float Volume { get => m_Volume; set => m_Volume = value; }
        private float m_Volume = 1f;
        public IWaveProvider OutputWaveProvider { get; set; }
        public WaveFormat OutputWaveFormat => OutputWaveProvider?.WaveFormat;
        public PlaybackState PlaybackState => throw new NotImplementedException();
        private PlaybackState m_PlaybackState;
#pragma warning disable CS0067
        public event EventHandler<StoppedEventArgs> PlaybackStopped;
#pragma warning restore CS0067
        public void Play() {
            m_PlaybackState = PlaybackState.Playing;
        }
        public void Stop() {
            m_PlaybackState = PlaybackState.Stopped;
        }
        public void Pause() {
            if (m_PlaybackState == PlaybackState.Paused) {
                m_PlaybackState = PlaybackState.Playing;
            } else {
                m_PlaybackState = PlaybackState.Paused;
            }
        }
        public void Init(IWaveProvider waveProvider) {
            OutputWaveProvider = waveProvider;
        }
        public void Dispose() {}
    }
}
