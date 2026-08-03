using CommunityToolkit.Mvvm.Input;
using GotaSoundIO.IO;
using NitroStudio2.Models;
using NitroStudio2.Services;
using NitroStudio2.ViewModels.Panels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NitroStudio2.ViewModels
{
    /// <summary>Which control fills the right-hand pane.</summary>
    public enum RightPaneMode
    {
        Tree,
        SequenceEditor,
    }

    /// <summary>
    /// Shared behaviour of every editor window, ported from the WinForms EditorBase: the file
    /// menu, the tree with its expansion/selection preservation, the info panel on the left, and
    /// the virtual hooks each concrete editor overrides.
    /// </summary>
    public abstract class EditorViewModelBase : ViewModelBase
    {
        private object activePanel;
        private RightPaneMode rightPane = RightPaneMode.Tree;
        private bool showPiano;
        private bool showToolsMenu;
        private bool showLeftPane = true;
        private string title;
        private string status = "No Valid Info Selected!";
        private string currentNote = "";
        private EditorTreeNode selectedNode;
        private bool showSoundPlayer;
        private bool showIndexPanel;
        private bool showForceUniqueFilePanel;
        private bool showSeqArcSeqPanel;

        protected EditorViewModelBase(
            IDialogService dialogs,
            Type fileType,
            string extensionDescription,
            string extension,
            string editorName
        )
        {
            Dialogs = dialogs;
            FileType = fileType;
            ExtensionDescription = extensionDescription;
            Extension = extension;
            EditorName = editorName;
            Title = editorName;

            NewCommand = new AsyncRelayCommand(NewAsync);
            OpenCommand = new AsyncRelayCommand(OpenAsync);
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            SaveAsCommand = new AsyncRelayCommand(SaveAsAsync);
            CloseFileCommand = new AsyncRelayCommand(CloseFileAsync);
            QuitCommand = new AsyncRelayCommand(QuitAsync);
            BlankFileCommand = new AsyncRelayCommand(BlankFileAsync);
            ImportFileCommand = new AsyncRelayCommand(ImportFileAsync);
            ExportFileCommand = new AsyncRelayCommand(ExportFileAsync);
        }

        // ------------------------------------------------------------------ file state

        protected IDialogService Dialogs { get; }

        public IOFile File { get; protected set; }

        /// <summary>Set when the editor was opened on a file living inside a sound archive.</summary>
        public IOFile ExtFile { get; protected set; }

        public string FileName { get; protected set; }

        public string FilePath { get; protected set; }

        public string ExtensionDescription { get; }

        public string Extension { get; }

        public string EditorName { get; }

        public bool FileOpen { get; protected set; }

        public Type FileType { get; }

        /// <summary>True while a panel is being filled from the file, so setters skip write-back.</summary>
        public bool WritingInfo { get; set; }

        // ------------------------------------------------------------------ shell state

        public string Title
        {
            get => title;
            protected set => SetProperty(ref title, value);
        }

        public string Status
        {
            get => status;
            set => SetProperty(ref status, value);
        }

        /// <summary>Right-hand status label showing the note currently under the pointer.</summary>
        public string CurrentNote
        {
            get => currentNote;
            set => SetProperty(ref currentNote, value);
        }

        /// <summary>Info panel currently shown on the left; a DataTemplate picks its layout.</summary>
        public object ActivePanel
        {
            get => activePanel;
            set => SetProperty(ref activePanel, value);
        }

        public RightPaneMode RightPane
        {
            get => rightPane;
            set => SetProperty(ref rightPane, value);
        }

        public bool ShowPiano
        {
            get => showPiano;
            set => SetProperty(ref showPiano, value);
        }

        /// <summary>The Tools menu is hidden everywhere except the sound archive window.</summary>
        public bool ShowToolsMenu
        {
            get => showToolsMenu;
            protected set => SetProperty(ref showToolsMenu, value);
        }

        public bool ShowLeftPane
        {
            get => showLeftPane;
            set => SetProperty(ref showLeftPane, value);
        }

        public NoInfoPanelViewModel NoInfoPanel { get; } = new();

        // The left pane is a stack, not a single panel: WinForms docked the sound player, item
        // index and force-unique sections to the top and let one Fill panel take the rest.

        public bool ShowSoundPlayer
        {
            get => showSoundPlayer;
            set => SetProperty(ref showSoundPlayer, value);
        }

        public bool ShowIndexPanel
        {
            get => showIndexPanel;
            set => SetProperty(ref showIndexPanel, value);
        }

        public bool ShowForceUniqueFilePanel
        {
            get => showForceUniqueFilePanel;
            set => SetProperty(ref showForceUniqueFilePanel, value);
        }

        public bool ShowSeqArcSeqPanel
        {
            get => showSeqArcSeqPanel;
            set => SetProperty(ref showSeqArcSeqPanel, value);
        }

        public IndexPanelViewModel IndexPanel { get; } = new();

        public ForceUniqueFilePanelViewModel ForceUniqueFilePanel { get; } = new();

        public SequenceArchiveSequencePanelViewModel SeqArcSeqPanel { get; } = new();

        /// <summary>
        /// The transport shown at the top of the left pane. Notifying because the sound archive
        /// window swaps it: sequences are driven by the sequence player, streams by the stream
        /// player, and the pane shows whichever matches the selection.
        /// </summary>
        public SoundPlayerPanelViewModel SoundPlayerPanel
        {
            get => soundPlayerPanel;
            protected set => SetProperty(ref soundPlayerPanel, value);
        }

        private SoundPlayerPanelViewModel soundPlayerPanel;

        // ------------------------------------------------------------------ tree

        public ObservableCollection<EditorTreeNode> Nodes { get; } = [];

        public EditorTreeNode SelectedNode
        {
            get => selectedNode;
            set
            {
                if (SetProperty(ref selectedNode, value) && !restoringSelection)
                {
                    DoInfoStuff();
                }
            }
        }

        /// <summary>
        /// Set while EndUpdateNodes puts the selection back after a rebuild. WinForms assigned
        /// tree.SelectedNode there without raising a selection event, and callers ran DoInfoStuff
        /// themselves afterwards; without this guard a rebuild refills every panel mid-edit,
        /// which throws away the row a grid is currently editing.
        /// </summary>
        private bool restoringSelection;

        /// <summary>Actions offered when right-clicking empty tree space; usually null.</summary>
        public IReadOnlyList<MenuAction> RootActions { get; protected set; }

        // ------------------------------------------------------------------ commands

        public ICommand NewCommand { get; }

        public ICommand OpenCommand { get; }

        public ICommand SaveCommand { get; }

        public ICommand SaveAsCommand { get; }

        public ICommand CloseFileCommand { get; }

        public ICommand QuitCommand { get; }

        public ICommand BlankFileCommand { get; }

        public ICommand ImportFileCommand { get; }

        public ICommand ExportFileCommand { get; }

        /// <summary>Raised when the window should close, e.g. from File ▸ Quit.</summary>
        public event EventHandler CloseRequested;

        protected void RequestClose() => CloseRequested?.Invoke(this, EventArgs.Empty);

        // ------------------------------------------------------------------ node bookkeeping

        private List<string> expandedNodes;
        private List<int> selectedPath;

        /// <summary>
        /// Records which nodes are open and where the selection sits, then empties the tree.
        /// Port of EditorBase.BeginUpdateNodes.
        /// </summary>
        public void BeginUpdateNodes()
        {
            expandedNodes = EditorTreeNode.CollectExpanded(Nodes);
            selectedPath = (SelectedNode ?? (Nodes.Count > 0 ? Nodes[0] : null))?.PathIndices() ?? [0];
            // Guard the whole rebuild, not just the restore at the end. Emptying the collections
            // makes the TreeView drop its selection, which comes back through the binding as a
            // null SelectedNode and runs DoInfoStuff for "nothing selected" -- so an edit that
            // touched the tree swapped the info panel out from under the user.
            restoringSelection = true;
            foreach (EditorTreeNode node in Nodes)
            {
                node.Nodes.Clear();
            }
        }

        /// <summary>Rebuilds the tree contents. Each editor fills in its own structure.</summary>
        public abstract void UpdateNodes();

        /// <summary>Restores the expansion and selection recorded by BeginUpdateNodes.</summary>
        public void EndUpdateNodes()
        {
            try
            {
                foreach (string name in expandedNodes ?? [])
                {
                    EditorTreeNode.FindByName(Nodes, name)?.ExpandPath();
                }
                EditorTreeNode restored = EditorTreeNode.FromPathIndices(Nodes, selectedPath ?? [0]);
                if (restored is not null)
                {
                    restored.ExpandPath();
                    SelectedNode = restored;
                    restored.IsSelected = true;
                }
            }
            finally
            {
                restoringSelection = false;
            }
        }

        /// <summary>Shows the right info panel for the current selection.</summary>
        public virtual void DoInfoStuff()
        {
            if (!FileOpen)
            {
                ActivePanel = NoInfoPanel;
                Status = "No Valid Info Selected!";
            }
        }

        // ------------------------------------------------------------------ file menu

        public virtual async Task NewAsync()
        {
            if (!await FileTestAsync(true))
            {
                return;
            }
            File = (IOFile)Activator.CreateInstance(FileType);
            FilePath = "";
            ExtFile = null;
            FileOpen = true;
            Title = EditorName + " - New " + ExtensionDescription + ".s" + Extension;
            UpdateNodes();
            DoInfoStuff();
        }

        /// <summary>Opens a file by path, as the command line does. No save prompt first.</summary>
        public virtual void OpenFile(string path)
        {
            File = (IOFile)Activator.CreateInstance(FileType);
            ExtFile = null;
            FilePath = path;
            FileName = Path.GetFileNameWithoutExtension(path);
            Title = EditorName + " - " + Path.GetFileName(path);
            FileOpen = true;
            File.Read(path);
            UpdateNodes();
            DoInfoStuff();
        }

        public virtual async Task OpenAsync()
        {
            if (!await FileTestAsync(true))
            {
                return;
            }
            string path = await GetFileOpenerPathAsync(ExtensionDescription, Extension);
            if (path == "")
            {
                return;
            }
            File = (IOFile)Activator.CreateInstance(FileType);
            ExtFile = null;
            FilePath = path;
            FileName = Path.GetFileNameWithoutExtension(path);
            Title = EditorName + " - " + Path.GetFileName(path);
            FileOpen = true;
            File.Read(path);
            UpdateNodes();
            DoInfoStuff();
        }

        public virtual async Task SaveAsync()
        {
            if (!FileOpen)
            {
                return;
            }
            if (ExtFile is not null)
            {
                // Editing a file inside an archive: write straight back into the archive copy.
                ExtFile.Read(File.Write());
                return;
            }
            if (string.IsNullOrEmpty(FilePath))
            {
                await SaveAsAsync();
                return;
            }
            File.Write(FilePath);
        }

        public virtual async Task SaveAsAsync()
        {
            if (!FileOpen)
            {
                return;
            }
            string path = await GetFileSaverPathAsync(ExtensionDescription, Extension);
            if (path == "")
            {
                return;
            }
            FilePath = path;
            FileName = Path.GetFileNameWithoutExtension(path);
            Title = EditorName + " - " + Path.GetFileName(path);
            File.Write(FilePath);
        }

        public virtual async Task CloseFileAsync()
        {
            if (!await FileTestAsync(true))
            {
                return;
            }
            FileOpen = false;
            FilePath = "";
            ExtFile = null;
            File = null;
            Title = EditorName;
            UpdateNodes();
            DoInfoStuff();
        }

        public virtual async Task QuitAsync()
        {
            if (!await FileTestAsync(true))
            {
                return;
            }
            RequestClose();
        }

        public virtual Task BlankFileAsync() => Task.CompletedTask;

        public virtual Task ImportFileAsync() => Task.CompletedTask;

        public virtual Task ExportFileAsync() => Task.CompletedTask;

        /// <summary>
        /// Port of EditorBase.FileTest: offers to save before an action that discards the current
        /// file, and blocks actions that need a file open when there is none.
        /// </summary>
        public async Task<bool> FileTestAsync(bool save, bool forceOpen = false)
        {
            if (FileOpen)
            {
                if (!save)
                {
                    return true;
                }
                switch (await Dialogs.AskSaveBeforeCloseAsync())
                {
                    case SavePrompt.Save:
                        await SaveAsync();
                        return true;
                    case SavePrompt.Discard:
                        return true;
                    default:
                        return false;
                }
            }
            if (forceOpen)
            {
                await Dialogs.ShowMessageAsync("There must be a file open to do this!", "Notice:");
                return false;
            }
            return true;
        }

        /// <summary>Open picker filtered to this editor's own extension.</summary>
        public Task<string> GetFileOpenerPathAsync(string description, string extension) =>
            Dialogs.OpenFileAsync(description + "|*.s" + extension.ToLower());

        /// <summary>Save picker filtered to this editor's own extension.</summary>
        public async Task<string> GetFileSaverPathAsync(string description, string extension)
        {
            string path = await Dialogs.SaveFileAsync(
                description + "|*.s" + extension.ToLower(),
                FileName is null ? null : FileName + ".s" + extension.ToLower()
            );
            if (path != "" && Path.GetExtension(path) == "")
            {
                path += ".s" + extension.ToLower();
            }
            return path;
        }

        // ------------------------------------------------------------------ node hooks

        public virtual void RootAdd() { }

        public virtual void NodeAddAbove() { }

        public virtual void NodeAddBelow() { }

        public virtual void NodeMoveUp() { }

        public virtual void NodeMoveDown() { }

        public virtual void NodeBlank() { }

        public virtual void NodeReplace() { }

        public virtual void NodeExport() { }

        public virtual void NodeNullify() { }

        public virtual void NodeDelete() { }

        public virtual void NodeMouseDoubleClick() { }

        /// <summary>
        /// Previews whatever the tree has selected. Bound to Space, as the WinForms editors did
        /// with their tree KeyPress handlers.
        /// </summary>
        public virtual void PlaySelected() { }

        /// <param name="note">The key just pressed. Passed in so no handler ordering matters.</param>
        public virtual void OnPianoPress(GotaSequenceLib.Notes note) { }

        public virtual void OnPianoRelease() { }

        /// <summary>Called as the window closes, so editors can stop playback and release files.</summary>
        public virtual void OnClosing() { }

        /// <summary>Swaps two list entries, returning false when either index is out of range.</summary>
        public static bool Swap<T>(IList<T> objects, int a, int b)
        {
            if (a < 0 || b < 0 || a >= objects.Count || b >= objects.Count)
            {
                return false;
            }
            (objects[b], objects[a]) = (objects[a], objects[b]);
            return true;
        }
    }
}
