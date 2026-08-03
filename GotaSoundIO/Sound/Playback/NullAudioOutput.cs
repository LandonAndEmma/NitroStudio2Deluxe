using NAudio.Wave;

namespace GotaSoundIO.Sound.Playback
{
    /// <summary>
    /// A device that consumes nothing and makes no sound. Used when no audio hardware can be
    /// opened, so the rest of the app — sequence timing, note events, rendering to file — keeps
    /// working on machines with no sound card, in containers, and under the headless tests.
    /// </summary>
    public class NullAudioOutput : IAudioOutput
    {
        public float Volume { get; set; } = 1f;

        public IWaveProvider OutputWaveProvider { get; private set; }

        public PlaybackState PlaybackState { get; private set; } = PlaybackState.Stopped;

        public void Init(IWaveProvider waveProvider)
        {
            OutputWaveProvider = waveProvider;
        }

        public void Play()
        {
            PlaybackState = PlaybackState.Playing;
        }

        public void Stop()
        {
            PlaybackState = PlaybackState.Stopped;
        }

        public void Pause()
        {
            PlaybackState =
                PlaybackState == PlaybackState.Paused
                    ? PlaybackState.Playing
                    : PlaybackState.Paused;
        }

        public void Dispose() { }
    }
}
