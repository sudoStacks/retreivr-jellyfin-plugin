using MediaBrowser.Model.Plugins;

namespace Retreivr.Jellyfin.Plugin.Configuration;

/// <summary>
/// Plugin configuration for the Retreivr Jellyfin integration.
/// </summary>
public sealed class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        ResolutionApiBaseUrl = "http://localhost:8000";
        ResolutionApiKey = string.Empty;
        RetreivrCoreBaseUrl = "http://localhost:8000";
        RetreivrCoreApiKey = string.Empty;
        EnableAvailabilityBadges = true;
        EnableInstantResolvedPlayback = false;
        EnableRetreivrDownloadActions = true;
        RequestTimeoutSeconds = 5;
    }

    /// <summary>
    /// Gets or sets the Resolution API base URL.
    /// </summary>
    public string ResolutionApiBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets an optional Resolution API key.
    /// </summary>
    public string ResolutionApiKey { get; set; }

    /// <summary>
    /// Gets or sets the Retreivr Core API base URL.
    /// </summary>
    public string RetreivrCoreBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets an optional Retreivr Core API key.
    /// </summary>
    public string RetreivrCoreApiKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether availability badges should be shown.
    /// </summary>
    public bool EnableAvailabilityBadges { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether immediate playback of resolved sources is enabled.
    /// </summary>
    public bool EnableInstantResolvedPlayback { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether download actions into Retreivr should be enabled.
    /// </summary>
    public bool EnableRetreivrDownloadActions { get; set; }

    /// <summary>
    /// Gets or sets the outbound Resolution API timeout in seconds.
    /// </summary>
    public int RequestTimeoutSeconds { get; set; }
}
