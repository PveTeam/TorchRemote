using System.Reactive.Linq;
using System.Text.Json.Serialization;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
namespace TorchRemote.ViewModels.Server;

public class ServerNavItem : ReactiveObject
{
    public ServerNavItem(string title, Symbol icon, ViewModelBase viewModel)
    {
        Title = title;
        Icon = icon;
        ViewModel = viewModel;
        
        this.WhenAnyValue(x => x.Icon)
            .Select(b => new SymbolIcon {Symbol = b})
            .BindTo(this, x => x.IconElement);
    }
    public string Title { get; set; }
    public Symbol Icon { get; set; }
    public ViewModelBase ViewModel { get; set; }
    [JsonIgnore]
    [Reactive]
    public bool IsVisible { get; set; }
    [JsonIgnore]
    public IconElement IconElement { get; set; } = null!;
}
