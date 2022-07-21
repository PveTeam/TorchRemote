using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using NLog;
using Torch.API;
using Torch.Managers;
using Torch.Views;
using TorchRemote.Models.Responses;
using TorchRemote.Plugin.Utils;
using VRage;
namespace TorchRemote.Plugin.Managers;

public class SettingManager : Manager
{
    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
    
    public SettingManager(ITorchBase torchInstance) : base(torchInstance)
    {
    }

    public Guid RegisterSetting(object value, Type type, bool includeOnlyDisplay = true)
    {
        var properties = type.IsInterface ? type.GetProperties() : type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var settingProperties = properties
            .Where(b => !b.HasAttribute<XmlIgnoreAttribute>() && 
                        !b.HasAttribute<JsonIgnoreAttribute>() &&
                        (!includeOnlyDisplay ||
                         b.HasAttribute<DisplayAttribute>()))
            .Select(property => new SettingProperty(property.Name,
                GetTypeId(property.PropertyType, property.GetValue(value), includeOnlyDisplay),
                property.PropertyType, property.GetMethod, property.SetMethod, 
                property.GetCustomAttribute<DisplayAttribute>() is { } attr ? 
                    new(attr.Name, attr.Description, attr.GroupName, attr.Order, attr.ReadOnly, attr.Enabled) : 
                    null))
            .ToArray();

        var setting = new Setting(type.Name, type, settingProperties, value);

        var id = (type.FullName! + value.GetHashCode()).ToGuid();
        Settings.Add(id, setting);
        Log.Debug("Registered type {0} with id {1}", type, id);
        return id;
    }

    private Guid GetTypeId(Type type, object value, bool includeOnlyDisplay)
    {
        if (type == typeof(int) || type == typeof(uint))
            return SettingPropertyTypeEnum.Integer;
        if (type == typeof(bool))
            return SettingPropertyTypeEnum.Boolean;
        if (type == typeof(short) ||
            type == typeof(ushort) ||
            type == typeof(byte) ||
            type == typeof(ulong) ||
            type == typeof(long) ||
            type == typeof(float) || 
            type == typeof(double) ||
            type == typeof(MyFixedPoint))
            return SettingPropertyTypeEnum.Number;
        if (type == typeof(string))
            return SettingPropertyTypeEnum.String;
        if (type == typeof(DateTime))
            return SettingPropertyTypeEnum.DateTime;
        if (type == typeof(TimeSpan))
            return SettingPropertyTypeEnum.TimeSpan;
        if (type == typeof(System.Drawing.Color) || type == typeof(VRageMath.Color))
            return SettingPropertyTypeEnum.Color;
        return RegisterSetting(value, type, includeOnlyDisplay);
    }

    public IDictionary<Guid, Setting> Settings { get; } = new ConcurrentDictionary<Guid, Setting>();
}

public record Setting(string Name, Type Type, IEnumerable<SettingProperty> Properties, object? Value = null);
public record SettingProperty(string Name, Guid TypeId, Type Type, MethodInfo Getter, MethodInfo? Setter, SettingPropertyDisplayInfo? DisplayInfo);
public record SettingPropertyDisplayInfo(string? Name, string? Description, string? GroupName, int? Order, bool? IsReadOnly, bool? IsEnabled);
