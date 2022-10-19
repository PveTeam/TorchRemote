using System.Security.Cryptography;
using System.Text.Json;
using EmbedIO;
using EmbedIO.WebApi;
using NLog;
using Torch.API;
using Torch.Managers;
using Torch.Server.Managers;
using TorchRemote.Plugin.Controllers;
using TorchRemote.Plugin.Modules;
using TorchRemote.Plugin.Utils;

namespace TorchRemote.Plugin.Managers;

public class ApiServerManager : Manager
{
    private static readonly ILogger Log = LogManager.GetCurrentClassLogger(); 
    
    private readonly Config _config;
    private readonly IWebServer _server;
    [Dependency]
    private readonly SettingManager _settingManager = null!;
    [Dependency]
    private readonly InstanceManager _instanceManager = null!;
    public ApiServerManager(ITorchBase torchInstance, Config config) : base(torchInstance)
    {
        _config = config;
        
        if (string.IsNullOrEmpty(_config.SecurityKey))
            _config.SecurityKey = CreateSecurityKey();

        var apiModule = new WebApiModule("/api/v1", async (context, data) =>
        {
            try
            {
                context.Response.ContentType = "application/json";
                using var stream = context.OpenResponseStream();
                await JsonSerializer.SerializeAsync(stream, data, Statics.SerializerOptions);
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw HttpException.InternalServerError(e.Message, e.Message);
            }
        });

        var chatModule = new ChatModule("/api/live/chat", true);
        Statics.ChatModule = chatModule;

        _server = new WebServer(o => o
                                     .WithUrlPrefix(_config.Listener.UrlPrefix)
                                     .WithMode(_config.Listener.ListenerType switch
                                     {
                                         WebListenerType.HttpSys => HttpListenerMode.Microsoft,
                                         WebListenerType.Internal => HttpListenerMode.EmbedIO,
                                         _ => throw new ArgumentOutOfRangeException()
                                     }))
                  .WithLocalSessionManager()
                  .WithCors("/api", "*", "*", "*")
                  .WithModule(new BearerTokenModule("/api", _config.SecurityKey))
                  .WithModule(apiModule
                              .WithController<ServerController>()
                              .WithController<SettingsController>()
                              .WithController<WorldsController>()
                              .WithController<ChatController>()
                              .WithController<PluginsController>()
                              .WithController<PluginDownloadsController>()
                              .WithController<PlayersController>())
                  .WithModule(new LogsModule("/api/live/logs", true))
                  .WithModule(chatModule);
    }

    public override void Attach()
    {
        base.Attach();

        Starter();
        Log.Info("Listening on {0}", _config.Listener.UrlPrefix);

        //_instanceManager.InstanceLoaded += model => _settingManager.RegisterSetting(model.Model, typeof(IMyConfigDedicated), false);
    }
    public override void Detach()
    {
        base.Detach();
        _server.Dispose();
    }

    private async void Starter()
    {
        await _server.RunAsync();
    }
    
    private static string CreateSecurityKey()
    {
        var aes = Aes.Create();
        aes.GenerateIV();
        aes.GenerateKey();

        return Convert.ToBase64String(aes.Key);
    }
}