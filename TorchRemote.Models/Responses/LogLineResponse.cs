namespace TorchRemote.Models.Responses;

public record LogLineResponse(DateTime Time, LogLineLevel Level, string Logger, string Message);

public enum LogLineLevel
{
    Trace,
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}
