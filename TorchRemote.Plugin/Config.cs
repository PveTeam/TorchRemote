using Torch;
using Torch.Views;

namespace TorchRemote.Plugin;

public class Config : ViewModel
{
    [Display(Name = "Web Server Config", Description = "Basic configuration for serving web api.")]
    public ListenerConfig Listener { get; set; } = new();
    [Display(Name = "Security Token", Description = "The security token to use when accessing the web api.", ReadOnly = true)]
    public string SecurityKey { get; set; } = string.Empty;
}

public enum WebListenerType
{
    HttpSys,
    Internal
}

public class ListenerConfig : ViewModel
{
    [Display(Name = "Url Prefix", Description = "Root url for all requests. If you want access server from remote replace + with your public ip or domain.")]
    public string UrlPrefix { get; set; } = "http://+:80/";

    [Display(Name = "Listener Type", Description = "Type of listener to serve requests. If you want to run on wine use Internal otherwise default is better choice")]
    public WebListenerType ListenerType { get; set; } = WebListenerType.HttpSys;
} 
