using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Torch.API.WebAPI;
using TorchRemote.Models.Responses;
using TorchRemote.Plugin.Utils;

namespace TorchRemote.Plugin.Controllers;

public class PluginDownloadsController : WebApiController
{
    private const string RootPath = "/plugins/downloads";

    [Route(HttpVerbs.Get, RootPath)]
    public async Task<IEnumerable<PluginInfo>> GetAsync()
    {
        var response = await PluginQuery.Instance.QueryAll();
        return response.Plugins.Select(b => new PluginItemInfo(Guid.Parse(b.ID), b.Name, b.LatestVersion, b.Author));
    }

    [Route(HttpVerbs.Get, $"{RootPath}/{{id}}")]
    public async Task<FullPluginItemInfo> GetFullAsync(Guid id)
    {
        var response = await PluginQuery.Instance.QueryOne(id);
        
        if (response is null)
            throw HttpException.NotFound("Plugin not found", id);

        return new(Guid.Parse(response.ID), response.Name, response.Description, response.LatestVersion,
                   response.Author);
    }

    [Route(HttpVerbs.Post, $"{RootPath}/{{id}}/install")]
    public async Task InstallAsync(Guid id)
    {
        if (Statics.PluginManager.Plugins.ContainsKey(id))
            throw HttpException.BadRequest("Plugin with given id already exists", id);
        
        var response = await PluginQuery.Instance.QueryOne(id);
        
        if (response is null)
            throw HttpException.NotFound("Plugin not found", id);

        if (!await PluginQuery.Instance.DownloadPlugin(response))
            throw HttpException.InternalServerError();
        
        Statics.Torch.Config.Plugins.Add(id);
    }
}