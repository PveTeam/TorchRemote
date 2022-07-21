using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Refit;
using Websocket.Client;
namespace TorchRemote.Services;

public class ApiClientService
{
    public const string Version = "v1";
    public string BearerToken
    {
        get => _client.DefaultRequestHeaders.Authorization?.Parameter ?? "*****";
        set => _client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {value}");
    }
    private readonly HttpClient _client = new();
    public string BaseUrl
    {
        get => _client.BaseAddress?.ToString() ?? "http://localhost";
        set => _client.BaseAddress = new($"{value}/api/{Version}/");
    }

    public event EventHandler? Connected;

    public ApiClientService()
    {
        Api = RestService.For<IRemoteApi>(_client);
        Task.Run(ConnectionTimer);
    }
    public IRemoteApi Api { get; }

    private async Task ConnectionTimer()
    {
        while (true)
        {
            await Task.Delay(1000);
            try
            {
                await Api.GetServerStatus();
                break;
            }
            catch
            {
            }
        }
        
        Connected?.Invoke(this, EventArgs.Empty);
    }

    public Task<WebsocketClient> WatchChatAsync() => StartWebsocketConnectionAsync("live/chat");

    public Task<WebsocketClient> WatchLogLinesAsync() => StartWebsocketConnectionAsync("live/logs");

    private async Task<WebsocketClient> StartWebsocketConnectionAsync(string url)
    {
        var client = new WebsocketClient(new($"{BaseUrl}{url}"
                .Replace($"/{Version}", string.Empty)
                .Replace("http", "ws")), 
            () =>
            {
                var socket = new ClientWebSocket();
                socket.Options.SetRequestHeader("Authorization", $"Bearer {BearerToken}");
                return socket;
            })
        {
            ReconnectTimeout = null,
            ErrorReconnectTimeout = TimeSpan.FromSeconds(10)
        };

        await client.Start();
        return client;
    }
}
