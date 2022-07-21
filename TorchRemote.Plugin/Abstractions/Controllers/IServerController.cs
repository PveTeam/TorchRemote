using TorchRemote.Models.Requests;
using TorchRemote.Models.Responses;
using TorchRemote.Models.Shared;
namespace TorchRemote.Plugin.Abstractions.Controllers;

public interface IServerController
{
    ServerStatusResponse GetStatus();
    Task Start();
    Task Stop(StopServerRequest request);
    ServerSettings GetSettings();
    Task SetSettings(ServerSettings request);
}
