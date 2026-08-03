using Avalonia.Controls;
using NitroStudio2.Services;
using NitroStudio2.ViewModels;
using System;

namespace NitroStudio2.Views
{
    public partial class InstrumentSelectorWindow : Window
    {
        public InstrumentSelectorWindow()
        {
            InitializeComponent();
        }

        public InstrumentSelectorWindow(InstrumentSelectorViewModel viewModel)
            : this()
        {
            DataContext = viewModel;
            Icon = new WindowIcon(Assets.WindowIcon("Bank"));
            viewModel.Finished += (_, _) => Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            (DataContext as IDisposable)?.Dispose();
            base.OnClosed(e);
        }
    }
}
