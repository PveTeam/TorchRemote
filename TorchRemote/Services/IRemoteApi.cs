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
    Task<IApiResponse<ServerStatusResponse>> GetServerStatus();
    [Post("/server/start")]
    Task<IApiResponse> StartServer();
    [Post("/server/stop")]
    Task<IApiResponse> StopServer([Body] StopServerRequest request);
    [Get("/server/settings")]
    Task<IApiResponse<ServerSettings>> GetServerSettings();
    [Post("/server/settings")]
    Task<IApiResponse> SetServerSettings([Body] ServerSettings request);
#endregion

#region Chat
    [Post("/chat/message")]
    Task<IApiResponse> SendChatMessage([Body] ChatMessageRequest request);
    [Post("/chat/command")]
    Task<IApiResponse<Guid>> InvokeChatCommand([Body] ChatCommandRequest request);
#endregion

#region Worlds
    [Get("/worlds")]
    Task<IApiResponse<IEnumerable<Guid>>> GetWorlds();
    [Get("/worlds/selected")]
    Task<IApiResponse<Guid>> GetSelectedWorld();
    [Get("/worlds/{id}")]
    Task<IApiResponse<WorldResponse>> GetWorld(Guid id);
    [Post("/worlds/{id}/select")]
    Task<IApiResponse<IApiResponse>> SelectWorld(Guid id);
#endregion

#region Settings
    [Get("/settings/{id}")]
    Task<IApiResponse<SettingInfoResponse>> GetSetting(Guid id);
#endregion
}
