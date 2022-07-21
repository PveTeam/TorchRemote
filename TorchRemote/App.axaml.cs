using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Splat;
using TorchRemote.ViewModels;
using TorchRemote.ViewModels.Server;
using TorchRemote.Views;
using TorchRemote.Views.Server;

namespace TorchRemote
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            Locator.CurrentMutable.RegisterConstant<IScreen>(new MainWindowViewModel());

            Locator.CurrentMutable.Register<IViewFor<RemoteServerViewModel>>(() => new RemoteServerView());
            Locator.CurrentMutable.Register<IViewFor<DashboardViewModel>>(() => new DashboardView());
            Locator.CurrentMutable.Register<IViewFor<ServerConfigViewModel>>(() => new ServerConfigView());
            Locator.CurrentMutable.Register<IViewFor<ChatViewModel>>(() => new ChatView());
            Locator.CurrentMutable.Register<IViewFor<PlayersViewModel>>(() => new PlayersView());
            Locator.CurrentMutable.Register<IViewFor<SettingsViewModel>>(() => new SettingsView());
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = Locator.Current.GetService<IScreen>(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
