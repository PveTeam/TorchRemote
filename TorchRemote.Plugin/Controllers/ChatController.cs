using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using EmbedIO.WebSockets;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.Commands;
using Torch.Managers;
using Torch.Utils;
using TorchRemote.Models.Requests;
using TorchRemote.Models.Responses;
using TorchRemote.Models.Shared;
using TorchRemote.Plugin.Abstractions.Controllers;
using TorchRemote.Plugin.Modules;
using TorchRemote.Plugin.Utils;
using VRage.Network;
namespace TorchRemote.Plugin.Controllers;

public class ChatController : WebApiController, IChatController
{
    private const string RootPath = "/chat";

    [ReflectedMethodInfo(typeof(MyMultiplayerBase), "OnChatMessageReceived_BroadcastExcept")]
    private static readonly MethodInfo BroadcastExceptMethod = null!;
    [ReflectedMethodInfo(typeof(MyMultiplayerBase), "OnChatMessageReceived_SingleTarget")]
    private static readonly MethodInfo SingleTargetMethod = null!;
    
    [Route(HttpVerbs.Post, $"{RootPath}/message")]
    public void SendMessage([JsonData] ChatMessageRequest request)
    {
        if (MyMultiplayer.Static is null)
            throw new HttpException(HttpStatusCode.ServiceUnavailable);

        var msg = new ChatMsg
        {
            CustomAuthorName = request.Author ?? Statics.Torch.Config.ChatName,
            Text = request.Message,
            Channel = (byte)request.Channel,
            TargetId = request.TargetId.GetValueOrDefault()
        };

        switch (request.Channel)
        {
            case ChatChannel.Global:
            case ChatChannel.GlobalScripted when request.TargetId is null:
                NetworkManager.RaiseStaticEvent(BroadcastExceptMethod, msg);
                break;
            
            case ChatChannel.Private when request.TargetId is not null:
            case ChatChannel.GlobalScripted:
                var steamId = Sync.Players.TryGetSteamId(request.TargetId.Value);
                if (steamId == 0)
                    throw HttpException.NotFound($"Unable to find player with identity id {request.TargetId.Value}", request.TargetId.Value);
                
                NetworkManager.RaiseStaticEvent(SingleTargetMethod, msg, new(steamId));
                break;
            
            case ChatChannel.Faction when request.TargetId is not null:
                var faction = MySession.Static.Factions.TryGetFactionById(request.TargetId.Value);
                if (faction is null)
                    throw HttpException.NotFound($"Unable to find faction with id {request.TargetId.Value}", request.TargetId.Value);
                
                foreach (var playerId in faction.Members.Keys.Where(Sync.Players.IsPlayerOnline))
                {
                    NetworkManager.RaiseStaticEvent(SingleTargetMethod, msg, new(Sync.Players.TryGetSteamId(playerId)));
                }
                break;
            
            default:
                throw HttpException.BadRequest("Invalid channel and targetId combination");
        }
        
        if (Statics.Torch.CurrentSession?.Managers.GetManager<IChatManagerServer>() is { } manager && 
            request.Channel is ChatChannel.Global or ChatChannel.GlobalScripted)
            manager.DisplayMessageOnSelf(msg.CustomAuthorName, msg.Text);
    }

    [Route(HttpVerbs.Post, $"{RootPath}/command")]
    public async Task<Guid> InvokeCommand([JsonData] ChatCommandRequest request)
    {
        if (Statics.CommandManager is null)
            throw new HttpException(HttpStatusCode.ServiceUnavailable);

        if (Statics.CommandManager.Commands.GetCommand(request.Command, out var argText) is not { } command)
            throw HttpException.NotFound($"Unable to find command {request.Command}", request.Command);

        var argsList = Regex.Matches(argText, "(\"[^\"]+\"|\\S+)").Cast<Match>().Select(x => x.ToString().Replace("\"", "")).ToList();

        var id = Guid.NewGuid();
        var context = new WebSocketCommandContext(Statics.Torch, command.Plugin, argText, argsList, Statics.ChatModule, id);
        
        if (await Statics.Torch.InvokeAsync(() => command.TryInvoke(context)))
            return id;
        
        throw HttpException.BadRequest("Invalid syntax", request.Command);
    }
}

internal class WebSocketCommandContext : CommandContext
{
    private readonly ChatModule _module;
    private readonly Guid _id;
    public WebSocketCommandContext(ITorchBase torch, ITorchPlugin plugin, string rawArgs, List<string> args, ChatModule module, Guid id) : base(torch, plugin, Sync.MyId, rawArgs, args)
    {
        _module = module;
        _id = id;
    }

    public override void Respond(string message, string? sender = null, string? font = null)
    {
        _module.SendChatResponse(new ChatCommandResponse(_id, sender ?? Torch.Config.ChatName, message));
    }
}