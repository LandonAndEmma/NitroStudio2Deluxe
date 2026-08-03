using Avalonia.Controls;
using NitroStudio2.Services;
using NitroStudio2.ViewModels;
using System;

namespace NitroStudio2.Views
{
    public partial class WaveMapperWindow : Window
    {
        public WaveMapperWindow()
        {
            InitializeComponent();
        }

        public WaveMapperWindow(WaveMapperViewModel viewModel)
            : this()
        {
            DataContext = viewModel;
            Icon = new WindowIcon(Assets.WindowIcon("Wave"));
            // Same column the WinForms form hid when mapping a single wave.
            MapGrid.Columns[1].IsVisible = !viewModel.HideWaveId;
            viewModel.Finished += (_, _) => Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            (DataContext as IDisposable)?.Dispose();
            base.OnClosed(e);
        }
    }
}
