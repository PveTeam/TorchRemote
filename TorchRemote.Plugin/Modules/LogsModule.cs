using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using EmbedIO.WebSockets;
using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;
using Torch.Server;
using TorchRemote.Models.Responses;
using TorchRemote.Plugin.Utils;
namespace TorchRemote.Plugin.Modules;

public class LogsModule : WebSocketModule
{
    
    public LogsModule(string urlPath, bool enableConnectionWatchdog) : base(urlPath, enableConnectionWatchdog)
    {
        ConfigureLogging();
    }
    protected override async Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
    {
    }

    public async void OnLogMessageReceived(DateTime time, LogLevel level, string logger, string message)
    {
        if (ActiveContexts.Count == 0)
            return;
        
        var response = new LogLineResponse(time, (LogLineLevel)level.Ordinal, logger, message);
        var buffer = JsonSerializer.SerializeToUtf8Bytes(response, Statics.SerializerOptions);

        await Task.WhenAll(ActiveContexts
            .Where(b => b.WebSocket.State is WebSocketState.Open)
            .Select(context => context.WebSocket.SendAsync(buffer, true)));
    }

    private void ConfigureLogging()
    {
        var cfg = LogManager.Configuration;
        var flowDocumentTarget = cfg.FindTargetByName("wpf");
        
        if (flowDocumentTarget is null or SplitGroupTarget)
            return;

        flowDocumentTarget.Name = "wpf-old";

        var target = new SplitGroupTarget
        {
            Name = "wpf",
            Targets =
            {
                flowDocumentTarget,
                new StupidTarget(this)
            }
        };
        
        cfg.RemoveTarget("wpf");
        cfg.AddTarget(target);
        foreach (var rule in cfg.LoggingRules)
        {
            if (rule.Targets.Remove(flowDocumentTarget))
                rule.Targets.Add(target);
        }
        LogManager.Configuration = cfg;
        LogManager.GetCurrentClassLogger().Info("Reconfigured logging");
    }

    private class StupidTarget : Target
    {
        private readonly LogsModule _module;
        public StupidTarget(LogsModule module)
        {
            _module = module;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var message = logEvent.FormattedMessage;
            if (logEvent.Exception is not null)
                message += $"\n{logEvent.Exception}";
            
            _module.OnLogMessageReceived(logEvent.TimeStamp, logEvent.Level, logEvent.LoggerName, message);
        }
    }
}
