using Json.Schema;

namespace TorchRemote.Models.Responses;

public record SettingInfoResponse(string Name, JsonSchema Schema, ICollection<SettingPropertyInfo> PropertyInfos);
public record SettingPropertyInfo(string Name, string PropertyName, string? Description, int? Order);