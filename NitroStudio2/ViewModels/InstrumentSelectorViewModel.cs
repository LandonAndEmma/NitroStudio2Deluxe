using CommunityToolkit.Mvvm.Input;
using GotaSequenceLib;
using GotaSequenceLib.Playback;
using GotaSoundIO.Sound.Formats;
using NitroFileLoader;
using NitroFileLoader.Instrument;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace NitroStudio2.ViewModels
{
    /// <summary>One instrument offered for import, with its tick box.</summary>
    public sealed class InstrumentRow : ViewModelBase
    {
        public InstrumentRow(int index, int instrumentId, string name)
        {
            Index = index;
            InstrumentId = instrumentId;
            Name = name;
        }

        /// <summary>Row position, used to pick the matching preview wave.</summary>
        public int Index { get; }

        public int InstrumentId { get; }

        public string Name { get; }

        public bool Use
        {
            get;
            set => SetProperty(ref field, value);
        } = true;
    }

    /// <summary>
    /// Lets the user pick which instruments to bring across during a bank import. Ported from
    /// the WinForms InstrumentSelector form, including its one-note preview player: a throwaway
    /// bank with a single direct instrument is prepared once, and previewing a row just swaps
    /// the wave sitting in archive slot 0.
    /// </summary>
    public sealed class InstrumentSelectorViewModel : ViewModelBase, IDisposable
    {
        private readonly List<RiffWave> waves;
        private readonly Mixer mixer = new();
        private readonly Player player;

        public InstrumentSelectorViewModel(
            List<RiffWave> waves,
            List<int> instruments,
            List<string> names
        )
        {
            this.waves = waves;
            player = new Player(mixer);
            player.PrepareForSong(
                [
                    new Bank
                    {
                        Instruments =
                        [
                            new DirectInstrument
                            {
                                Index = 0,
                                NoteInfo = [new NoteInfo { Key = Notes.gn9 }],
                            },
                        ],
                    },
                ],
                [new RiffWave[1]]
            );
            player.LoadSong(
                [
                    new SequenceCommand
                    {
                        CommandType = SequenceCommands.ProgramChange,
                        Parameter = (uint)0,
                    },
                    new SequenceCommand
                    {
                        CommandType = SequenceCommands.Note,
                        Parameter = new NoteParameter
                        {
                            Note = Notes.cn4,
                            Length = 48 * 2,
                            Velocity = 127,
                        },
                    },
                    new SequenceCommand { CommandType = SequenceCommands.Fin },
                ]
            );

            int index = 0;
            foreach (int instrument in instruments)
            {
                string name = "Instrument " + instrument;
                try
                {
                    name = names[index];
                }
                catch { }
                Rows.Add(new InstrumentRow(index, instrument, name));
                index++;
            }

            PlayCommand = new RelayCommand<InstrumentRow>(Play);
            FinishedCommand = new RelayCommand(Finish);
            CheckAllCommand = new RelayCommand(() => SetAll(true));
            UncheckAllCommand = new RelayCommand(() => SetAll(false));
        }

        /// <summary>Ids the user kept, or null while the dialog is still open.</summary>
        public List<int> SelectedInstruments { get; private set; }

        public ObservableCollection<InstrumentRow> Rows { get; } = [];

        public ICommand PlayCommand { get; }

        public ICommand FinishedCommand { get; }

        public ICommand CheckAllCommand { get; }

        public ICommand UncheckAllCommand { get; }

        public event EventHandler Finished;

        private void Play(InstrumentRow row)
        {
            if (row is null)
            {
                return;
            }
            player.Stop();
            player.WaveArchives[0][0] = waves[row.Index];
            player.Play();
        }

        private void SetAll(bool value)
        {
            foreach (InstrumentRow row in Rows)
            {
                row.Use = value;
            }
        }

        private void Finish()
        {
            SelectedInstruments = [.. Rows.Where(r => r.Use).Select(r => r.InstrumentId)];
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
