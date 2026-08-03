using GotaSequenceLib.Playback;
using GotaSoundBank.DLS;
using GotaSoundBank.SF2;
using GotaSoundIO.Sound.Formats;
using NitroFileLoader;
using NitroStudio2.Views;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NitroStudio2.ViewModels
{
    /// <summary>
    /// Replacing and exporting individual archive entries, plus sequence playback.
    /// Ported from MainWindow.Replace, MainWindow.Export and the transport handlers.
    /// </summary>
    public sealed partial class SoundArchiveViewModel
    {
        /// <summary>
        /// Directory holding the optional Nintendo SDK converters. WinForms read
        /// Application.StartupPath; on .NET that is the app base directory.
        /// </summary>
        private static string NitroPath => AppContext.BaseDirectory.TrimEnd('/', '\\');

        /// <summary>Opens a window to edit the selected sequence archive's contents.</summary>
        private void OpenSequenceArchiveFile()
        {
            // Wired up once the sequence archive editor is ported; the entry itself is resolved
            // here so the tree selection is the only input the editor needs.
            SequenceArchiveInfo f = (SequenceArchiveInfo)SelectedEntry();
            OpenSequenceArchiveRequested?.Invoke(f);
        }

        /// <summary>Set by the host so the archive editor window can be opened from here.</summary>
        public Action<SequenceArchiveInfo> OpenSequenceArchiveRequested { get; set; }

        /// <summary>Set by the host so exporting to WAV can show the recorder dialog.</summary>
        public Func<SequenceRecorderViewModel, Task> ShowRecorderRequested { get; set; }

        // ------------------------------------------------------------------ replace

        private async Task ReplaceAsync()
        {
            Category category = Selected();
            if (category is null)
            {
                return;
            }
            int ind = IdFromNode(SelectedNode);
            string filter = category.Key switch
            {
                "sequences" =>
                    "Supported Sound Files|*.sseq;*.smft;*.mid|Sound Sequence|*.sseq|SMF Text|*.smft|MIDI|*.mid",
                "sequenceArchives" =>
                    "Sequence Archive|*.ssar;*.mus|Sound Sequence Archive|*.ssar|Music List|*.mus",
                "banks" =>
                    "Supported Bank Files|*.sbnk;*.sf2;*.dls|Sound Bank|*.sbnk|Soundfont|*.sf2|Downloadable Sounds|*.dls",
                "waveArchives" => "Sound Wave Archive|*.swar",
                "streams" =>
                    "Supported Sound Files|*.strm;*.swav;*.wav|Stream|*.strm|Sound Wave|*.swav|Wave|*.wav",
                _ => "",
            };
            string path = await Dialogs.OpenFileAsync(filter);
            if (path == "")
            {
                return;
            }

            switch (System.IO.Path.GetExtension(path))
            {
                case ".sseq":
                {
                    SequenceInfo e = SA.Sequences.First(x => x.Index == ind);
                    e.File = new Sequence();
                    e.File.Read(path);
                    DoInfoStuff();
                    break;
                }
                case ".smft":
                {
                    SequenceInfo e = SA.Sequences.First(x => x.Index == ind);
                    e.File = new Sequence();
                    e.File.FromText([.. System.IO.File.ReadAllLines(path)]);
                    e.File.WriteCommandData();
                    break;
                }
                case ".mid":
                    if (!await ImportMidiAsync(ind, path))
                    {
                        return;
                    }
                    break;
                case ".ssar":
                {
                    SequenceArchiveInfo e = SA.SequenceArchives.First(x => x.Index == ind);
                    e.File = new SequenceArchive();
                    e.File.Read(path);
                    e.File.ReadCommandData(true);
                    e.File.FromText([.. e.File.ToText()], SA);
                    UpdateNodes();
                    DoInfoStuff();
                    break;
                }
                case ".mus":
                {
                    SequenceArchiveInfo e = SA.SequenceArchives.First(x => x.Index == ind);
                    e.File = new SequenceArchive();
                    e.File.FromText([.. System.IO.File.ReadAllLines(path)], SA);
                    e.File.WriteCommandData();
                    UpdateNodes();
                    DoInfoStuff();
                    break;
                }
                case ".sbnk":
                {
                    BankInfo e = SA.Banks.First(x => x.Index == ind);
                    e.File = new Bank();
                    e.File.Read(path);
                    DoInfoStuff();
                    break;
                }
                case ".sf2":
                    await ReplaceBankWithSoundFontAsync(
                        SA.Banks.First(x => x.Index == ind),
                        new SoundFont(path)
                    );
                    DoInfoStuff();
                    return;
                case ".dls":
                    await ReplaceBankWithDlsAsync(
                        SA.Banks.First(x => x.Index == ind),
                        new DownloadableSounds(path)
                    );
                    DoInfoStuff();
                    return;
                case ".swar":
                {
                    WaveArchiveInfo e = SA.WaveArchives.First(x => x.Index == ind);
                    e.File = new WaveArchive();
                    e.File.Read(path);
                    DoInfoStuff();
                    break;
                }
                case ".strm":
                {
                    StreamInfo e = SA.Streams.First(x => x.Index == ind);
                    e.File = new NitroFileLoader.Stream();
                    e.File.Read(path);
                    DoInfoStuff();
                    break;
                }
                case ".swav":
                {
                    StreamInfo e = SA.Streams.First(x => x.Index == ind);
                    e.File = new NitroFileLoader.Stream();
                    Wave swav = new();
                    swav.Read(path);
                    e.File.FromOtherStreamFile(swav);
                    DoInfoStuff();
                    break;
                }
                case ".wav":
                {
                    StreamInfo e = SA.Streams.First(x => x.Index == ind);
                    e.File = new NitroFileLoader.Stream();
                    RiffWave riff = new();
                    riff.Read(path);
                    e.File.FromOtherStreamFile(riff);
                    DoInfoStuff();
                    break;
                }
            }
        }

        /// <summary>
        /// MIDI import, honouring the Settings panel's import mode: the built-in converter, or
        /// one of the Nintendo SDK executables sitting next to the app.
        /// </summary>
        private async Task<bool> ImportMidiAsync(int ind, string path)
        {
            SequenceInfo e = SA.Sequences.First(x => x.Index == ind);
            switch (SettingsPanel.ImportMode)
            {
                case 0:
                    e.File = new Sequence();
                    e.File.FromMIDI(path);
                    return true;
                case 1:
                    if (!await RequireToolAsync("midi2sseq.exe"))
                    {
                        return false;
                    }
                    System.IO.File.Copy(path, "temp.mid", true);
                    RunTool("midi2sseq.exe", "temp.mid temp.sseq");
                    e.File = new Sequence();
                    e.File.Read("temp.sseq");
                    Delete("temp.mid", "temp.sseq");
                    return true;
                default:
                    if (
                        !await RequireToolAsync("smfconv.exe")
                        || !await RequireToolAsync("seqconv.exe")
                    )
                    {
                        return false;
                    }
                    System.IO.File.Copy(path, "temp.mid", true);
                    RunTool("smfconv.exe", "temp.mid");
                    RunTool("seqconv.exe", "temp.smft");
                    e.File = new Sequence();
                    e.File.Read("temp.sseq");
                    Delete("temp.mid", "temp.smft", "temp.sseq");
                    return true;
            }
        }

        private async Task<bool> RequireToolAsync(string tool)
        {
            if (System.IO.File.Exists(NitroPath + "/" + tool))
            {
                return true;
            }
            await Dialogs.ShowMessageAsync("Cannot find " + tool + "!");
            return false;
        }

        private static void RunTool(string tool, string arguments)
        {
            Process process = new();
            process.StartInfo.FileName = NitroPath + "/" + tool;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            process.WaitForExit();
        }

        private static void Delete(params string[] paths)
        {
            foreach (string path in paths)
            {
                try
                {
                    System.IO.File.Delete(path);
                }
                catch { }
            }
        }

        // ------------------------------------------------------------------ export

        private async Task ExportAsync()
        {
            if (SelectedNode is null)
            {
                return;
            }
            bool nested = SelectedNode.Parent?.Parent is not null;
            Category category = nested ? null : Selected();
            if (category is null && !nested)
            {
                return;
            }
            int ind = IdFromNode(SelectedNode);
            string baseName = SelectedNode.Text[(SelectedNode.Text.IndexOf(' ') + 1)..];

            const string sequenceFilter =
                "Supported Sound Files|*.sseq;*.smft;*.mid;*.wav|Sound Sequence|*.sseq|SMF Text|*.smft|MIDI|*.mid|Wave|*.wav";
            string filter;
            string suggested;
            if (nested)
            {
                filter = sequenceFilter;
                suggested = baseName + ".sseq";
            }
            else
            {
                (filter, string ext) = category.Key switch
                {
                    "sequences" => (sequenceFilter, ".sseq"),
                    "sequenceArchives" => (
                        "Sequence Archive|*.ssar;*.mus|Sound Sequence Archive|*.ssar|Music List|*.mus",
                        ".ssar"
                    ),
                    "banks" => (
                        "Supported Bank Files|*.sbnk;*.sf2;*.dls|Sound Bank|*.sbnk|Soundfont|*.sf2|Downloadable Sounds|*.dls",
                        ".sbnk"
                    ),
                    "waveArchives" => ("Sound Wave Archive|*.swar", ".swar"),
                    "streams" => (
                        "Supported Sound Files|*.strm;*.swav;*.wav|Stream|*.strm|Sound Wave|*.swav|Wave|*.wav",
                        ".strm"
                    ),
                    _ => ("", ""),
                };
                suggested = baseName + ext;
            }

            string path = await Dialogs.SaveFileAsync(filter, suggested);
            if (path == "")
            {
                return;
            }

            switch (System.IO.Path.GetExtension(path))
            {
                case ".sseq":
                    if (nested)
                    {
                        throw new NotImplementedException();
                    }
                    SA.Sequences.First(x => x.Index == ind).File.Write(path);
                    break;
                case ".smft":
                {
                    if (nested)
                    {
                        throw new NotImplementedException();
                    }
                    SequenceInfo e = SA.Sequences.First(x => x.Index == ind);
                    e.File.ReadCommandData();
                    e.File.Name = e.Name;
                    System.IO.File.WriteAllLines(path, e.File.ToText());
                    break;
                }
                case ".mid":
                {
                    if (nested)
                    {
                        throw new NotImplementedException();
                    }
                    SequenceInfo e = SA.Sequences.First(x => x.Index == ind);
                    if (SettingsPanel.ExportMode == 0)
                    {
                        e.File.SaveMIDI(path);
                    }
                    else
                    {
                        if (!await RequireToolAsync("sseq2midi.exe"))
                        {
                            return;
                        }
                        e.File.Write("temp.sseq");
                        RunTool("sseq2midi.exe", "temp.sseq");
                        if (System.IO.File.Exists(path) && path != "temp.mid")
                        {
                            System.IO.File.Delete(path);
                        }
                        System.IO.File.Move("temp.mid", path);
                        Delete("temp.sseq");
                    }
                    break;
                }
                case ".wav":
                    await ExportWavAsync(ind, path, nested);
                    break;
                case ".ssar":
                    SA.SequenceArchives.First(x => x.Index == ind).File.Write(path);
                    break;
                case ".mus":
                    ExportMusicList(ind, path);
                    break;
                case ".sbnk":
                    SA.Banks.First(x => x.Index == ind).File.Write(path);
                    break;
                case ".sf2":
                {
                    BankInfo e = SA.Banks.First(x => x.Index == ind);
                    e.File.ToSoundFont(SA, e).Write(path);
                    break;
                }
                case ".dls":
                {
                    BankInfo e = SA.Banks.First(x => x.Index == ind);
                    e.File.ToDLS(SA, e).Write(path);
                    break;
                }
                case ".swar":
                    SA.WaveArchives.First(x => x.Index == ind).File.Write(path);
                    break;
                case ".strm":
                    SA.Streams.First(x => x.Index == ind).File.Write(path);
                    break;
                case ".swav":
                {
                    Wave swav = new();
                    swav.FromOtherStreamFile(SA.Streams.First(x => x.Index == ind).File);
                    swav.Write(path);
                    break;
                }
            }
        }

        /// <summary>
        /// A stream exports its audio directly; a sequence is rendered by the recorder dialog,
        /// which needs the sequence's bank and waves.
        /// </summary>
        private async Task ExportWavAsync(int ind, string path, bool nested)
        {
            if (!nested && SelectedNode.Parent.Name == "streams")
            {
                RiffWave wav = new();
                wav.FromOtherStreamFile(SA.Streams.First(x => x.Index == ind).File);
                wav.Write(path);
                return;
            }

            if (!nested && SelectedNode.Parent.Name == "sequences")
            {
                SequenceInfo seq = SA.Sequences.First(x => x.Index == ind);
                seq.File.ReadCommandData();
                try
                {
                    SequenceRecorderViewModel recorder = new(
                        [seq.Bank.File],
                        seq.Bank.GetAssociatedWaves(),
                        seq.File.Commands,
                        0,
                        path
                    );
                    await ShowRecorder(recorder);
                }
                catch
                {
                    await Dialogs.ShowMessageAsync(
                        "Sequence entry has no valid bank hooked up to it!"
                    );
                }
                return;
            }

            SequenceArchiveInfo archive = SA.SequenceArchives.First(x =>
                x.Index == IdFromNode(SelectedNode.Parent)
            );
            SequenceArchiveSequence inner = archive.File.Sequences.First(x => x.Index == ind);
            archive.File.ReadCommandData(true);
            try
            {
                SequenceRecorderViewModel recorder = new(
                    [inner.Bank.File],
                    inner.Bank.GetAssociatedWaves(),
                    archive.File.Commands,
                    archive.File.PublicLabels.Values.ElementAt(
                        archive.File.Sequences.IndexOf(inner)
                    ),
                    path
                );
                await ShowRecorder(recorder);
            }
            catch
            {
                await Dialogs.ShowMessageAsync("Sequence entry has no valid bank hooked up to it!");
            }
        }

        private Task ShowRecorder(SequenceRecorderViewModel recorder) =>
            ShowRecorderRequested?.Invoke(recorder) ?? Task.CompletedTask;

        /// <summary>
        /// Round-trips a sequence archive through a fresh parse so its labels line up with the
        /// sequence names, then writes it as a music list. Port of the ".mus" export branch.
        /// </summary>
        private void ExportMusicList(int ind, string path)
        {
            SequenceArchiveInfo info = SA.SequenceArchives.First(x => x.Index == ind);
            SequenceArchive other = info.File;
            SequenceArchive rebuilt = new();
            rebuilt.Read(other.Write());
            for (int i = 0; i < rebuilt.Sequences.Count; i++)
            {
                rebuilt.Sequences[i].Name = other.Sequences[i].Name;
                rebuilt.Sequences[i].Bank = other.Sequences[i].Bank;
                rebuilt.Sequences[i].Player = other.Sequences[i].Player;
            }
            uint[] values = [.. rebuilt.Labels.Values];
            string[] previousNames = [.. rebuilt.Labels.Keys];
            rebuilt.Labels = [];
            int valueIndex = 0;
            foreach (SequenceArchiveSequence s in rebuilt.Sequences)
            {
                rebuilt.Labels.Add(s.Name ?? previousNames[valueIndex], values[valueIndex++]);
            }
            rebuilt.ReadCommandData(true);
            rebuilt.Name = info.Name;
            System.IO.File.WriteAllLines(path, rebuilt.ToText());
        }

        // ------------------------------------------------------------------ playback

        /// <summary>
        /// Plays whichever sequence the tree has selected, from the archive or from inside a
        /// sequence archive. Port of MainWindow.PlayClick.
        /// </summary>
        private void Play() => _ = PlayAsync();

        /// <summary>Space previews a sequence, or one inside a sequence archive.</summary>
        public override void PlaySelected()
        {
            if (SelectedNode?.Parent is null)
            {
                return;
            }
            if (SelectedNode.Parent.Name == "streams")
            {
                PlayStream();
            }
            else if (SelectedNode.Parent.Parent is not null || SelectedNode.Parent.Name == "sequences")
            {
                Play();
            }
        }

        private async Task PlayAsync()
        {
            if (SelectedNode?.Parent is null)
            {
                return;
            }

            if (SelectedNode.Parent.Name == "sequences")
            {
                SequenceInfo s = SA.Sequences.First(x => x.Index == IdFromNode(SelectedNode));
                s.File.ReadCommandData();
                if (
                    !playback.LoadSong(
                        s.Bank?.File,
                        s.Bank?.GetAssociatedWaves(),
                        s.File.Commands
                    )
                )
                {
                    await Dialogs.ShowMessageAsync(
                        "Sequence entry has no valid bank hooked up to it!"
                    );
                    return;
                }
                playback.Play();
                return;
            }

            if (SelectedNode.Parent.Parent is null)
            {
                return;
            }

            SequenceArchiveInfo a = SA.SequenceArchives.First(x =>
                x.Index == IdFromNode(SelectedNode.Parent)
            );
            SequenceArchiveSequence seq = a.File.Sequences.First(x =>
                x.Index == IdFromNode(SelectedNode)
            );
            a.File.ReadCommandData(true);
            if (
                !playback.LoadSong(
                    seq.Bank?.File,
                    seq.Bank?.GetAssociatedWaves(),
                    a.File.Commands,
                    a.File.PublicLabels.Values.ElementAt(a.File.Sequences.IndexOf(seq))
                )
            )
            {
                await Dialogs.ShowMessageAsync(
                    "Sequence Archive entry has no valid bank hooked up to it!"
                );
                return;
            }
            playback.Play();
        }
    }
}
