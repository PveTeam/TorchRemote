using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Json.Schema;
using Json.Schema.Generation;
using NLog;
using Torch;
using Torch.API;
using Torch.Managers;
using Torch.Server;
using Torch.Server.Managers;
using Torch.Server.ViewModels;
using TorchRemote.Plugin.Refiners;
using TorchRemote.Plugin.Utils;

namespace TorchRemote.Plugin.Managers;

public class SettingManager : Manager
{
    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

    [Dependency]
    private readonly InstanceManager _instanceManager = null!;
    [Dependency]
    private readonly PluginManager _pluginManager = null!;
    
    public SettingManager(ITorchBase torchInstance) : base(torchInstance)
    {
    }

    public override void Attach()
    {
        base.Attach();
        _instanceManager.InstanceLoaded += InstanceManagerOnInstanceLoaded;
        
        RegisterSetting("Torch Config", Torch.Config, typeof(TorchConfig));
        
        foreach (var plugin in _pluginManager.Plugins.Values)
        {
            var type = plugin.GetType();
            object persistentInstance;
            
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            bool IsSuitable(MemberInfo m, Type t) =>
                t.IsGenericType && typeof(Persistent<>).IsAssignableFrom(t.GetGenericTypeDefinition()) &&
                (m.Name.Contains(
                     "config", StringComparison.InvariantCultureIgnoreCase) ||
                 m.Name.Contains(
                     "cfg", StringComparison.InvariantCultureIgnoreCase));

            var fields = type.GetFields(flags).Where(b => IsSuitable(b, b.FieldType)).ToArray();
            var props = type.GetProperties(flags).Where(b => IsSuitable(b, b.PropertyType)).ToArray();

            if (fields.FirstOrDefault() is { } field)
            {
                persistentInstance = field.GetValue(plugin);
            }
            else if (props.FirstOrDefault() is { } prop)
            {
                persistentInstance = prop.GetValue(plugin);
            }
            else
            {
                continue;
            }
            
            if (persistentInstance is null)
                continue;

            var persistentType = persistentInstance.GetType();
            var getter = persistentType.GetProperty("Data")!;

            var settingType = persistentType.GenericTypeArguments[0];
            
            RegisterSetting(plugin.Name, getter.GetValue(persistentInstance), settingType);
            PluginSettings.Add(plugin.Id, settingType.FullName!);
        }
    }

    private void InstanceManagerOnInstanceLoaded(ConfigDedicatedViewModel config)
    {
        RegisterSetting("Session Settings", config.SessionSettings, typeof(SessionSettingsViewModel));
    }

    public void RegisterSetting(string name, object value, Type type)
    {
        var builder = new JsonSchemaBuilder().FromType(type, new()
        {
            PropertyNamingMethod = input => Statics.SerializerOptions.PropertyNamingPolicy!.ConvertName(input),
            Refiners = { new DisplayAttributeRefiner() }
        });
        Settings[type.FullName!] = new(name, type, builder.Build(), value);
        Log.Info("Registered {0} type with name {1}", type.FullName, name);
    }

    public IDictionary<string, Setting> Settings { get; } = new ConcurrentDictionary<string, Setting>();
    public IDictionary<Guid, string> PluginSettings { get; } = new ConcurrentDictionary<Guid, string>();
}

public record Setting(string Name, Type Type, JsonSchema Schema, object Value);