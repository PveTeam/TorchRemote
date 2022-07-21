using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Controls.Primitives;
namespace TorchRemote.Controls.Flyouts;
#nullable disable
public class MessageConfirmDismissFlyout : PickerFlyoutBase
{
    public static readonly DirectProperty<MessageConfirmDismissFlyout, ICommand> ResultCommandProperty =
        AvaloniaProperty.RegisterDirect<MessageConfirmDismissFlyout, ICommand>(nameof(ResultCommand), flyout => flyout._command!,
            (flyout, command) => flyout._command = command);

    public static readonly DirectProperty<MessageConfirmDismissFlyout, string> MessageProperty =
        AvaloniaProperty.RegisterDirect<MessageConfirmDismissFlyout, string>(nameof(Message), flyout => flyout._message, 
            (flyout, s) => flyout._message = s);
    
    public string Message
    {
        get => _message;
        set => SetAndRaise(MessageProperty, ref _message, value);
    }

    public ICommand ResultCommand
    {
        get => _command;
        set => SetAndRaise(ResultCommandProperty, ref _command, value);
    }

    private ICommand _command;
    private string _message;
    protected override Control CreatePresenter()
    {
        var pfp = new PickerFlyoutPresenter
        {
            Content = new TextBlock
            {
                Text = _message,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new(3)
            }
        };
        
        pfp.Confirmed += PfpOnConfirmed;
        pfp.Dismissed += PfpOnDismissed;
        
        return pfp;
    }
    private void PfpOnDismissed(PickerFlyoutPresenter sender, object args)
    {
        ResultCommand?.Execute(false);
        Hide();
    }
    private void PfpOnConfirmed(PickerFlyoutPresenter sender, object args)
    {
        OnConfirmed();
        Hide();
    }
    protected override void OnConfirmed()
    {
        ResultCommand?.Execute(true);
    }
}
