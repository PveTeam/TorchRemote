using System.Windows;
using System.Windows.Controls;
using Torch;

namespace TorchRemote.Plugin;

public partial class ConfigControl : UserControl
{
    private readonly Persistent<Config> _persistent;

    public ConfigControl(Persistent<Config> persistent)
    {
        _persistent = persistent;
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        _persistent.Save();
    }
}