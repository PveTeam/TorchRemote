namespace TorchRemote.Models.Responses;

public record ServerStatusResponse(double SimSpeed, int MemberCount, TimeSpan Uptime, ServerStatus Status);

public enum ServerStatus
{
    /// <summary>The server is not running.</summary>
    Stopped,
    /// <summary>The server is starting/loading the session.</summary>
    Starting,
    /// <summary>The server is running.</summary>
    Running,
    /// <summary>The server encountered an error.</summary>
    Error,
}
