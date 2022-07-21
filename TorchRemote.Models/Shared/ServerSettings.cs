namespace TorchRemote.Models.Shared;

public record ServerSettings(
    string ServerName,
    string MapName,
    string ServerDescription,
    short MemberLimit,
    IpAddress ListenEndPoint
);
