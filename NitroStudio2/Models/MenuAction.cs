using Avalonia.Media.Imaging;
using NitroStudio2.Services;
using System;
using System.Windows.Input;

namespace NitroStudio2.Models
{
    /// <summary>
    /// One entry in a menu or tree context menu. Replaces ToolStripMenuItem, including the
    /// small icon WinForms drew to the left of the text.
    /// </summary>
    public sealed class MenuAction
    {
        public MenuAction(string header, string iconName, Action execute, Func<bool> canExecute = null)
        {
            Header = header;
            IconName = iconName;
            Command = new DelegateCommand(execute, canExecute);
        }

        public string Header { get; }

        /// <summary>Asset name under Assets/Menu, or null for an item with no icon.</summary>
        public string IconName { get; }

        public Bitmap Icon => IconName is null ? null : Assets.Menu(IconName);

        public ICommand Command { get; }

        /// <summary>Re-evaluates CanExecute so the item enables/disables with the current state.</summary>
        public void RaiseCanExecuteChanged()
        {
            ((DelegateCommand)Command).RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Minimal ICommand over a delegate pair. CommunityToolkit's RelayCommand would do, but menu
    /// items are built in loops from (text, icon, handler) tuples and this keeps that call shape.
    /// </summary>
    public sealed class DelegateCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        public DelegateCommand(Action execute, Func<bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            execute?.Invoke();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
