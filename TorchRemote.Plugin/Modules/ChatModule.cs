using System.Net.WebSockets;
using System.Text.Json;
using EmbedIO.WebSockets;
using TorchRemote.Models.Responses;
using TorchRemote.Plugin.Utils;
namespace TorchRemote.Plugin.Modules;

public class ChatModule : WebSocketModule
{
    public ChatModule(string urlPath, bool enableConnectionWatchdog) : base(urlPath, enableConnectionWatchdog)
    {
    }

    public async void SendChatResponse(ChatResponseBase response)
    {
        if (ActiveContexts.Count == 0)
            return;

        var buffer = JsonSerializer.SerializeToUtf8Bytes(response, Statics.SerializerOptions);
        await BroadcastAsync(buffer);
    }
    
    protected override async Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
    {
    }
}
