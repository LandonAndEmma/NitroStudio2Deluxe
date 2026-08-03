using CommunityToolkit.Mvvm.Input;
using GotaSoundIO.Sound;
using GotaSoundIO.Sound.Encoding;
using GotaSoundIO.Sound.Formats;
using NitroFileLoader;
using NitroStudio2.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NitroStudio2.ViewModels
{
    /// <summary>
    /// Converts a WAV/SWAV/STRM into a .swav or .strm. Ported from the WinForms
    /// CreateStreamTool form, which the Tools menu opens in either mode.
    /// </summary>
    public sealed class CreateStreamToolViewModel : ViewModelBase
    {
        private readonly IDialogService dialogs;
        private readonly bool swavMode;

        public CreateStreamToolViewModel(IDialogService dialogs, bool swavMode)
        {
            this.dialogs = dialogs;
            this.swavMode = swavMode;
            BrowseInputCommand = new AsyncRelayCommand(BrowseInputAsync);
            BrowseOutputCommand = new AsyncRelayCommand(BrowseOutputAsync);
            ExportCommand = new AsyncRelayCommand(ExportAsync);
        }

        public string Title => swavMode ? "Create Wave" : "Create Stream";

        public string IconName => swavMode ? "Wave" : "Stream";

        public string InputFile
        {
            get;
            set => SetProperty(ref field, value);
        } = "";

        public string OutputFile
        {
            get;
            set => SetProperty(ref field, value);
        } = "";

        public IReadOnlyList<string> OutputFormats { get; } = ["PCM8", "PCM16", "IMA-ADPCM"];

        public int OutputFormatIndex
        {
            get;
            set => SetProperty(ref field, value);
        } = 2;

        public ICommand BrowseInputCommand { get; }

        public ICommand BrowseOutputCommand { get; }

        public ICommand ExportCommand { get; }

        private async Task BrowseInputAsync()
        {
            string path = await dialogs.OpenFileAsync("Supported Sound Files|*.wav;*.swav;*.strm");
            if (path != "")
            {
                InputFile = path;
            }
        }

        private async Task BrowseOutputAsync()
        {
            string path = await dialogs.SaveFileAsync(
                swavMode ? "Sound Wave|*.swav" : "Sound Stream|*.strm"
            );
            if (path != "")
            {
                OutputFile = path;
            }
        }

        private async Task ExportAsync()
        {
            if (InputFile == "")
            {
                await dialogs.ShowMessageAsync("No Input File Selected!");
                return;
            }
            if (OutputFile == "")
            {
                await dialogs.ShowMessageAsync("No Output File Selected!");
                return;
            }

            SoundFile output = swavMode ? new Wave() : new NitroFileLoader.Stream();
            SoundFile input = Path.GetExtension(InputFile) switch
            {
                ".swav" => new Wave(),
                ".strm" => new NitroFileLoader.Stream(),
                _ => new RiffWave(),
            };
            input.Read(InputFile);
            Type conversionType = OutputFormatIndex switch
            {
                0 => typeof(PCM8Signed),
                1 => typeof(PCM16),
                _ => typeof(ImaAdpcm),
            };
            output.FromOtherStreamFile(input, conversionType);
            output.Write(OutputFile);
        }
    }
}
