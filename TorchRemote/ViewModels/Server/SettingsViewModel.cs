using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TorchRemote.Services;
namespace TorchRemote.ViewModels.Server;

public class SettingsViewModel : ViewModelBase
{
    private readonly ApiClientService _clientService;
    [Reactive]
    public string BearerToken { get; set; } = "NSN9qSbvKO6PtvoUg+fV5CrSpLqz+F2ssXvzbFbgOpE=";
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
