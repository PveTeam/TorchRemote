using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TorchRemote.Services;
namespace TorchRemote.ViewModels.Server;

public class SettingsViewModel : ViewModelBase
{
    private readonly ApiClientService _clientService;
    [Reactive]
    public string BearerToken { get; set; } = "WcdYT5qHjSt5Uzjs54xu8vE9Oq4a5MD2edLxywtJHtc=";
    [Reactive]
    public string RemoteUrl { get; set; } = "http://localhost";

    public SettingsViewModel(ApiClientService clientService)
    {
        _clientService = clientService;
        
        this.WhenValueChanged(x => x.BearerToken)
            .BindTo(_clientService, x => x.BearerToken);
        
        this.WhenValueChanged(x => x.RemoteUrl)
            .BindTo(_clientService, x => x.BaseUrl);
    }
}
