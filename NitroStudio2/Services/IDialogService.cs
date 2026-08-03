using System.Threading.Tasks;

namespace NitroStudio2.Services
{
    /// <summary>Outcome of the "save before closing?" prompts, matching SaveCloseDialog.getValue().</summary>
    public enum SavePrompt
    {
        Save = 0,
        Discard = 1,
        Cancel = 2,
    }

    /// <summary>
    /// Everything the editors used to reach for directly: OpenFileDialog, SaveFileDialog,
    /// MessageBox.Show, Interaction.InputBox and the two save-confirmation forms.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>Open picker. Returns the chosen path, or "" when cancelled (as WinForms did).</summary>
        Task<string> OpenFileAsync(string filter, string suggestedFileName = null);

        /// <summary>Save picker. Returns the chosen path, or "" when cancelled.</summary>
        Task<string> SaveFileAsync(string filter, string suggestedFileName = null);

        /// <summary>Folder picker. Returns the chosen directory, or "" when cancelled.</summary>
        Task<string> PickFolderAsync(string title = null);

        /// <summary>Informational OK box, the shape all 61 MessageBox.Show call sites used.</summary>
        Task ShowMessageAsync(string text, string caption = "Nitro Studio 2");

        Task ShowWarningAsync(string text, string caption = "Nitro Studio 2");

        Task ShowErrorAsync(string text, string caption = "Nitro Studio 2");

        /// <summary>Replaces SaveCloseDialog: "Save and Close" / "Close" / "Cancel".</summary>
        Task<SavePrompt> AskSaveBeforeCloseAsync();

        /// <summary>Replaces SaveQuitDialog: "Save and Quit" / "Quit" / "Cancel".</summary>
        Task<SavePrompt> AskSaveBeforeQuitAsync();

        /// <summary>Replaces Microsoft.VisualBasic Interaction.InputBox. "" means cancelled.</summary>
        Task<string> InputBoxAsync(string prompt, string title, string defaultValue = "");
    }
}
