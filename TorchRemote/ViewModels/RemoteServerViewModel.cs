using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json.Serialization;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TorchRemote.Services;
using TorchRemote.ViewModels.Server;
namespace TorchRemote.ViewModels;

public class RemoteServerViewModel : TabViewModelBase, IScreen
{
    private readonly ApiClientService _clientService = new();

    [Reactive]
    public override string Header { get; set; } = "Torch Server";

    public ObservableCollection<ServerNavItem> NavItems { get; set; }
    [Reactive]
    public ServerNavItem CurrentNavItem { get; set; }

    public RemoteServerViewModel()
    {
        var settingsViewModel = new SettingsViewModel(_clientService);
        NavItems = new()
        {
            new("Dashboard", Symbol.Home, new DashboardViewModel(_clientService)),
            new("Server Config", Symbol.Settings, new ServerConfigViewModel(_clientService)),
            new("Chat", Symbol.Message, new ChatViewModel(_clientService)),
            new("Players", Symbol.People, new PlayersViewModel(_clientService)),
            new("Settings", Symbol.More, settingsViewModel) {IsVisible = true}
        };
        CurrentNavItem = NavItems.Last();

        this.WhenAnyValue(x => x.CurrentNavItem)
            .Select<ServerNavItem, IRoutableViewModel>(b => b.ViewModel)
            .InvokeCommand(Router, x => x.Navigate);

        _clientService.Connected
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToPropertyEx(this, x => x.Connected);

        this.WhenAnyValue(x => x.Connected)
            .Where(b => b)
            .Subscribe(_ =>
            {
                foreach (var item in NavItems)
                {
                    item.IsVisible = true;
                }
                CurrentNavItem = NavItems[0];
            });
    }
    public RoutingState Router { get; set; } = new();

    [JsonIgnore]
    public extern bool Connected { [ObservableAsProperty] get; }
}
