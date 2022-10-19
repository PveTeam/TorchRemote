using System.Net;
using EmbedIO;

namespace TorchRemote.Plugin.Modules;

internal class BearerTokenModule : WebModuleBase
{
    private readonly string _token;

    public BearerTokenModule(string baseRoute, string token) : base(baseRoute)
    {
        _token = token;
    }

    protected override Task OnRequestAsync(IHttpContext context)
    {
        const string bearer = "Bearer ";
        if (context.Request.Headers["Authorization"] is { } headerValue && 
            headerValue.StartsWith(bearer, StringComparison.OrdinalIgnoreCase) && 
            headerValue.Substring(bearer.Length) == _token)
            return Task.CompletedTask;
        
        throw HttpException.Unauthorized();
    }

    public override bool IsFinalHandler => false;
}