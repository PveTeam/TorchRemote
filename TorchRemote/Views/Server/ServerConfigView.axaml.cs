using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using TorchRemote.ViewModels.Server;

namespace TorchRemote.Views.Server;

public partial class ServerConfigView : ReactiveUserControl<ServerConfigViewModel>
{
    public ServerConfigView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

