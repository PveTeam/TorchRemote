using Json.Schema;

namespace TorchRemote.Models.Responses;

public record SettingInfoResponse(string Name, JsonSchema Schema);