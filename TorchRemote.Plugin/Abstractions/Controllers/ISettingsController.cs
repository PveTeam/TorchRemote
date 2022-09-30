using TorchRemote.Models.Responses;
namespace TorchRemote.Plugin.Abstractions.Controllers;

public interface ISettingsController
{
    SettingInfoResponse Get(string fullName);
}
