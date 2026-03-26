using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Retreivr.Jellyfin.Plugin.Configuration;

namespace Retreivr.Jellyfin.Plugin;

/// <summary>
/// Main Jellyfin plugin entrypoint for Retreivr integration.
/// </summary>
public sealed class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    private static readonly JsonSerializerOptions ConfigJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Jellyfin application paths.</param>
    /// <param name="xmlSerializer">Jellyfin XML serializer.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        Configuration = LoadPersistedConfiguration();
    }

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <summary>
    /// Gets the plugin-owned JSON configuration file path.
    /// </summary>
    public string LocalConfigPath => Path.Combine(DataFolderPath, "retreivr-config.json");

    /// <inheritdoc />
    public override string Name => "Retreivr - Music Search and Acquisition";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("5fe1b9d8-2f4d-48d9-8fc3-f37b4760a96f");

    /// <summary>
    /// Get the effective configuration, preferring the plugin-owned JSON store.
    /// </summary>
    public PluginConfiguration GetEffectiveConfiguration()
    {
        var local = TryLoadLocalConfiguration();
        if (local is not null)
        {
            Configuration = local;
            return Configuration;
        }

        Configuration = MergeConfiguration(Configuration);
        return Configuration;
    }

    /// <summary>
    /// Persist plugin configuration to both Jellyfin XML config and plugin-owned JSON storage.
    /// </summary>
    public PluginConfiguration PersistConfiguration(PluginConfiguration? configuration)
    {
        var merged = MergeConfiguration(configuration);
        Configuration = merged;
        SaveConfiguration(merged);

        Directory.CreateDirectory(DataFolderPath);
        File.WriteAllText(LocalConfigPath, JsonSerializer.Serialize(merged, ConfigJsonOptions));
        return Configuration;
    }

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = Name,
                DisplayName = "Retreivr",
                EnableInMainMenu = true,
                MenuSection = "server",
                MenuIcon = "library_music",
                EmbeddedResourcePath = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.Configuration.configPage.html",
                    GetType().Namespace)
            }
        ];
    }

    private PluginConfiguration LoadPersistedConfiguration()
    {
        var local = TryLoadLocalConfiguration();
        if (local is not null)
        {
            return local;
        }

        return MergeConfiguration(Configuration);
    }

    private PluginConfiguration? TryLoadLocalConfiguration()
    {
        try
        {
            if (!File.Exists(LocalConfigPath))
            {
                return null;
            }

            var json = File.ReadAllText(LocalConfigPath);
            var parsed = JsonSerializer.Deserialize<PluginConfiguration>(json, ConfigJsonOptions);
            return MergeConfiguration(parsed);
        }
        catch
        {
            return null;
        }
    }

    private static PluginConfiguration MergeConfiguration(PluginConfiguration? configuration)
    {
        var defaults = new PluginConfiguration();
        if (configuration is null)
        {
            return defaults;
        }

        defaults.ResolutionApiBaseUrl = string.IsNullOrWhiteSpace(configuration.ResolutionApiBaseUrl)
            ? defaults.ResolutionApiBaseUrl
            : configuration.ResolutionApiBaseUrl;
        defaults.ResolutionApiKey = configuration.ResolutionApiKey ?? string.Empty;
        defaults.RetreivrCoreBaseUrl = string.IsNullOrWhiteSpace(configuration.RetreivrCoreBaseUrl)
            ? defaults.RetreivrCoreBaseUrl
            : configuration.RetreivrCoreBaseUrl;
        defaults.RetreivrCoreApiKey = configuration.RetreivrCoreApiKey ?? string.Empty;
        defaults.EnableAvailabilityBadges = configuration.EnableAvailabilityBadges;
        defaults.EnableInstantResolvedPlayback = configuration.EnableInstantResolvedPlayback;
        defaults.EnableRetreivrDownloadActions = configuration.EnableRetreivrDownloadActions;
        defaults.RequestTimeoutSeconds = configuration.RequestTimeoutSeconds > 0
            ? configuration.RequestTimeoutSeconds
            : defaults.RequestTimeoutSeconds;
        return defaults;
    }
}
