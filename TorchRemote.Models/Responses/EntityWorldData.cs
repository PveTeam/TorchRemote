using TorchRemote.Models.Shared;

namespace TorchRemote.Models.Responses;

public record EntityWorldData(Vector3 Position, Vector3 Forward, Vector3 Up, Vector3 LinearVelocity, Vector3 AngularVelocity);