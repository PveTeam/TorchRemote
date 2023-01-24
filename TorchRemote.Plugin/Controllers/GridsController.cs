using System.Net;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Sandbox.Game.Entities;
using Torch.API;
using TorchRemote.Models.Responses;
using TorchRemote.Plugin.Utils;
using VRageMath;
using Vector3 = TorchRemote.Models.Shared.Vector3;

namespace TorchRemote.Plugin.Controllers;

public class GridsController : WebApiController
{
    private const string RootPath = "/grids";

    [Route(HttpVerbs.Get, RootPath)]
    public Task<IEnumerable<long>> GetAsync()
    {
        if (Statics.Torch.GameState is not TorchGameState.Loaded)
            throw new HttpException(HttpStatusCode.ServiceUnavailable);
        
        return Statics.Torch.InvokeAsync<IEnumerable<long>>(() =>
        {
            return MyCubeGridGroups.Static.Physical.Groups.SelectMany(b => b.Nodes.Select(n => n.NodeData.EntityId))
                                   .ToArray();
        });
    }

    [Route(HttpVerbs.Get, $"{RootPath}/{{id}}")]
    public Task<GridInfo> GetAsync(long id)
    {
        if (Statics.Torch.GameState is not TorchGameState.Loaded)
            throw new HttpException(HttpStatusCode.ServiceUnavailable);
        
        return Statics.Torch.InvokeAsync(() =>
        {
            if (!MyEntities.TryGetEntityById(id, out MyCubeGrid grid))
                throw HttpException.NotFound("Grid with given id does not exist", id);

            return GetInfo(grid);
        });
    }

    [Route(HttpVerbs.Post, $"{RootPath}/{{id}}/power")]
    public Task PostPowerAsync(long id, [QueryField] bool powered)
    {
        if (Statics.Torch.GameState is not TorchGameState.Loaded)
            throw new HttpException(HttpStatusCode.ServiceUnavailable);
        
        return Statics.Torch.InvokeAsync(() =>
        {
            if (!MyEntities.TryGetEntityById(id, out MyCubeGrid grid))
                throw HttpException.NotFound("Grid with given id does not exist", id);

            if (grid.IsPowered != powered)
                grid.SwitchPower();
        });
    }

    [Route(HttpVerbs.Delete, $"{RootPath}/{{id}}")]
    public Task DeleteAsync(long id)
    {
        if (Statics.Torch.GameState is not TorchGameState.Loaded)
            throw new HttpException(HttpStatusCode.ServiceUnavailable);
        
        return Statics.Torch.InvokeAsync(() =>
        {
            if (!MyEntities.TryGetEntityById(id, out MyCubeGrid grid))
                throw HttpException.NotFound("Grid with given id does not exist", id);
            
            grid.Close();
        });
    }
    
    [Route(HttpVerbs.Delete, $"{RootPath}/{{id}}/group")]
    public Task DeleteGroupAsync(long id)
    {
        if (Statics.Torch.GameState is not TorchGameState.Loaded)
            throw new HttpException(HttpStatusCode.ServiceUnavailable);
        
        return Statics.Torch.InvokeAsync(() =>
        {
            if (!MyEntities.TryGetEntityById(id, out MyCubeGrid grid))
                throw HttpException.NotFound("Grid with given id does not exist", id);

            foreach (var node in MyCubeGridGroups.Static.Physical.GetGroup(grid).Nodes.ToArray())
            {
                node.NodeData.Close();
            }
        });
    }
    
    [Route(HttpVerbs.Get, $"{RootPath}/{{id}}/group")]
    public Task<IEnumerable<GridInfo>> GetGroupAsync(long id)
    {
        if (Statics.Torch.GameState is not TorchGameState.Loaded)
            throw new HttpException(HttpStatusCode.ServiceUnavailable);
        
        return Statics.Torch.InvokeAsync<IEnumerable<GridInfo>>(() =>
        {
            if (!MyEntities.TryGetEntityById(id, out MyCubeGrid grid))
                throw HttpException.NotFound("Grid with given id does not exist", id);

            return MyCubeGridGroups.Static.Physical.GetGroup(grid).Nodes.Select(n => GetInfo(n.NodeData)).ToArray();
        });
    }
    
    private static GridInfo GetInfo(MyCubeGrid grid)
    {
        var matrixD = grid.PositionComp?.WorldMatrixRef ?? MatrixD.Zero;
        return new GridInfo(grid.EntityId, grid.DisplayName,
                            new(matrixD.Translation.ToNumerics(), matrixD.Forward.ToNumerics(),
                                matrixD.Up.ToNumerics(), grid.Physics?.LinearVelocity.ToNumerics() ?? Vector3.Zero,
                                grid.Physics?.AngularVelocity.ToNumerics() ?? Vector3.Zero),
                            grid.BigOwners.Count > 0 ? grid.BigOwners[0] : null,
                            grid.BigOwners.Count + grid.SmallOwners.Count > 0 ? grid.BigOwners.Concat(grid.SmallOwners).ToArray() : null,
                            grid.BlocksCount, grid.BlocksPCU);
    }
}