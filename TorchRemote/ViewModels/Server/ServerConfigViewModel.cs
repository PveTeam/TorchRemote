using System;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TorchRemote.Services;
namespace TorchRemote.ViewModels.Server;

public class ServerConfigViewModel : ViewModelBase
{
    public ServerConfigViewModel(ApiClientService clientService)
    {
        Observable.FromEventPattern(clientService, nameof(clientService.Connected))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(_ => Observable.FromAsync(clientService.Api.GetServerSettings))
            .Concat()
            .Subscribe(b =>
            {
                Name = b.ServerName;
                MapName = b.MapName;
                MemberLimit = b.MemberLimit;
                Description = b.ServerDescription;
                Ip = b.ListenEndPoint.Ip;
                Port = b.ListenEndPoint.Port;
            });

        SaveCommand = ReactiveCommand.CreateFromTask(() => 
            clientService.Api.SetServerSettings(new(
                Name, 
                MapName,
                Description,
                MemberLimit, 
                new(Ip, Port)
                )));

        Worlds = Observable.FromEventPattern(clientService, nameof(clientService.Connected))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(_ => Observable.FromAsync(clientService.Api.GetWorlds))
            .Concat()
            .SelectMany(ids => ids)
            .Select(id => Observable.FromAsync(() => clientService.Api.GetWorld(id)).Select(b => new World(id, b.Name, b.SizeKb)))
            .Concat();

        Observable.FromEventPattern(clientService, nameof(clientService.Connected))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(_ => Observable.FromAsync(clientService.Api.GetSelectedWorld))
            .Concat()
            .Select(id => Observable.FromAsync(() => clientService.Api.GetWorld(id)).Select(b => new World(id, b.Name, b.SizeKb)))
            .Concat()
            .BindTo(this, x => x.SelectedWorld);

        this.ObservableForProperty(x => x.SelectedWorld)
            .Select(world => Observable.FromAsync(() => clientService.Api.SelectWorld(world.Value!.Id)))
            .Concat()
            .Subscribe(_ => { });
    }
    public ReactiveCommand<Unit,Unit> SaveCommand { get; set; }

    [Reactive]
    public string Name { get; set; } = null!;
    [Reactive]
    public string MapName { get; set; } = null!;
    [Reactive]
    public string Description { get; set; } = null!;
    [Reactive]
    public short MemberLimit { get; set; }
    [Reactive]
    public string Ip { get; set; } = null!;
    [Reactive]
    public int Port { get; set; }

    public IObservable<World> Worlds { get; set; }
    [Reactive]
    public World? SelectedWorld { get; set; }
}

public class World : ReactiveObject
{
    public World(Guid id, string name, long sizeKb)
    {
        Id = id;
        Name = name;
        SizeKb = sizeKb;
    }
    public Guid Id { get; set; }
    public string Name { get; set; }
    public long SizeKb { get; set; }

    public string SizeString => SizeKb > 1024 ? $"{SizeKb / 1024:N1} MB" : $"{SizeKb:N1} KB";
}
