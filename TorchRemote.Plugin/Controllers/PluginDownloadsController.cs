using System.Net.Http;
using System.Text.Json;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Torch.API.WebAPI;
using TorchRemote.Models.Responses;
using TorchRemote.Plugin.Utils;

namespace TorchRemote.Plugin.Controllers;

public record PluginsResponse(IReadOnlyList<PluginItemInfo> Plugins);

public class PluginDownloadsController : WebApiController
{
    private const string RootPath = "/plugins/downloads";
    private const string BaseAddress = "https://torchapi.com/";

    [Route(HttpVerbs.Get, RootPath)]
    public async Task<IEnumerable<PluginItemInfo>> GetAsync()
    {
        using var client = new HttpClient()
        {
            BaseAddress = new(BaseAddress)
        };
        using var stream = await client.GetStreamAsync("api/plugins");
        
        var response = await JsonSerializer.DeserializeAsync<PluginsResponse>(stream, Statics.SerializerOptions);

        return response?.Plugins.Select(b => b with { Icon = BaseAddress + b.Icon }) ?? throw HttpException.InternalServerError("Torch site unavailable");
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
            throw HttpException.InternalServerError("Torch site unavailable");
        
        Statics.Torch.Config.Plugins.Add(id);
    }
}