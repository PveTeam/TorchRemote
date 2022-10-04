namespace TorchRemote.Models.Requests;

public record ChatCommandRequest(string Command, bool Streamed = false, TimeSpan? StreamingDuration = null);