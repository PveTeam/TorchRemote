using System.Text.Json;
using System.Text.Json.Serialization;

namespace TorchRemote.Models.Shared.Settings;

[JsonDerivedType(typeof(IntegerProperty), "integer")]
[JsonDerivedType(typeof(StringProperty), "string")]
[JsonDerivedType(typeof(BooleanProperty), "boolean")]
[JsonDerivedType(typeof(NumberProperty), "number")]
[JsonDerivedType(typeof(ObjectProperty), "object")]
[JsonDerivedType(typeof(EnumProperty), "enum")]
[JsonDerivedType(typeof(DateTimeProperty), "date-time")]
[JsonDerivedType(typeof(DurationProperty), "duration")]
[JsonDerivedType(typeof(UriProperty), "uri")]
[JsonDerivedType(typeof(UuidProperty), "uuid")]
public abstract record PropertyBase(string Name);

public record IntegerProperty(string Name, int? Value) : PropertyBase(Name);
public record NumberProperty(string Name, double? Value) : PropertyBase(Name);
public record StringProperty(string Name, string? Value) : PropertyBase(Name);
public record EnumProperty(string Name, string Value) : PropertyBase(Name);
public record BooleanProperty(string Name, bool? Value) : PropertyBase(Name);
public record ObjectProperty(string Name, JsonElement Value) : PropertyBase(Name);
public record DateTimeProperty(string Name, DateTime? Value) : PropertyBase(Name);
public record DurationProperty(string Name, TimeSpan? Value) : PropertyBase(Name);
public record UriProperty(string Name, Uri? Value) : PropertyBase(Name);
public record UuidProperty(string Name, Guid? Value) : PropertyBase(Name);