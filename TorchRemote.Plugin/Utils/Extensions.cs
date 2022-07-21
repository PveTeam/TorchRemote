using System.Security.Cryptography;
using System.Text;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Networking;
using Sandbox.Game;
using Sandbox.Game.World;
using TorchRemote.Models.Shared;
namespace TorchRemote.Plugin.Utils;

public static class Extensions
{
    public static void ApplyDynamically(this ServerSettings settings)
    {
        MyGameService.GameServer.SetServerName(settings.ServerName);

        MyMultiplayer.Static.HostName = settings.ServerName;
        MyMultiplayer.Static.WorldName = settings.MapName;
        MySession.Static.Name = settings.MapName;
        MyMultiplayer.Static.MemberLimit = settings.MemberLimit;
        
        MyCachedServerItem.SendSettingsToSteam();
    }
    
    public static Guid ToGuid(this string s)
    {
        using var md5 = MD5.Create();
        return new(md5.ComputeHash(Encoding.UTF8.GetBytes(s)));
    }
}
