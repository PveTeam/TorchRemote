using System.Collections.Concurrent;
using System.Text.Json;
using Json.Schema;
using Json.Schema.Generation;
using NLog;
using Torch.API;
using Torch.Managers;
using Torch.Server.Managers;
using Torch.Server.ViewModels;
using TorchRemote.Plugin.Utils;

namespace TorchRemote.Plugin.Managers;

public class SettingManager : Manager
{
    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

    [Dependency]
    private readonly InstanceManager _instanceManager = null!;
    
    public SettingManager(ITorchBase torchInstance) : base(torchInstance)
    {
    }

    public override void Attach()
    {
        base.Attach();
        _instanceManager.InstanceLoaded += InstanceManagerOnInstanceLoaded;
    }

    private void InstanceManagerOnInstanceLoaded(ConfigDedicatedViewModel config)
    {
        RegisterSetting(config.SessionSettings, typeof(SessionSettingsViewModel));
    }

    public void RegisterSetting(object value, Type type, bool includeOnlyDisplay = true)
    {
        var builder = new JsonSchemaBuilder().FromType(type, new()
        {
            PropertyNamingMethod = input => Statics.SerializerOptions.PropertyNamingPolicy!.ConvertName(input)
        });
        
        Settings[type.FullName!] = new(type.Name, type, builder.Build(), value, includeOnlyDisplay);
        Log.Info("Registered {0} type", type.FullName);
    }

    public IDictionary<string, Setting> Settings { get; } = new ConcurrentDictionary<string, Setting>();
}

public record Setting(string Name, Type Type, JsonSchema Schema, object Value, bool IncludeDisplayOnly);
