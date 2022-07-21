using System.Collections.ObjectModel;
using ReactiveUI;
namespace TorchRemote.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    public ObservableCollection<TabViewModelBase> Tabs { get; set; } = new()
    {
        new RemoteServerViewModel()
    };

    public RoutingState Router { get; set; } = new();
}