using System.Collections;
using System.Reflection;
using System.Text.Json;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Humanizer;
using TorchRemote.Models.Responses;
using TorchRemote.Models.Shared.Settings;
using TorchRemote.Plugin.Abstractions.Controllers;
using TorchRemote.Plugin.Utils;
using StringExtensions = Swan.StringExtensions;

namespace TorchRemote.Plugin.Controllers;

public class SettingsController : WebApiController, ISettingsController
{
    private const string RootPath = "/settings";

    [Route(HttpVerbs.Get, RootPath)]
    public IEnumerable<string> GetAllSettings()
    {
        return Statics.SettingManager.Settings.Keys;
    }

    [Route(HttpVerbs.Get, $"{RootPath}/{{fullName}}")]
    public SettingInfoResponse Get(string fullName)
    {
        if (!Statics.SettingManager.Settings.TryGetValue(fullName, out var setting))
            throw HttpException.NotFound($"Setting with fullName {fullName} not found", fullName);

        return new(StringExtensions.Humanize(setting.Name), setting.Schema);
    }

    [Route(HttpVerbs.Get, $"{RootPath}/{{fullName}}/values")]
    public IEnumerable<PropertyBase> GetValues(string fullName, [JsonBody] IEnumerable<string> propertyNames)
    {
        if (!Statics.SettingManager.Settings.TryGetValue(fullName, out var setting))
            throw HttpException.NotFound($"Setting with fullName {fullName} not found", fullName);

        return propertyNames.Select(name =>
        {
            var propInfo =
                setting.Type.GetProperty(name.Pascalize(), BindingFlags.Instance | BindingFlags.Public);

            if (propInfo is null)
                throw HttpException.NotFound("Property not found", name);

            var type = propInfo!.PropertyType;
            var value = propInfo.GetValue(setting.Value);

            return type switch
            {
                _ when type == typeof(int) || type == typeof(int?) => (PropertyBase)new IntegerProperty(
                    name, (int?)value),
                _ when type == typeof(bool) || type == typeof(bool?) => new BooleanProperty(name, (bool?)value),
                _ when type == typeof(string) => new StringProperty(name, (string?)value),
                _ when type.IsPrimitive => new NumberProperty(
                    name, value is null ? null : (double?)Convert.ChangeType(value, typeof(double))),
                _ when type == typeof(DateTime) || type == typeof(DateTime?) => new DateTimeProperty(
                    name, (DateTime?)value),
                _ when type == typeof(TimeSpan) || type == typeof(TimeSpan?) => new DurationProperty(
                    name, (TimeSpan?)value),
                _ when type.IsEnum || Nullable.GetUnderlyingType(type)?.IsEnum == true => new EnumProperty(
                    name, Enum.GetName(Nullable.GetUnderlyingType(type) ?? type, value)!),
                _ when type == typeof(Guid) || type == typeof(Guid?) => new UuidProperty(
                    name, (Guid?)value),
                _ when type == typeof(Uri) => new UriProperty(name, (Uri?)value),
                _ when typeof(ICollection).IsAssignableFrom(type) =>
                    new ArrayProperty(name, JsonSerializer.SerializeToElement(value, type, Statics.SerializerOptions)),
                _ when type.IsClass => new ObjectProperty(
                    name, JsonSerializer.SerializeToElement(value, type, Statics.SerializerOptions)),
                _ => throw HttpException.NotFound("Property type not found", name),
            };
        });
    }

    [Route(HttpVerbs.Patch, $"{RootPath}/{{fullName}}/values")]
    public int Patch(string fullName, [JsonBody] IEnumerable<PropertyBase> properties)
    {
        if (!Statics.SettingManager.Settings.TryGetValue(fullName, out var setting))
            throw HttpException.NotFound($"Setting with fullName {fullName} not found", fullName);

        return properties.Select(property =>
        {
            void Throw() => throw HttpException.BadRequest("Invalid value", property);

            var propInfo =
                setting.Type.GetProperty(property.Name.Pascalize(), BindingFlags.Instance | BindingFlags.Public);

            if (propInfo is null) Throw();

            var type = propInfo!.PropertyType;
            var instance = setting.Value;

            switch (property)
            {
                case BooleanProperty booleanProperty when type == typeof(bool):
                    if (booleanProperty.Value is null) Throw();
                    propInfo.SetValue(instance, booleanProperty.Value!.Value);
                    break;
                case BooleanProperty booleanProperty when type == typeof(bool?):
                    propInfo.SetValue(instance, booleanProperty.Value);
                    break;
                case DateTimeProperty dateTimeProperty when type == typeof(DateTime):
                    if (dateTimeProperty.Value is null) Throw();
                    propInfo.SetValue(instance, dateTimeProperty.Value!.Value);
                    break;
                case DateTimeProperty dateTimeProperty when type == typeof(DateTime?):
                    propInfo.SetValue(instance, dateTimeProperty.Value);
                    break;
                case DurationProperty durationProperty when type == typeof(TimeSpan):
                    if (durationProperty.Value is null) Throw();
                    propInfo.SetValue(instance, durationProperty.Value!.Value);
                    break;
                case DurationProperty durationProperty when type == typeof(TimeSpan?):
                    propInfo.SetValue(instance, durationProperty.Value);
                    break;
                case EnumProperty enumProperty when type.IsEnum:
                    propInfo.SetValue(instance, Enum.Parse(type, enumProperty.Value, true));
                    break;
                case IntegerProperty integerProperty when type == typeof(int):
                    if (integerProperty.Value is null) Throw();
                    propInfo.SetValue(instance, integerProperty.Value!.Value);
                    break;
                case IntegerProperty integerProperty when type == typeof(int?):
                    propInfo.SetValue(instance, integerProperty.Value);
                    break;
                case NumberProperty numberProperty when type.IsPrimitive:
                    if (numberProperty.Value is null) Throw();
                    propInfo.SetValue(
                        instance,
                        type != typeof(double)
                            ? Convert.ChangeType(numberProperty.Value!.Value, type)
                            : numberProperty.Value!.Value);
                    break;
                case NumberProperty numberProperty when Nullable.GetUnderlyingType(type)?.IsPrimitive == true:
                    propInfo.SetValue(
                        instance,
                        type != typeof(double?)
                            ? Convert.ChangeType(numberProperty.Value, type)
                            : numberProperty.Value);
                    break;
                case ObjectProperty objectProperty when type.IsClass:
                    propInfo.SetValue(instance, objectProperty.Value.Deserialize(type, Statics.SerializerOptions));
                    break;
                case ArrayProperty arrayProperty when typeof(ICollection).IsAssignableFrom(type):
                    propInfo.SetValue(instance, arrayProperty.Value.Deserialize(type, Statics.SerializerOptions));
                    break;
                case StringProperty stringProperty when type == typeof(string):
                    propInfo.SetValue(instance, stringProperty.Value);
                    break;
                case UriProperty uriProperty when type == typeof(Uri):
                    propInfo.SetValue(instance, uriProperty.Value);
                    break;
                case UuidProperty uuidProperty when type == typeof(Guid):
                    if (uuidProperty.Value is null) Throw();
                    propInfo.SetValue(instance, uuidProperty.Value!.Value);
                    break;
                case UuidProperty uuidProperty when type == typeof(Guid?):
                    propInfo.SetValue(instance, uuidProperty.Value);
                    break;
                default:
                    Throw();
                    break;
            }

            return true;
        }).Count();
    }
}