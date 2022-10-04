using System.IO;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using HttpMultipartParser;
using Swan;
using Torch.API.Managers;
using Torch.Managers;
using TorchRemote.Models.Responses;
using TorchRemote.Plugin.Utils;

namespace TorchRemote.Plugin.Controllers;

public class PluginsController : WebApiController
{
    private const string RootPath = "/plugins";

    [Route(HttpVerbs.Get, RootPath)]
    public IEnumerable<InstalledPluginInfo> Get()
    {
        return Statics.PluginManager.Select(plugin => new InstalledPluginInfo(plugin.Id, plugin.Name, plugin.Version,
                                                                              Statics.SettingManager.PluginSettings!
                                                                                  .GetValueOrDefault(plugin.Id)));
    }

    [Route(HttpVerbs.Delete, $"{RootPath}/{{id}}")]
    public void Uninstall(Guid id)
    {
        foreach (var zip in Directory.EnumerateFiles(Statics.PluginManager.PluginDir, "*.zip"))
        {
            var manifest = PluginManifestUtils.ReadFromZip(zip);
            if (manifest.Guid != id)
                continue;
            
            File.Delete(zip);
            return;
        }

        throw HttpException.NotFound("Plugin zip with given id not found", id);
    }

    [Route(HttpVerbs.Put, RootPath)]
    public async Task<IEnumerable<PluginInfo>> InstallAsync()
    {
        var payload = await MultipartFormDataParser.ParseAsync(Request.InputStream);

        var pluginsToInstall = payload.Files.ToDictionary(f => PluginManifestUtils.ReadFromZip(f.Data));
        
        if (pluginsToInstall.Keys.FirstOrDefault(m => Statics.PluginManager.Plugins.ContainsKey(m.Guid)) is { } m)
            throw HttpException.BadRequest("Plugin with given id already exists", m.Guid);

        return pluginsToInstall.Select(b =>
        {
            var (manifest, file) = b;

            var path = Path.Combine(Statics.PluginManager.PluginDir, $"{manifest.Name}.zip");

            using var zipStream = File.Create(path);
            
            file.Data.Position = 0;
            file.Data.CopyTo(zipStream);
            
            return new PluginInfo(manifest.Guid, manifest.Name, manifest.Version);
        });
    }
}