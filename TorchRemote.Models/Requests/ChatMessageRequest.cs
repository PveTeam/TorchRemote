using TorchRemote.Models.Shared;
namespace TorchRemote.Models.Requests;

public record ChatMessageRequest(string? Author, string Message, ChatChannel Channel, long? TargetId = null);