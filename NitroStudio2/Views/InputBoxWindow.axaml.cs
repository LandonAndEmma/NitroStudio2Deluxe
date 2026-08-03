using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace NitroStudio2.Views
{
    /// <summary>
    /// Stand-in for Microsoft.VisualBasic Interaction.InputBox, which the rename flow used.
    /// Closes with the entered text, or "" when cancelled, matching InputBox's contract.
    /// </summary>
    public partial class InputBoxWindow : Window
    {
        public InputBoxWindow()
        {
            InitializeComponent();
        }

        public InputBoxWindow(string prompt, string title, string defaultValue)
            : this()
        {
            Title = title;
            Prompt.Text = prompt;
            Input.Text = defaultValue ?? "";
        }

        protected override void OnOpened(System.EventArgs e)
        {
            base.OnOpened(e);
            Input.SelectAll();
            _ = Input.Focus();
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            Close(Input.Text ?? "");
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            Close("");
        }

        private void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Close(Input.Text ?? "");
            }
        }
    }
}
