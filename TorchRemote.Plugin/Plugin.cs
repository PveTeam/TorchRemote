using System.IO;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Views;
using TorchRemote.Plugin.Managers;

namespace TorchRemote.Plugin;

public class Plugin : TorchPluginBase, IWpfPlugin
{
    private Persistent<Config> _config = null!;

    public override void Init(ITorchBase torch)
    {
        base.Init(torch);
        _config = Persistent<Config>.Load(Path.Combine(StoragePath, "TorchRemote.cfg"));

        Torch.Managers.AddManager(new ApiServerManager(Torch, _config.Data));
        Torch.Managers.AddManager(new SettingManager(Torch));
        Torch.Managers.GetManager<ITorchSessionManager>()
            .AddFactory(s => new ChatMonitorManager(s.Torch));
    }

    public UserControl GetControl() => new PropertyGrid
    {
        Margin = new(3),
        DataContext = _config.Data
    };
}
