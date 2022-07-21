using TorchRemote.Models.Responses;
namespace TorchRemote.Plugin.Abstractions.Controllers;

public interface IWorldsController
{
    IEnumerable<Guid> Get();
    Guid GetSelected();
    WorldResponse GetWorld(Guid id);
    void Select(Guid id);
}
