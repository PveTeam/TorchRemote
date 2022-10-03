using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Refit;
using TorchRemote.Models.Responses;
using Websocket.Client;
using Websocket.Client.Models;

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
        get => _client.BaseAddress?.ToString() ?? $"http://localhost/api/{Version}/";
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

    public async Task<WebsocketFeed<ChatResponseBase>> WatchChatAsync() => 
        new(await StartWebsocketConnectionAsync("live/chat"));

    public async Task<WebsocketFeed<LogLineResponse>> WatchLogLinesAsync() => 
        new(await StartWebsocketConnectionAsync("live/logs"));

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

    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
}

public sealed class WebsocketFeed<TMessage> : IDisposable where TMessage : class
{
    private readonly WebsocketClient _client;

    public WebsocketFeed(WebsocketClient client)
    {
        _client = client;
        Disconnected = client.DisconnectionHappened;
        Connected = client.ReconnectionHappened;
        Messages = client.MessageReceived.Where(b => b.MessageType is WebSocketMessageType.Text)
                         .Select(b => JsonSerializer.Deserialize<TMessage>(b.Text, ApiClientService.SerializerOptions))
                         .Where(b => b is not null)!;
    }

    public IObservable<DisconnectionInfo> Disconnected { get; }

    public IObservable<ReconnectionInfo> Connected { get; }

    public IObservable<TMessage> Messages { get; }

    public void Dispose()
    {
        _client.Dispose();
    }
}
