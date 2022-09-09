using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Refit;
using Websocket.Client;
namespace TorchRemote.Services;

public class ApiClientService : IDisposable
{
    public const string Version = "v1";
    public string BearerToken
    {
        get => _client.DefaultRequestHeaders.Authorization?.Parameter ?? "*****";
        set => _client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {value}");
    }
    private readonly HttpClient _client = new();
    private readonly CancellationTokenSource _tokenSource = new();
    public string BaseUrl
    {
        get => _client.BaseAddress?.ToString() ?? "http://localhost";
        set => _client.BaseAddress = new($"{value}/api/{Version}/");
    }

    public IObservable<bool> Connected { get; }

    public ApiClientService()
    {
        Api = RestService.For<IRemoteApi>(_client);
        Connected = ConnectionTimer(_tokenSource.Token).ToObservable();
    }
    public IRemoteApi Api { get; }

    private async IAsyncEnumerable<bool> ConnectionTimer([EnumeratorCancellation] CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(1000, token);
            var success = (await Api.GetServerStatus()).Error is null;
            yield return success;
            if (success)
                await Task.Delay(TimeSpan.FromSeconds(30), token);
        }
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
    public void Dispose()
    {
        _client.Dispose();
        _tokenSource.Cancel();
        _tokenSource.Dispose();
    }
}
