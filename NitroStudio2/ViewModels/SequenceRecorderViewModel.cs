using CommunityToolkit.Mvvm.Input;
using GotaSequenceLib;
using GotaSequenceLib.Playback;
using GotaSoundIO.Sound.Formats;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace NitroStudio2.ViewModels
{
    /// <summary>
    /// Renders a sequence to a WAV file. Ported from the WinForms SequenceRecorder form: the
    /// loop count and fade-out flag are handed straight to the player before Record runs.
    /// </summary>
    public sealed class SequenceRecorderViewModel : ViewModelBase, IDisposable
    {
        private readonly Mixer mixer = new();
        private readonly Player player;
        private readonly List<SequenceCommand> commands;
        private readonly int seqStart;
        private readonly string filePath;

        private decimal loops = 1;
        private bool fadeOut = true;

        public SequenceRecorderViewModel(
            PlayableBank[] banks,
            RiffWave[][] wars,
            List<SequenceCommand> commands,
            int startIndex,
            string filePath
        )
        {
            player = new Player(mixer);
            player.PrepareForSong(banks, wars);
            this.commands = commands;
            seqStart = startIndex;
            this.filePath = filePath;
            ExportCommand = new RelayCommand(Export);
        }

        /// <summary>Raised once the export finishes, so the view can close itself.</summary>
        public event EventHandler Finished;

        public decimal Loops
        {
            get => loops;
            set => SetProperty(ref loops, value);
        }

        public bool FadeOut
        {
            get => fadeOut;
            set => SetProperty(ref fadeOut, value);
        }

        public ICommand ExportCommand { get; }

        private void Export()
        {
            player.LoadSong(commands, seqStart);
            player.NumLoops = (long)Loops;
            player.DontFadeSong = !FadeOut;
            player.Record(filePath);
            Finished?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            try
            {
                player.Dispose();
                mixer.Dispose();
            }
            catch { }
        }
    }
}
