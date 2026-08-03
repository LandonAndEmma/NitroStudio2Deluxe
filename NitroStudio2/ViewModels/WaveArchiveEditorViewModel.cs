using GotaSoundIO.IO;
using GotaSoundIO.Sound.Formats;
using NitroFileLoader;
using NitroStudio2.Models;
using NitroStudio2.Services;
using NitroStudio2.ViewModels.Panels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NitroStudio2.ViewModels
{
    /// <summary>
    /// Editor for a .swar wave archive: a flat list of waves with a preview player.
    /// Ported from the WinForms WaveArchiveEditor.
    /// </summary>
    public sealed class WaveArchiveEditorViewModel : EditorViewModelBase, IDisposable
    {
        private readonly WavePlayback playback = new();

        /// <param name="archive">
        /// Unused by this editor, but accepted so every editor opens the same way from the
        /// sound archive window.
        /// </param>
        public WaveArchiveEditorViewModel(IDialogService dialogs, SoundArchive archive = null)
            : base(dialogs, typeof(WaveArchive), "Wave Archive", "war", "Wave Archive Editor")
        {
            SoundPlayerPanel = new SoundPlayerPanelViewModel(playback, "Sound Player Deluxe™")
            {
                PlayRequested = Play,
            };
            Nodes.Add(new EditorTreeNode("root", "Wave Archive", 5));
            UpdateNodes();
            Nodes[0].IsExpanded = true;
            DoInfoStuff();
        }

        public WaveArchive WA => File as WaveArchive;

        public BlankPanelViewModel BlankPanel { get; } = new();

        /// <summary>Opens a wave archive that lives inside a sound archive.</summary>
        public void LoadEmbedded(IOFile file, string fileName)
        {
            ExtFile = file;
            File = (IOFile)Activator.CreateInstance(file.GetType());
            File.Read(file.Write());
            FilePath = "";
            FileName = fileName;
            FileOpen = true;
            Title = EditorName + " - " + (fileName ?? "{ Null File Name }") + ".swar";
            UpdateNodes();
            DoInfoStuff();
        }

        public override void UpdateNodes()
        {
            BeginUpdateNodes();
            if (FileOpen && File is not null)
            {
                Nodes[0].ContextActions =
                    [new MenuAction("Add", "New", () => _ = AddWaveAsync(WA.Waves.Count))];
                for (int i = 0; i < WA.Waves.Count; i++)
                {
                    Nodes[0].Add("wave" + i, "Wave " + i, 14).ContextActions = NodeActions();
                }
            }
            else
            {
                foreach (EditorTreeNode node in Nodes)
                {
                    node.ContextActions = null;
                }
            }
            EndUpdateNodes();
        }

        /// <summary>The seven-item node menu, matching the WinForms nodeMenu.</summary>
        private IReadOnlyList<MenuAction> NodeActions() =>
        [
            new MenuAction("Add Above", "New", NodeAddAbove),
            new MenuAction("Add Below", "Open", NodeAddBelow),
            new MenuAction("Move Up", "Save", NodeMoveUp),
            new MenuAction("Move Down", null, NodeMoveDown),
            new MenuAction("Replace", null, NodeReplace),
            new MenuAction("Export", "Export", NodeExport),
            new MenuAction("Delete", "Close", NodeDelete),
        ];

        public override void DoInfoStuff()
        {
            if (!FileOpen || File is null || SelectedNode?.Parent is null)
            {
                ShowSoundPlayer = false;
                ActivePanel = NoInfoPanel;
                Status = "No Valid Info Selected!";
                return;
            }
            int index = SelectedNode.Index;
            ShowSoundPlayer = true;
            ActivePanel = BlankPanel;
            Status =
                "Wave "
                + index
                + " Selected. "
                + (WA.Waves[index].Loops ? "(Loops)" : "(Doesn't Loop)")
                + " File Is "
                + SoundArchiveViewModel.GetBytesSize(WA.Waves[index])
                + ".";
        }

        /// <summary>Space previews the selected wave.</summary>
        public override void PlaySelected() => Play();

        private void Play()
        {
            if (SelectedNode?.Parent is null)
            {
                return;
            }
            playback.LoadWave(WA.Waves[SelectedNode.Index]);
            playback.Play();
        }

        // ------------------------------------------------------------------ node actions

        public override void RootAdd() => _ = AddWaveAsync(WA.Waves.Count);

        public override void NodeAddAbove() => _ = AddWaveAsync(SelectedNode.Index);

        public override void NodeAddBelow() => _ = AddWaveAsync(SelectedNode.Index + 1);

        public override void NodeMoveUp()
        {
            int index = SelectedNode.Index;
            if (Swap(WA.Waves, index, index - 1))
            {
                UpdateNodes();
                SelectNode(index - 1);
            }
        }

        public override void NodeMoveDown()
        {
            int index = SelectedNode.Index;
            if (Swap(WA.Waves, index, index + 1))
            {
                UpdateNodes();
                SelectNode(index + 1);
            }
        }

        public override void NodeReplace() => _ = ReplaceWaveAsync();

        public override void NodeExport() => _ = ExportWaveAsync();

        public override void NodeDelete()
        {
            WA.Waves.RemoveAt(SelectedNode.Index);
            UpdateNodes();
            DoInfoStuff();
        }

        private void SelectNode(int index)
        {
            if (index >= 0 && index < Nodes[0].Nodes.Count)
            {
                SelectedNode = Nodes[0].Nodes[index];
            }
            DoInfoStuff();
        }

        /// <summary>Reads a WAV, SWAV or STRM into a .swav wave, or null if unsupported.</summary>
        private async Task<Wave> ReadWaveAsync(string path)
        {
            Wave w = new();
            switch (Path.GetExtension(path))
            {
                case ".wav":
                    w.FromOtherStreamFile(new RiffWave(path));
                    return w;
                case ".swav":
                    w.Read(path);
                    return w;
                case ".strm":
                    NitroFileLoader.Stream s = new();
                    s.Read(path);
                    w.FromOtherStreamFile(s);
                    return w;
                default:
                    await Dialogs.ShowMessageAsync("Unsupported file format!");
                    return null;
            }
        }

        private async Task AddWaveAsync(int index)
        {
            string path = await Dialogs.OpenFileAsync("Supported Audio Files|*.wav;*.swav;*.strm");
            if (path == "")
            {
                return;
            }
            Wave w = await ReadWaveAsync(path);
            if (w is null)
            {
                return;
            }
            WA.Waves.Insert(index, w);
            UpdateNodes();
            DoInfoStuff();
        }

        private async Task ReplaceWaveAsync()
        {
            string path = await Dialogs.OpenFileAsync("Supported Audio Files|*.wav;*.swav;*.strm");
            if (path == "")
            {
                return;
            }
            Wave w = await ReadWaveAsync(path);
            if (w is null)
            {
                return;
            }
            WA.Waves[SelectedNode.Index] = w;
            UpdateNodes();
            DoInfoStuff();
        }

        private async Task ExportWaveAsync()
        {
            int index = SelectedNode.Index;
            string path = await Dialogs.SaveFileAsync(
                "Supported Audio Files|*.wav;*.swav;*.strm|Wave|*.wav|Sound Wave|*.swav|Sound Stream|*.strm",
                "Wave " + index + ".swav"
            );
            if (path == "")
            {
                return;
            }
            Wave w = WA.Waves[index];
            switch (Path.GetExtension(path))
            {
                case ".wav":
                    RiffWave r = new();
                    r.FromOtherStreamFile(w);
                    r.Write(path);
                    break;
                case ".swav":
                    w.Write(path);
                    break;
                case ".strm":
                    NitroFileLoader.Stream stm = new();
                    stm.FromOtherStreamFile(w);
                    stm.Write(path);
                    break;
                default:
                    await Dialogs.ShowMessageAsync("Unsupported file format!");
                    break;
            }
        }

        public override void OnClosing()
        {
            Dispose();
            base.OnClosing();
        }

        public void Dispose() => playback.Dispose();
    }
}
