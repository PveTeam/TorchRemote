using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Swan;
using TorchRemote.Models.Responses;
using TorchRemote.Plugin.Abstractions.Controllers;
using TorchRemote.Plugin.Utils;
namespace TorchRemote.Plugin.Controllers;

public class SettingsController : WebApiController, ISettingsController
{
    private const string RootPath = "/settings";

    [Route(HttpVerbs.Get, $"{RootPath}/{{id}}")]
    public SettingInfoResponse Get(Guid id)
    {
        if (!Statics.SettingManager.Settings.TryGetValue(id, out var setting))
            throw HttpException.NotFound($"Setting with id {id} not found", id);

        return new(setting.Name.Humanize(), setting.Properties.Select(b => 
            new SettingPropertyInfo(b.DisplayInfo?.Name ?? b.Name.Humanize(), 
                b.DisplayInfo?.Description, b.DisplayInfo?.Order, b.TypeId))
            .ToArray());
    }
}
