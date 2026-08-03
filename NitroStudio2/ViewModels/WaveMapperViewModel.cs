using CommunityToolkit.Mvvm.Input;
using GotaSoundIO.Sound.Formats;
using GotaSoundIO.Sound.Playback;
using NitroFileLoader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace NitroStudio2.ViewModels
{
    /// <summary>One wave awaiting an archive assignment.</summary>
    public sealed class WaveMapRow : ViewModelBase
    {
        private string waveArchive;

        public WaveMapRow(int waveId, string waveArchive)
        {
            WaveId = waveId;
            this.waveArchive = waveArchive;
        }

        public int WaveId { get; }

        /// <summary>Entry text of the chosen archive, formatted "[index] name".</summary>
        public string WaveArchive
        {
            get => waveArchive;
            set => SetProperty(ref waveArchive, value);
        }
    }

    /// <summary>
    /// Asks which wave archive each imported wave should land in. Ported from the WinForms
    /// WaveMapper form; <see cref="WarMap"/> stays null until Finished is pressed, which is how
    /// callers tell a cancelled dialog from a completed one.
    /// </summary>
    public sealed class WaveMapperViewModel : ViewModelBase, IDisposable
    {
        private readonly List<RiffWave> waves;
        private readonly StreamPlayer player = new();

        public WaveMapperViewModel(
            List<RiffWave> waves,
            List<WaveArchiveInfo> waveArchives,
            bool hideId = false
        )
        {
            this.waves = waves;
            HideWaveId = hideId;
            WaveArchiveOptions = [.. waveArchives.Select(w => "[" + w.Index + "] " + w.Name)];

            int id = 0;
            foreach (RiffWave _ in waves)
            {
                Rows.Add(new WaveMapRow(id++, WaveArchiveOptions[0]));
            }

            PlayCommand = new RelayCommand<WaveMapRow>(Play);
            FinishedCommand = new RelayCommand(Finish);
        }

        /// <summary>Chosen archive index per wave, or null while the dialog is still open.</summary>
        public List<ushort> WarMap { get; private set; }

        public ObservableCollection<WaveMapRow> Rows { get; } = [];

        public IReadOnlyList<string> WaveArchiveOptions { get; }

        /// <summary>Callers importing a single wave hide the id column, as the form did.</summary>
        public bool HideWaveId { get; }

        public ICommand PlayCommand { get; }

        public ICommand FinishedCommand { get; }

        /// <summary>Raised when Finished is pressed, so the view can close.</summary>
        public event EventHandler Finished;

        private void Play(WaveMapRow row)
        {
            if (row is null)
            {
                return;
            }
            player.Stop();
            player.LoadStream(waves[row.WaveId]);
            player.Play();
        }

        private void Finish()
        {
            WarMap = [];
            foreach (WaveMapRow row in Rows)
            {
                WarMap.Add(ushort.Parse(row.WaveArchive.Split(']')[0].Split('[')[1]));
            }
            Finished?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            try
            {
                player.Dispose();
            }
            catch { }
        }
    }
}
