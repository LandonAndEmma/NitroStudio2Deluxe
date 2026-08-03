using Avalonia.Controls;
using NitroStudio2.Services;
using NitroStudio2.ViewModels;

namespace NitroStudio2.Views
{
    public partial class CreateStreamToolWindow : Window
    {
        public CreateStreamToolWindow()
        {
            InitializeComponent();
        }

        /// <param name="swavMode">True for the Tools "Creave Wave" entry, false for "Create Stream".</param>
        public CreateStreamToolWindow(bool swavMode)
            : this()
        {
            CreateStreamToolViewModel viewModel = new(new DialogService(this), swavMode);
            DataContext = viewModel;
            Icon = new WindowIcon(Assets.WindowIcon(viewModel.IconName));
        }
    }
}
