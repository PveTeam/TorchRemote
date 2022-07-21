﻿using System;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TorchRemote.Models.Responses;
using TorchRemote.Models.Shared;
using TorchRemote.Services;
namespace TorchRemote.ViewModels.Server;

public class ChatViewModel : ViewModelBase
{
    public ChatViewModel(ApiClientService clientService)
    {
        Observable.FromEventPattern(clientService, nameof(clientService.Connected))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ =>
            {
                var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                Observable.FromAsync(clientService.WatchChatAsync)
                    .Select(b => b.MessageReceived)
                    .Concat()
                    .Select(b => JsonSerializer.Deserialize<ChatResponseBase>(b.Text, options))
                    .Select(b => b switch
                    {
                        ChatMessageResponse msg => $"[{msg.Channel}] {msg.AuthorName}: {msg.Message}",
                        ChatCommandResponse cmd => $"[Command] {cmd.Author}: {cmd.Message}",
                        _ => throw new ArgumentOutOfRangeException(nameof(b), b, null)
                    })
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(s => ChatLines += $"{s}{Environment.NewLine}");
            });
        
        SendMessageCommand = ReactiveCommand.CreateFromTask<string>((s, t) => s.StartsWith("!") ?
            clientService.InvokeCommandAsync(new(s[(s.IndexOf('!') + 1)..]), t) :
            clientService.SendChatMessageAsync(new("Server", s, ChatChannel.GlobalScripted), t));

        InvalidCommandPopup = SendMessageCommand.ThrownExceptions
            .Where(b => b is HttpRequestException {StatusCode: HttpStatusCode.NotFound or HttpStatusCode.BadRequest})
            .Select(_ => true);
    }
    [Reactive]
    public string ChatLines { get; set; } = string.Empty;
    
    public ReactiveCommand<string, Unit> SendMessageCommand { get; set; }
    
    public IObservable<bool> InvalidCommandPopup { get; set; }
}
