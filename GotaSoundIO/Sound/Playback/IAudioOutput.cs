using NAudio.Wave;
using System;

namespace GotaSoundIO.Sound.Playback
{
    /// <summary>
    /// An audio output device. Deliberately shaped like NAudio's IWavePlayer, which this replaces,
    /// so the call sites read the same; the difference is that implementations here work on every
    /// platform rather than only Windows.
    ///
    /// NAudio is still used for the wave formats and file readers feeding this, which are managed
    /// and platform-neutral. Only the device hand-off changed.
    /// </summary>
    public interface IAudioOutput : IDisposable
    {
        /// <summary>Linear gain, 0 to 1.</summary>
        float Volume { get; set; }

        PlaybackState PlaybackState { get; }

        /// <summary>Attaches the source this device pulls samples from.</summary>
        void Init(IWaveProvider waveProvider);

        void Play();

        void Pause();

        void Stop();
    }
}
