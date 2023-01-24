namespace TorchRemote.Models.Responses;

public record GridInfo(long Id, 
                       string Name, 
                       EntityWorldData WorldData, 
                       long? BiggestOwner, 
                       ICollection<long>? Owners,
                       int BlockCount,
                       int Pcu);