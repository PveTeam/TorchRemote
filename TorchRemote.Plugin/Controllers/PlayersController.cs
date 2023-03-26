using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Multiplayer;
using TorchRemote.Models.Responses;
using TorchRemote.Plugin.Utils;

namespace TorchRemote.Plugin.Controllers;

public class PlayersController : WebApiController
{
    private const string RootPath = "/players";

    [Route(HttpVerbs.Get, RootPath)]
    public Task<IEnumerable<PlayerResponse>> Get()
    {
        return Statics.Torch.InvokeAsync(() => Sync.Players.GetOnlinePlayers()
                                                   .Select(b => new PlayerResponse(b.Id.SteamId, b.DisplayName,
                                                               (PlayerPromoteLevel)Statics.MultiplayerManager!
                                                                   .GetUserPromoteLevel(b.Id.SteamId))));
    }

    [Route(HttpVerbs.Post, $"{RootPath}/{{id}}/kick")]
    public Task Kick(ulong id, [QueryField] bool cooldown = true)
    {
        return Statics.Torch.InvokeAsync(() => MyMultiplayer.Static.KickClient(id, true, cooldown));
    }

    [Route(HttpVerbs.Post, $"{RootPath}/{{id}}/ban")]
    public void Ban(ulong id)
    {
        Statics.MultiplayerManager!.BanPlayer(id);
    }
    
    [Route(HttpVerbs.Post, $"{RootPath}/{{id}}/unban")]
    public void UnBan(ulong id)
    {
        Statics.MultiplayerManager!.BanPlayer(id, false);
    }

    [Route(HttpVerbs.Get, $"{RootPath}/{{id}}/banned")]
    public IEnumerable<ulong> Banned()
    {
        return MySandboxGame.ConfigDedicated.Banned;
    }
    
    [Route(HttpVerbs.Post, $"{RootPath}/{{id}}/disconnect")]
    public Task Disconnect(ulong id)
    {
        return Statics.Torch.InvokeAsync(() => MyMultiplayer.Static.DisconnectClient(id));
    }
    
    [Route(HttpVerbs.Post, $"{RootPath}/{{id}}/promote")]
    public void Promote(ulong id)
    {
        Statics.MultiplayerManager!.PromoteUser(id);
    }
    
    [Route(HttpVerbs.Post, $"{RootPath}/{{id}}/demote")]
    public void Demote(ulong id)
    {
        Statics.MultiplayerManager!.DemoteUser(id);
    }
}