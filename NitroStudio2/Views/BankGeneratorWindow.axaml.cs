using Avalonia.Controls;
using Avalonia.Input;
using NitroStudio2.Services;
using NitroStudio2.ViewModels;
using System;

namespace NitroStudio2.Views
{
    public partial class BankGeneratorWindow : Window
    {
        public BankGeneratorWindow()
        {
            InitializeComponent();
        }

        public BankGeneratorWindow(BankGeneratorViewModel viewModel)
            : this()
        {
            DataContext = viewModel;
            Icon = new WindowIcon(Assets.WindowIcon("BankGenerator"));
            viewModel.Finished += (_, _) => Close();
        }

        /// <summary>
        /// Delete removes the selected row, matching the WinForms grid's AllowUserToDeleteRows.
        /// Avalonia's DataGrid has no built-in equivalent.
        /// </summary>
        private void OnGridKeyDown(object sender, KeyEventArgs e)
        {
            if (
                e.Key == Key.Delete
                && DataContext is BankGeneratorViewModel viewModel
                && Instruments.SelectedItem is BankGeneratorRow row
                && !Instruments.IsFocused
            )
            {
                viewModel.DeleteRowCommand.Execute(row);
                e.Handled = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            (DataContext as IDisposable)?.Dispose();
            base.OnClosed(e);
        }
    }
}
