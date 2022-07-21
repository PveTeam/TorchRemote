using TorchRemote.Models.Shared;
namespace TorchRemote.Models.Responses;

public record ChatMessageResponse(string AuthorName, ulong? Author, ChatChannel Channel, string Message) : ChatResponseBase;
