using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;
using NitroStudio2.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NitroStudio2.Services
{
    /// <summary>
    /// Avalonia implementation of <see cref="IDialogService"/>, owned by whichever window is
    /// asking so message boxes and pickers are parented correctly.
    /// </summary>
    public sealed class DialogService : IDialogService
    {
        private readonly Window owner;

        /// <summary>
        /// WinForms' RestoreDirectory reopened the picker where the user last was; Avalonia has
        /// no equivalent, so the last folder is remembered here and used as the start location.
        /// </summary>
        private static string lastFolder;

        public DialogService(Window owner)
        {
            this.owner = owner;
        }

        // ------------------------------------------------------------------ file pickers

        public async Task<string> OpenFileAsync(string filter, string suggestedFileName = null)
        {
            IReadOnlyList<IStorageFile> files = await owner.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    AllowMultiple = false,
                    FileTypeFilter = [.. FileFilter.Parse(filter), FileFilter.AnyFile],
                    SuggestedStartLocation = await StartLocationAsync(),
                }
            );
            return Remember(files.Count > 0 ? files[0].TryGetLocalPath() : null);
        }

        public async Task<string> SaveFileAsync(string filter, string suggestedFileName = null)
        {
            IStorageFile file = await owner.StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    SuggestedFileName = suggestedFileName,
                    DefaultExtension = FileFilter.DefaultExtension(filter)?.TrimStart('.'),
                    FileTypeChoices = [.. FileFilter.Parse(filter), FileFilter.AnyFile],
                    ShowOverwritePrompt = true,
                    SuggestedStartLocation = await StartLocationAsync(),
                }
            );
            return Remember(file?.TryGetLocalPath());
        }

        public async Task<string> PickFolderAsync(string title = null)
        {
            IReadOnlyList<IStorageFolder> folders =
                await owner.StorageProvider.OpenFolderPickerAsync(
                    new FolderPickerOpenOptions
                    {
                        Title = title,
                        AllowMultiple = false,
                        SuggestedStartLocation = await StartLocationAsync(),
                    }
                );
            string path = folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
            if (!string.IsNullOrEmpty(path))
            {
                lastFolder = path;
            }
            return path ?? "";
        }

        private async Task<IStorageFolder> StartLocationAsync() =>
            string.IsNullOrEmpty(lastFolder)
                ? null
                : await owner.StorageProvider.TryGetFolderFromPathAsync(lastFolder);

        private static string Remember(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "";
            }
            try
            {
                lastFolder = Path.GetDirectoryName(path);
            }
            catch { }
            return path;
        }

        // ------------------------------------------------------------------ message boxes

        public Task ShowMessageAsync(string text, string caption = "Nitro Studio 2") =>
            ShowStandardAsync(text, caption, Icon.Info);

        public Task ShowWarningAsync(string text, string caption = "Nitro Studio 2") =>
            ShowStandardAsync(text, caption, Icon.Warning);

        public Task ShowErrorAsync(string text, string caption = "Nitro Studio 2") =>
            ShowStandardAsync(text, caption, Icon.Error);

        private async Task ShowStandardAsync(string text, string caption, Icon icon)
        {
            await MessageBoxManager
                .GetMessageBoxStandard(
                    new MessageBoxStandardParams
                    {
                        ContentTitle = caption,
                        ContentMessage = text,
                        ButtonDefinitions = ButtonEnum.Ok,
                        Icon = icon,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        CanResize = false,
                    }
                )
                .ShowWindowDialogAsync(owner);
        }

        public Task<SavePrompt> AskSaveBeforeCloseAsync() =>
            AskSaveAsync(
                "Do you want to save before you close the file?",
                "Save and Close",
                "Close"
            );

        public Task<SavePrompt> AskSaveBeforeQuitAsync() =>
            AskSaveAsync("Do you want to save before you exit?", "Save and Quit", "Quit");

        private async Task<SavePrompt> AskSaveAsync(string message, string save, string discard)
        {
            string result = await MessageBoxManager
                .GetMessageBoxCustom(
                    new MessageBoxCustomParams
                    {
                        ContentTitle = "Warning",
                        ContentMessage = message,
                        ButtonDefinitions =
                        [
                            new ButtonDefinition { Name = save, IsDefault = true },
                            new ButtonDefinition { Name = discard },
                            new ButtonDefinition { Name = "Cancel", IsCancel = true },
                        ],
                        Icon = Icon.Warning,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        CanResize = false,
                    }
                )
                .ShowWindowDialogAsync(owner);

            if (result == save)
            {
                return SavePrompt.Save;
            }
            return result == discard ? SavePrompt.Discard : SavePrompt.Cancel;
        }

        // ------------------------------------------------------------------ text input

        public async Task<string> InputBoxAsync(string prompt, string title, string defaultValue = "")
        {
            InputBoxWindow window = new(prompt, title, defaultValue);
            return await window.ShowDialog<string>(owner) ?? "";
        }
    }
}
