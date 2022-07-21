using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using TorchRemote.Models.Requests;
using TorchRemote.Models.Responses;
using TorchRemote.Models.Shared;
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
        Task.Run(ConnectionTimer);
    }

    private async Task ConnectionTimer()
    {
        while (true)
        {
            await Task.Delay(1000);
            try
            {
                await GetServerStatusAsync(CancellationToken.None);
                break;
            }
            catch
            {
            }
        }
        
        Connected?.Invoke(this, EventArgs.Empty);
    }

    public Task<ServerStatusResponse> GetServerStatusAsync(CancellationToken token) =>
        _client.GetFromJsonAsync<ServerStatusResponse>("server/status", token)!;
    
    public Task<ServerSettings> GetServerSettingsAsync(CancellationToken token) =>
        _client.GetFromJsonAsync<ServerSettings>("server/settings", token)!;
    
    public Task SetServerSettingsAsync(ServerSettings settings, CancellationToken token) => 
        _client.PostAsJsonAsync("server/settings", settings, token);
    
    public Task StartServerAsync(CancellationToken token) => 
        _client.PostAsync("server/start", null, token);
    
    public Task StopServerAsync(StopServerRequest request, CancellationToken token) =>
        _client.PostAsJsonAsync("server/stop", request, token);
    
    public Task<IEnumerable<Guid>> GetWorldsAsync(CancellationToken token) =>
        _client.GetFromJsonAsync<IEnumerable<Guid>>("worlds", token)!;

    public Task<WorldResponse> GetWorldAsync(Guid id, CancellationToken token) =>
        _client.GetFromJsonAsync<WorldResponse>($"worlds/{id}", token)!;

    public Task<Guid> GetSelectedWorld(CancellationToken token) =>
        _client.GetFromJsonAsync<Guid>("worlds/selected", token);
    
    public Task SelectWorldAsync(Guid id, CancellationToken token) =>
        _client.PostAsync($"worlds/{id}/select", null, token);
    
    public Task SendChatMessageAsync(ChatMessageRequest request, CancellationToken token) =>
        _client.PostAsJsonAsync("chat/message", request, token);

    public async Task<Guid> InvokeCommandAsync(ChatCommandRequest request, CancellationToken token)
    {
        var r = await _client.PostAsJsonAsync("chat/command", request, token);
        r.EnsureSuccessStatusCode();
        return await r.Content.ReadFromJsonAsync<Guid>(cancellationToken: token);
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
