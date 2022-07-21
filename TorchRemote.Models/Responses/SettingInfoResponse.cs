namespace TorchRemote.Models.Responses;

public record SettingInfoResponse(string Name, ICollection<SettingPropertyInfo> Properties);
public record SettingPropertyInfo(string Name, string? Description, int? Order, Guid Type);

public struct SettingPropertyTypeEnum
{
    public static readonly Guid Integer = new("95c0d25b-e44d-4505-9549-48ee9c14bce8");
    public static readonly Guid Boolean = new("028ef347-1fc3-486a-b70b-3d3b1dcdb538");
    public static readonly Guid Number = new("009ced71-4a69-4af0-abb9-ec3339fffce0");
    public static readonly Guid String = new("22dbed1b-b976-44b4-98c9-d1b742a93f0c");
    public static readonly Guid DateTime = new("f0978b29-9da9-4289-85c9-41d5b92056e8");
    public static readonly Guid TimeSpan = new("7a2bebf1-78f5-4e4e-8d83-18914dbee55c");
    public static readonly Guid Color = new("99c74632-0fa9-469b-ba05-825ba21a017b");
}