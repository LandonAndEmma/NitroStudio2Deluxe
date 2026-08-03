using NAudio.Wave;
using Silk.NET.OpenAL;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GotaSoundIO.Sound.Playback
{
    /// <summary>
    /// Audio output through OpenAL, used on every platform.
    ///
    /// This replaces NAudio's WasapiOut and WaveOutEvent, which are Windows-only wrappers. Using
    /// one backend everywhere rather than a different one per OS is what makes the output match:
    /// the samples were already identical, since the mixer and decoders are pure managed code, so
    /// the device hand-off was the only place platforms could diverge.
    ///
    /// Everything is resampled to <see cref="OutputRate"/> before it reaches OpenAL. Opening the
    /// device at whatever rate it prefers would be marginally better quality on odd hardware, but
    /// then two machines with different sound cards would produce different bytes.
    ///
    /// The device and context are shared process-wide (see <see cref="OpenAlDevice"/>); this class
    /// owns only its own source and buffers, so several editors can play at the same time.
    /// </summary>
    public sealed class OpenAlOutput : IAudioOutput
    {
        /// <summary>The rate every platform outputs at. See the class remarks.</summary>
        public const int OutputRate = 48000;

        /// <summary>Buffers kept queued on the source, and how much audio each holds.</summary>
        private const int BufferCount = 6;

        private const int BufferMilliseconds = 20;

        private readonly AL al;
        private readonly uint source;
        private readonly uint[] buffers;
        private readonly Queue<uint> available = new();

        /// <summary>Shared with every other output, because they all use one OpenAL context.</summary>
        private static object Gate => OpenAlDevice.Gate;

        private Thread feeder;
        private volatile bool running;
        private IWaveProvider provider;
        private Resampler resampler;
        private byte[] readBuffer = [];
        private short[] sampleBuffer = [];

        /// <summary>
        /// Takes a source on the shared device. Throws if no device is available, which is what
        /// the callers catch to fall back to <see cref="NullAudioOutput"/>.
        /// </summary>
        public OpenAlOutput()
        {
            OpenAlDevice.Ensure();
            al = OpenAlDevice.Al;
            lock (Gate)
            {
                source = al.GenSource();
                buffers = al.GenBuffers(BufferCount);
            }
            foreach (uint buffer in buffers)
            {
                available.Enqueue(buffer);
            }
        }

        public float Volume
        {
            get;
            set
            {
                field = Math.Clamp(value, 0f, 1f);
                lock (Gate)
                {
                    al.SetSourceProperty(source, SourceFloat.Gain, field);
                }
            }
        } = 1f;

        public PlaybackState PlaybackState { get; private set; } = PlaybackState.Stopped;

        public void Init(IWaveProvider waveProvider)
        {
            WaveFormat format = waveProvider.WaveFormat;
            if (format.BitsPerSample != 16)
            {
                throw new NotSupportedException(
                    $"Only 16-bit PCM is supported, got {format.BitsPerSample}-bit."
                );
            }

            lock (Gate)
            {
                provider = waveProvider;
                resampler = new Resampler(format.SampleRate, format.Channels, OutputRate);
            }
        }

        public void Play()
        {
            if (provider is null)
            {
                return;
            }
            PlaybackState = PlaybackState.Playing;
            lock (Gate)
            {
                al.SourcePlay(source);
            }
            StartFeeder();
        }

        public void Pause()
        {
            lock (Gate)
            {
                if (PlaybackState == PlaybackState.Paused)
                {
                    PlaybackState = PlaybackState.Playing;
                    al.SourcePlay(source);
                }
                else if (PlaybackState == PlaybackState.Playing)
                {
                    PlaybackState = PlaybackState.Paused;
                    al.SourcePause(source);
                }
            }
        }

        public void Stop()
        {
            PlaybackState = PlaybackState.Stopped;
            lock (Gate)
            {
                al.SourceStop(source);
                Recycle(all: true);
                resampler?.Reset();
            }
        }

        private void StartFeeder()
        {
            if (feeder is not null)
            {
                return;
            }
            running = true;
            feeder = new Thread(Feed)
            {
                IsBackground = true,
                Name = "OpenAL feeder",
            };
            feeder.Start();
        }

        /// <summary>
        /// Keeps the source's queue topped up. OpenAL plays from a ring of queued buffers and
        /// reports how many it has finished with, so the loop recycles those and refills them.
        ///
        /// Reading and resampling happen outside the shared lock. Every editor's feeder thread
        /// contends for that lock, so holding it across a resample made playback stutter once
        /// more than one window was open.
        /// </summary>
        private void Feed()
        {
            while (running)
            {
                try
                {
                    if (PlaybackState != PlaybackState.Playing)
                    {
                        Thread.Sleep(5);
                        continue;
                    }

                    uint buffer;
                    lock (Gate)
                    {
                        Recycle(all: false);
                        if (available.Count == 0)
                        {
                            RestartIfStarved();
                            Thread.Sleep(5);
                            continue;
                        }
                        buffer = available.Dequeue();
                    }

                    short[] output = ReadAndResample();
                    lock (Gate)
                    {
                        if (output is null || output.Length == 0)
                        {
                            available.Enqueue(buffer);
                        }
                        else
                        {
                            al.BufferData(buffer, BufferFormat.Stereo16, output, OutputRate);
                            al.SourceQueueBuffers(source, [buffer]);
                        }
                        RestartIfStarved();
                    }

                    if (output is null || output.Length == 0)
                    {
                        Thread.Sleep(2);
                    }
                }
                catch
                {
                    // A device disappearing mid-playback should silence the app, not crash it.
                    Thread.Sleep(5);
                }
            }
        }

        /// <summary>An underrun stops the source even though we still want to play; restart it.</summary>
        private void RestartIfStarved()
        {
            al.GetSourceProperty(source, GetSourceInteger.SourceState, out int state);
            if ((SourceState)state != SourceState.Playing && available.Count < BufferCount)
            {
                al.SourcePlay(source);
            }
        }

        /// <summary>Takes back buffers OpenAL has finished with.</summary>
        private void Recycle(bool all)
        {
            al.GetSourceProperty(source, GetSourceInteger.BuffersProcessed, out int processed);
            if (all)
            {
                al.GetSourceProperty(source, GetSourceInteger.BuffersQueued, out int queued);
                processed = queued;
            }
            for (int i = 0; i < processed; i++)
            {
                uint[] one = new uint[1];
                al.SourceUnqueueBuffers(source, one);
                available.Enqueue(one[0]);
            }
        }

        /// <summary>
        /// Pulls one buffer's worth from the provider and resamples it. Runs without the shared
        /// lock held, so it must not touch any AL state.
        /// </summary>
        private short[] ReadAndResample()
        {
            IWaveProvider current = provider;
            Resampler currentResampler = resampler;
            if (current is null || currentResampler is null)
            {
                return null;
            }

            WaveFormat format = current.WaveFormat;
            int frames = format.SampleRate * BufferMilliseconds / 1000;
            int bytes = frames * format.Channels * 2;
            if (readBuffer.Length < bytes)
            {
                readBuffer = new byte[bytes];
                sampleBuffer = new short[bytes / 2];
            }

            int read = current.Read(readBuffer, 0, bytes);
            if (read <= 0)
            {
                return null;
            }
            Buffer.BlockCopy(readBuffer, 0, sampleBuffer, 0, read);
            return currentResampler.Process(sampleBuffer.AsSpan(0, read / 2));
        }

        public void Dispose()
        {
            running = false;
            _ = (feeder?.Join(500));
            feeder = null;
            // Only this output's source and buffers go; the device and context are shared with
            // every other editor and stay open for the life of the process.
            lock (Gate)
            {
                try
                {
                    al.SourceStop(source);
                    Recycle(all: true);
                    al.DeleteSource(source);
                    al.DeleteBuffers(buffers);
                }
                catch { }
            }
        }
    }
}
