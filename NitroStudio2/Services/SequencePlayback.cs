using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using GotaSequenceLib;
using GotaSequenceLib.Playback;
using GotaSoundIO.Sound;
using GotaSoundIO.Sound.Formats;
using System;
using System.Collections.Generic;

namespace NitroStudio2.Services
{
    /// <summary>
    /// Bindable transport behind the sound player panel, whichever engine drives it: the
    /// sequence player in the archive, sequence and bank editors, or the stream player in the
    /// wave archive editor. Replaces the per-editor timer and slider wiring the WinForms forms
    /// each set up by hand.
    /// </summary>
    public abstract class PlaybackTransport : ObservableObject, IDisposable
    {

        /// <summary>
        /// False while the user is dragging the position bar, so the tick stops fighting them.
        /// Mirrors the PositionBarFree flag the editors kept.
        /// </summary>
        public bool PositionBarFree { get; set; } = true;

        public long Position
        {
            get;
            set => SetProperty(ref field, value);
        }

        public long MaxPosition
        {
            get;
            set => SetProperty(ref field, Math.Max(1, value));
        } = 1;

        /// <summary>Volume as the 0-100 the slider used; engines scale it as they need.</summary>
        public int Volume
        {
            get;
            set
            {
                if (SetProperty(ref field, value))
                {
                    OnVolumeChanged(value);
                }
            }
        } = 75;

        public bool Loop
        {
            get;
            set
            {
                if (SetProperty(ref field, value))
                {
                    OnLoopChanged(value);
                }
            }
        }

        protected abstract void OnVolumeChanged(int value);

        protected abstract void OnLoopChanged(bool value);

        public abstract void Play();

        public abstract void Pause();

        public abstract void Stop();

        /// <summary>Seeks to the current <see cref="Position"/>, once a drag has finished.</summary>
        public abstract void SeekToPosition();

        public abstract void Dispose();
    }

    /// <summary>Owns the Mixer, Player and 30 fps position tick used for sequence playback.</summary>
    public sealed class SequencePlayback : PlaybackTransport
    {
        private readonly DispatcherTimer timer;

        public SequencePlayback()
        {
            Mixer = new Mixer();
            Player = new Player(Mixer);
            Mixer.Volume = Volume / 100f;

            // WinForms used a Forms.Timer at 1000/30 ms to drag the position bar along.
            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000 / 30) };
            timer.Tick += (_, _) => Tick();
            timer.Start();
        }

        public Mixer Mixer { get; }

        public Player Player { get; }

        /// <summary>
        /// Loads a bank and its waves, then the song itself. Returns false when the entry has no
        /// usable bank, which is the case every caller reported with a message box.
        /// </summary>
        public bool LoadSong(
            PlayableBank bank,
            RiffWave[][] waveArchives,
            List<SequenceCommand> commands,
            int startOffset = 0
        )
        {
            try
            {
                Player.PrepareForSong([bank], waveArchives);
            }
            catch
            {
                return false;
            }
            Player.LoadSong(commands, startOffset);
            MaxPosition = Player.MaxTicks;
            return true;
        }

        protected override void OnVolumeChanged(int value)
        {
            Mixer.Volume = value / 100f;
        }

        protected override void OnLoopChanged(bool value)
        {
            Player.NumLoops = value ? 0xFFFFFFFF : 0;
        }

        public override void Play()
        {
            Player.Play();
        }

        public override void Pause()
        {
            Player.Pause();
        }

        public override void Stop()
        {
            Player.Stop();
        }

        public override void SeekToPosition()
        {
            if (Player?.Events is not null)
            {
                Player.SetCurrentPosition(Position);
            }
            PositionBarFree = true;
        }

        private void Tick()
        {
            if (Player is null || !PositionBarFree)
            {
                return;
            }
            long current = Player.GetCurrentPosition();
            Position = current > MaxPosition ? MaxPosition : current;
        }

        public override void Dispose()
        {
            timer.Stop();
            try
            {
                Player.Stop();
                Player.Dispose();
            }
            catch { }
            try
            {
                Mixer.Dispose();
            }
            catch { }
        }
    }

    /// <summary>
    /// Plays individual waves out of a wave archive. The WinForms wave archive editor ticked at
    /// 10 ms rather than 30 fps, because a single sample's position moves much faster.
    /// </summary>
    public sealed class WavePlayback : PlaybackTransport
    {
        private readonly DispatcherTimer timer;

        public WavePlayback()
        {
            Player = new GotaSoundIO.Sound.Playback.StreamPlayer();
            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
            timer.Tick += (_, _) => Tick();
            timer.Start();
        }

        public GotaSoundIO.Sound.Playback.StreamPlayer Player { get; }

        /// <summary>Loads one wave and sizes the position bar to it.</summary>
        public void LoadWave(SoundFile wave)
        {
            Player.Stop();
            Player.LoadStream(wave);
            MaxPosition = Player.GetLength();
        }

        // The wave archive editor's volume slider was inert in WinForms; the stream player has
        // no volume control, so it stays inert here too.
        protected override void OnVolumeChanged(int value) { }

        protected override void OnLoopChanged(bool value)
        {
            Player.Loop = value;
        }

        public override void Play()
        {
            Player.Play();
        }

        public override void Pause()
        {
            Player.Pause();
        }

        public override void Stop()
        {
            Player.Stop();
        }

        public override void SeekToPosition()
        {
            Player.SetPosition((uint)Position);
            PositionBarFree = true;
        }

        private void Tick()
        {
            if (!PositionBarFree)
            {
                return;
            }
            long current = Player.GetPosition();
            Position = current > MaxPosition ? MaxPosition : current;
        }

        public override void Dispose()
        {
            timer.Stop();
            try
            {
                Player.Dispose();
            }
            catch { }
        }
    }
}
