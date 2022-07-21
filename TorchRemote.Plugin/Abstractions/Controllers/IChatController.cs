using TorchRemote.Models.Requests;
namespace TorchRemote.Plugin.Abstractions.Controllers;

public interface IChatController
{
    void SendMessage(ChatMessageRequest request);
    Task<Guid> InvokeCommand(ChatCommandRequest request);
}
