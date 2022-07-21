using System.Net;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using TorchRemote.Models.Responses;
using TorchRemote.Plugin.Abstractions.Controllers;
using TorchRemote.Plugin.Utils;
namespace TorchRemote.Plugin.Controllers;

public class WorldsController : WebApiController, IWorldsController
{
    private const string RootPath = "/worlds";

    [Route(HttpVerbs.Get, RootPath)]
    public IEnumerable<Guid> Get()
    {
        var config = Statics.InstanceManager.DedicatedConfig;
        
        if (config is null)
            throw new HttpException(HttpStatusCode.ServiceUnavailable);

        return config.Worlds.Select(b => b.FolderName.ToGuid());
    }

    [Route(HttpVerbs.Get, $"{RootPath}/selected")]
    public Guid GetSelected()
    {
        if (Statics.InstanceManager.DedicatedConfig?.SelectedWorld is not { } world)
            throw new HttpException(HttpStatusCode.ServiceUnavailable);

        return world.FolderName.ToGuid();
    }
    
    [Route(HttpVerbs.Get, $"{RootPath}/{{id}}")]
    public WorldResponse GetWorld(Guid id)
    {
        var config = Statics.InstanceManager.DedicatedConfig;
        
        if (config is null)
            throw new HttpException(HttpStatusCode.ServiceUnavailable);
        
        if (config.Worlds.FirstOrDefault(b => b.FolderName.ToGuid() == id) is not { } world)
            throw HttpException.NotFound($"World not found by given id {id}", id);

        return new(world.FolderName, world.WorldSizeKB);
    }

    [Route(HttpVerbs.Post, $"{RootPath}/{{id}}/select")]
    public void Select(Guid id)
    {
        var config = Statics.InstanceManager.DedicatedConfig;
        
        if (config is null)
            throw new HttpException(HttpStatusCode.ServiceUnavailable);
        
        if (config.Worlds.FirstOrDefault(b => b.FolderName.ToGuid() == id) is not { } world)
            throw HttpException.NotFound($"World not found by given id {id}", id);

        config.Model.IgnoreLastSession = true;
        config.SelectedWorld = world;
        config.Save();
    }
}
