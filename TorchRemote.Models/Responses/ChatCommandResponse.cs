namespace TorchRemote.Models.Responses;

public record ChatCommandResponse(Guid Id, string Author, string Message) : ChatResponseBase;
