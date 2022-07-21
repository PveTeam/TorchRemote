using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Torch.API.Session;
using TorchRemote.Models.Requests;
using TorchRemote.Models.Responses;
using TorchRemote.Models.Shared;
using TorchRemote.Plugin.Utils;

namespace TorchRemote.Plugin.Controllers;

public class ServerController : WebApiController
{
    private const string RootPath = "/server";
    
    [Route(HttpVerbs.Get, $"{RootPath}/status")]
    public ServerStatusResponse GetStatus()
    {
        return new(Math.Round(Sync.ServerSimulationRatio, 2), 
            MySession.Static?.Players?.GetOnlinePlayerCount() ?? 0, 
            Statics.Torch.ElapsedPlayTime, 
            (ServerStatus)Statics.Torch.State);
    }

    [Route(HttpVerbs.Post, $"{RootPath}/start")]
    public void Start()
    {
        if (!Statics.Torch.CanRun)
            throw HttpException.BadRequest($"Server can't start in state {Statics.Torch.State}", Statics.Torch.State);
        
        Statics.Torch.Start();
    }

    [Route(HttpVerbs.Post, $"{RootPath}/stop")]
    public async Task Stop(StopServerRequest request)
    {
        if (!Statics.Torch.IsRunning)
            throw HttpException.BadRequest($"Server can't stop in state {Statics.Torch.State}", Statics.Torch.State);

        var saveResult = await Statics.Torch.Save(exclusive: true);
        if (saveResult is not GameSaveResult.Success)
            throw HttpException.InternalServerError($"Save resulted in {saveResult}", saveResult);
        
        Statics.Torch.Stop();
    }

    [Route(HttpVerbs.Get, $"{RootPath}/settings")]
    public ServerSettings GetSettings()
    {
        var settings = Statics.Torch.DedicatedInstance.DedicatedConfig;

        return new(settings.ServerName ?? "unamed",
            settings.WorldName ?? "unamed",
            settings.ServerDescription ?? string.Empty,
            settings.SessionSettings.MaxPlayers,
            new(settings.IP, settings.Port));
    }

    [Route(HttpVerbs.Post, $"{RootPath}/settings")]
    public async Task SetSettings([JsonData] ServerSettings request)
    {
        var settings = Statics.Torch.DedicatedInstance.DedicatedConfig;

        settings.ServerName = request.ServerName;
        settings.WorldName = request.MapName;
        settings.ServerDescription = request.ServerDescription;
        settings.SessionSettings.MaxPlayers = request.MemberLimit;
        settings.IP = request.ListenEndPoint.Ip;
        settings.Port = request.ListenEndPoint.Port;

        if (Statics.Torch.IsRunning)
            await Statics.Torch.InvokeAsync(request.ApplyDynamically);
    }
}
