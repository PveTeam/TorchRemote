using System.Text.Json.Serialization;

namespace TorchRemote.Models.Responses;

public record PluginInfo(Guid Id, string Name, string Version);
public record InstalledPluginInfo(Guid Id, string Name, string Version, string? SettingId) : PluginInfo(Id, Name, Version);

public record PluginItemInfo(
    [property: JsonPropertyName("guid")] Guid Id,
    string Name,
    string Author,
    string Description,
    int Downloads,
    bool Archived,
    bool Private,
    string LatestVersion,
    IReadOnlyList<string> Versions,
    string Icon
);