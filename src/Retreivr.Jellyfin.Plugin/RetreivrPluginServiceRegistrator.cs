using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Retreivr.Jellyfin.Plugin.Api;
using Retreivr.Jellyfin.Plugin.Services;

namespace Retreivr.Jellyfin.Plugin;

/// <summary>
/// Registers plugin services with Jellyfin.
/// </summary>
public sealed class RetreivrPluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddHttpClient<ResolutionApiClient>();
        serviceCollection.AddHttpClient<RetreivrCoreClient>();
        serviceCollection.AddSingleton(provider => Plugin.Instance?.Configuration ?? new Configuration.PluginConfiguration());
        serviceCollection.AddSingleton<ResolutionAvailabilityService>();
        serviceCollection.AddSingleton<RetreivrDownloadService>();
    }
}
