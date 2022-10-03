using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Refit;
using TorchRemote.Models.Responses;
using TorchRemote.Services;
namespace TorchRemote.ViewModels.Server;

public class DashboardViewModel : ViewModelBase
{
    private readonly ApiClientService _clientService;
    public DashboardViewModel(ApiClientService clientService)
    {
        _clientService = clientService;

        _clientService.Connected
                      .ObserveOn(RxApp.MainThreadScheduler)
                      .Subscribe(_ =>
                      {
                          Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(10))
                                    .Select(_ => Observable.FromAsync(() => _clientService.Api.GetServerStatus()))
                                    .Concat()
                                    .ObserveOn(RxApp.MainThreadScheduler)
                                    .Subscribe(r =>
                                    {
                                        var (simSpeed, online, uptime, status) = r.Content!;
                                        SimSpeed = simSpeed;
                                        Status = status;
                                        Uptime = uptime;
                                        MemberCount = online;
                                    });

                          Observable.FromAsync(() => _clientService.WatchLogLinesAsync())
                                    .Select(b => b.Messages)
                                    .Concat()
                                    .Select(b => $"{b.Time:hh:mm:ss} [{b.Level}] {(b.Logger.Contains('.') ? b.Logger[(b.Logger.LastIndexOf('.') + 1)..] : b.Logger)}: {b.Message}")
                                    .ObserveOn(RxApp.MainThreadScheduler)
                                    .Subscribe(s =>
                                    {
                                        if (LogLines.Count(b => b == '\n') > 1000)
                                            LogLines = LogLines['\n'..];
                                        LogLines += $"{s}\n";
                                    });
                      });

        StartCommand = ReactiveCommand.CreateFromTask(() => _clientService.Api.StartServer(), 
            this.WhenAnyValue(x => x.Status)
                .Select(b => b is ServerStatus.Stopped));
        
        StopCommand = ReactiveCommand.CreateFromTask<bool, IApiResponse>(b => _clientService.Api.StopServer(new(b)), 
            this.WhenAnyValue(x => x.Status)
                .Select(b => b is ServerStatus.Running));
    }
    public ReactiveCommand<bool, IApiResponse> StopCommand { get; set; }
    public ReactiveCommand<Unit, IApiResponse> StartCommand { get; set; }

    [Reactive]
    public double SimSpeed { get; set; }
    [Reactive]
    public ServerStatus Status { get; set; }
    [Reactive]
    public TimeSpan Uptime { get; set; }
    [Reactive]
    public string LogLines { get; set; } = string.Empty;
    [Reactive]
    public int MemberCount { get; set; }
}
