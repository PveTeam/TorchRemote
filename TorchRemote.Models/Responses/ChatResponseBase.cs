using System.Text.Json.Serialization;
namespace TorchRemote.Models.Responses;

[JsonDerivedType(typeof(ChatCommandResponse), "command")]
[JsonDerivedType(typeof(ChatMessageResponse), "message")]
public record ChatResponseBase();
