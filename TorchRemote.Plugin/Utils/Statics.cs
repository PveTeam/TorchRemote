using System.Text.Json;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.Commands;
using Torch.Managers;
using Torch.Server;
using Torch.Server.Managers;
using TorchRemote.Plugin.Managers;
using TorchRemote.Plugin.Modules;
namespace TorchRemote.Plugin.Utils;

internal static class Statics
{
#pragma warning disable CS0618
    public static TorchServer Torch => (TorchServer)TorchBase.Instance;
#pragma warning restore CS0618

    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    public static SettingManager SettingManager => Torch.Managers.GetManager<SettingManager>();
    public static InstanceManager InstanceManager => Torch.Managers.GetManager<InstanceManager>();
    public static PluginManager PluginManager => Torch.Managers.GetManager<PluginManager>();
    public static CommandManager? CommandManager => Torch.CurrentSession?.Managers.GetManager<CommandManager>();
    public static MultiplayerManagerDedicated? MultiplayerManager =>
        Torch.CurrentSession?.Managers.GetManager<MultiplayerManagerDedicated>();

    public static ChatModule ChatModule = null!;
}
