using System.Text.Json.Serialization;
namespace TorchRemote.ViewModels;

[JsonDerivedType(typeof(RemoteServerViewModel))]
public abstract class TabViewModelBase : ViewModelBase
{
    public abstract string Header { get; set; }
}
