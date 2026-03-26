namespace Retreivr.Jellyfin.Plugin.Configuration;

/// <summary>
/// Provides the latest plugin configuration instead of a startup snapshot.
/// </summary>
public sealed class PluginConfigurationProvider
{
    /// <summary>
    /// Get the current plugin configuration.
    /// </summary>
    public PluginConfiguration GetCurrent()
        => Plugin.Instance?.GetEffectiveConfiguration() ?? new PluginConfiguration();
}
