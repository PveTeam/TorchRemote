using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using TorchRemote.Models.Requests;
using TorchRemote.Models.Responses;
using TorchRemote.Models.Shared;
namespace TorchRemote.Services;

public interface IRemoteApi
{
#region Server
    [Get("/server/status")]
    Task<ServerStatusResponse> GetServerStatus();
    [Post("/server/start")]
    Task StartServer();
    [Post("/server/stop")]
    Task StopServer([Body] StopServerRequest request);
    [Get("/server/settings")]
    Task<ServerSettings> GetServerSettings();
    [Post("/server/settings")]
    Task SetServerSettings([Body] ServerSettings request);
#endregion

#region Chat
    [Post("/chat/message")]
    Task SendChatMessage([Body] ChatMessageRequest request);
    [Post("/chat/command")]
    Task<Guid> InvokeChatCommand([Body] ChatCommandRequest request);
#endregion

#region Worlds
    [Get("/worlds")]
    Task<IEnumerable<Guid>> GetWorlds();
    [Get("/worlds/selected")]
    Task<Guid> GetSelectedWorld();
    [Get("/worlds/{id}")]
    Task<WorldResponse> GetWorld(Guid id);
    [Post("/worlds/{id}/select")]
    Task SelectWorld(Guid id);
#endregion

#region Settings
    [Get("/settings/{id}")]
    Task<SettingInfoResponse> GetSetting(Guid id);
#endregion
}
