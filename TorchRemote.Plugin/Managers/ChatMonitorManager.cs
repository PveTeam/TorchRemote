using Sandbox.Engine.Multiplayer;
using Torch.API;
using Torch.API.Managers;
using Torch.Managers;
using TorchRemote.Models.Responses;
using TorchRemote.Models.Shared;
using TorchRemote.Plugin.Utils;
namespace TorchRemote.Plugin.Managers;

public class ChatMonitorManager : Manager
{
    [Dependency]
    private readonly IChatManagerServer _chatManager = null!;
    public ChatMonitorManager(ITorchBase torchInstance) : base(torchInstance)
    {
    }

    public override void Attach()
    {
        base.Attach();
        _chatManager.MessageRecieved += ChatManagerOnMessageReceived;
    }
    private void ChatManagerOnMessageReceived(TorchChatMessage msg, ref bool consumed)
    {
        Statics.ChatModule.SendChatResponse(new ChatMessageResponse(msg.Author ?? (msg.AuthorSteamId is null ? Torch.Config.ChatName : MyMultiplayer.Static.GetMemberName(msg.AuthorSteamId.Value)), 
            msg.AuthorSteamId, (ChatChannel)msg.Channel, msg.Message));
    }
}
