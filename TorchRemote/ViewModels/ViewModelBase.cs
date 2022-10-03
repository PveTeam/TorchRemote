using ReactiveUI;

namespace TorchRemote.ViewModels
{
    public class ViewModelBase : ReactiveObject, IRoutableViewModel
    {
        public string? UrlPathSegment { get; set; }
        public IScreen HostScreen { get; set; } = null!;
    }
}
