using Avalonia.Controls;
using NitroStudio2.Services;
using NitroStudio2.ViewModels;
using System;

namespace NitroStudio2.Views
{
    public partial class SequenceRecorderWindow : Window
    {
        public SequenceRecorderWindow()
        {
            InitializeComponent();
        }

        public SequenceRecorderWindow(SequenceRecorderViewModel viewModel)
            : this()
        {
            DataContext = viewModel;
            Icon = new WindowIcon(Assets.WindowIcon("Wave"));
            viewModel.Finished += (_, _) => Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            (DataContext as IDisposable)?.Dispose();
            base.OnClosed(e);
        }
    }
}
