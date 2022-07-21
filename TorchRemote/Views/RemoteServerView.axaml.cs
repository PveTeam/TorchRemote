using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using TorchRemote.ViewModels;

namespace TorchRemote.Views;

public partial class RemoteServerView : ReactiveUserControl<RemoteServerViewModel>
{
    public RemoteServerView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

