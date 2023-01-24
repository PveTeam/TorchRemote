namespace TorchRemote.Models.Shared;

public record Vector3(float X, float Y, float Z)
{
    public static Vector3 Zero => new Vector3(0, 0, 0);
}