using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Json.Schema;
using Json.Schema.Generation;
using NLog;
using Torch;
using Torch.API;
using Torch.API.Plugins;
using Torch.Managers;
using Torch.Server;
using Torch.Server.Managers;
using Torch.Server.ViewModels;
using TorchRemote.Plugin.Refiners;
using TorchRemote.Plugin.Utils;

namespace TorchRemote.Plugin.Managers;

public class SettingManager : Manager
{
    private const string BlockLimiterGuid = "11fca5c4-01b6-4fc3-a215-602e2325be2b";
    private const string BlockLimiterPointSusGuid = "91CA8ED2-AB79-4538-BADC-0EE67F62A906";
    private const BindingFlags Flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

    private readonly Dictionary<Guid, (string? Type, string Field)> _pluginsCustomConfigPath = new()
    {
        [new(BlockLimiterGuid)] = ("BlockLimiter.Settings.BlockLimiterConfig", "_instance"),
        [new(BlockLimiterPointSusGuid)] = ("BlockLimiter.Settings.BlockLimiterConfig", "_instance"),
    };

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
            if (_pluginsCustomConfigPath.TryGetValue(plugin.Id, out var tuple))
            {
                RegisterCustomPluginSetting(plugin, tuple);
                continue;
            }
            
            var type = plugin.GetType();
            object persistentInstance;

            bool IsSuitable(MemberInfo m, Type t) =>
                t.IsGenericType && typeof(Persistent<>).IsAssignableFrom(t.GetGenericTypeDefinition()) &&
                (m.Name.Contains(
                     "config", StringComparison.InvariantCultureIgnoreCase) ||
                 m.Name.Contains(
                     "cfg", StringComparison.InvariantCultureIgnoreCase));

            var fields = type.GetFields(Flags).Where(b => IsSuitable(b, b.FieldType)).ToArray();
            var props = type.GetProperties(Flags).Where(b => IsSuitable(b, b.PropertyType)).ToArray();

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
            
            RegisterPluginSetting(plugin, getter.GetValue(persistentInstance), settingType);
        }
    }

    private void RegisterCustomPluginSetting(ITorchPlugin plugin, (string? Type, string Field) tuple)
    {
        var customType = tuple.Type is null ? plugin.GetType() : plugin.GetType().Assembly.GetType(tuple.Type, true);

        var field = customType.GetField(tuple.Field, Flags) ?? throw new MissingFieldException(customType.FullName, tuple.Field);

        var value = field.GetValue(field.IsStatic ? null : plugin);
        
        if (value is not null)
            RegisterPluginSetting(plugin, value, field.FieldType);
    }

    private void InstanceManagerOnInstanceLoaded(ConfigDedicatedViewModel config)
    {
        RegisterSetting("Session Settings", config.SessionSettings, typeof(SessionSettingsViewModel));
    }

    public void RegisterPluginSetting(ITorchPlugin plugin, object value, Type settingType)
    {
        RegisterSetting(plugin.Name, value, settingType);
        PluginSettings.Add(plugin.Id, settingType.FullName!);
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