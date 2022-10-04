namespace TorchRemote.Models.Responses;

public record PluginInfo(Guid Id, string Name, string Version);

public record PluginItemInfo(Guid Id, string Name, string Version, string Author) : PluginInfo(Id, Name, Version);

public record FullPluginItemInfo(Guid Id, string Name, string Description, string Version, string Author) : PluginItemInfo(Id, Name, Version, Author);
public record InstalledPluginInfo(Guid Id, string Name, string Version, string? SettingId) : PluginInfo(Id, Name, Version);