using Torch;
using Torch.Views;

namespace TorchRemote.Plugin;

public class Config : ViewModel
{
    [Display(Name = "Web Server Config", Description = "Basic configuration for serving web api.")]
    public ListenerConfig Listener { get; set; } = new();
    public string SecurityKey { get; set; } = string.Empty;
}

public class ListenerConfig : ViewModel
{
    [Display(Name = "Url Prefix", Description = "Root url for all requests. If you want access server from remote replace + with your public ip or domain.")]
    public string UrlPrefix { get; set; } = "http://+:80/";
} 
