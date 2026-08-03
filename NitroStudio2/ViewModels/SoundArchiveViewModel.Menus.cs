using GotaSoundIO.IO;
using NitroFileLoader;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NitroStudio2.ViewModels
{
    /// <summary>File and Tools menu behaviour specific to the sound archive window.</summary>
    public sealed partial class SoundArchiveViewModel
    {
        /// <summary>Adopts an already-loaded archive. Used by tests and by in-memory flows.</summary>
        public void LoadArchive(SoundArchive archive, string name)
        {
            File = archive;
            FilePath = "";
            FileName = name;
            Title = EditorName + " - " + name + ".sdat";
            FileOpen = true;
            UpdateNodes();
            DoInfoStuff();
        }

        /// <summary>Opens the archive named on the command line, as Program.Main did.</summary>
        public void OpenStartupFile(string path)
        {
            File = (IOFile)Activator.CreateInstance(FileType);
            FilePath = path;
            FileName = Path.GetFileNameWithoutExtension(path);
            Title = EditorName + " - " + Path.GetFileName(path);
            FileOpen = true;
            File.Read(path);
            UpdateNodes();
            DoInfoStuff();
        }

        /// <summary>The sound archive window keeps its own name in the title bar when empty.</summary>
        public override async Task NewAsync()
        {
            await base.NewAsync();
            if (FileOpen)
            {
                Title = "Nitro Studio 2";
            }
        }

        public override async Task ImportFileAsync()
        {
            if (!await FileTestAsync(false, true))
            {
                return;
            }
            string path = await Dialogs.OpenFileAsync(
                "Sound Archive|*.sdat;*.dsxe|All Files|*.*"
            );
            if (path == "")
            {
                return;
            }
            File = (IOFile)Activator.CreateInstance(FileType);
            File.Read(path);
            UpdateNodes();
            DoInfoStuff();
        }

        public override async Task ExportFileAsync()
        {
            if (!await FileTestAsync(false, true))
            {
                return;
            }
            string path = await Dialogs.SaveFileAsync(
                "Sound Archive|*.sdat;*.dsxe|All Files|*.*",
                FileName
            );
            if (path != "")
            {
                SA.Write(path);
            }
        }

        /// <summary>Writes the archive out as an SDK sound project. Port of the Tools entry.</summary>
        public async Task ExportSdkProjectAsync()
        {
            string suggested = string.IsNullOrEmpty(FilePath)
                ? null
                : Path.GetFileNameWithoutExtension(FilePath) + ".sprj";
            string path = await Dialogs.SaveFileAsync("Sound Project|*.sprj", suggested);
            if (path == "")
            {
                return;
            }
            SA.ExportSDKProject(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
        }

        /// <summary>
        /// Double-clicking a sequence, sequence archive, bank or wave archive opens it in its
        /// own editor. Port of MainWindow.NodeMouseDoubleClick.
        /// </summary>
        public override void NodeMouseDoubleClick()
        {
            if (SelectedNode?.Parent is null || OpenEntryEditorRequested is null)
            {
                return;
            }
            if (SelectedNode.Parent.Parent is not null)
            {
                if (SelectedNode.Parent.Parent.Name == "sequenceArchives")
                {
                    OpenEntryEditorRequested(
                        SA.SequenceArchives.Find(x => x.Index == IdFromNode(SelectedNode.Parent))
                    );
                }
                return;
            }
            // A stream has no editor. The WinForms build rendered it to a temporary WAV and
            // opened a separate player window for it; the transport in the left pane plays the
            // .strm directly, so double-clicking just starts it.
            if (SelectedNode.Parent.Name == "streams")
            {
                PlayStream();
                return;
            }
            object entry = SelectedEntry();
            if (entry is not null)
            {
                OpenEntryEditorRequested(entry);
            }
        }

        /// <summary>
        /// Set by the host: opens the editor matching an archive entry (sequence, sequence
        /// archive, bank or wave archive).
        /// </summary>
        public Action<object> OpenEntryEditorRequested { get; set; }
    }
}
