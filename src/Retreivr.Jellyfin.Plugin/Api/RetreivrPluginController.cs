using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Retreivr.Jellyfin.Plugin.Configuration;
using Retreivr.Jellyfin.Plugin.Services;

namespace Retreivr.Jellyfin.Plugin.Api;

/// <summary>
/// Plugin-facing API surface for availability and download actions.
/// </summary>
[ApiController]
[Route("Plugins/Retreivr")]
public sealed class RetreivrPluginController : ControllerBase
{
    private readonly ILibraryManager _libraryManager;
    private readonly ResolutionAvailabilityService _availabilityService;
    private readonly RetreivrDownloadService _downloadService;
    private readonly RetreivrCoreClient _retreivrCoreClient;
    private readonly ILogger<RetreivrPluginController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetreivrPluginController"/> class.
    /// </summary>
    public RetreivrPluginController(
        ILibraryManager libraryManager,
        ResolutionAvailabilityService availabilityService,
        RetreivrDownloadService downloadService,
        RetreivrCoreClient retreivrCoreClient,
        ILogger<RetreivrPluginController> logger)
    {
        _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
        _availabilityService = availabilityService ?? throw new ArgumentNullException(nameof(availabilityService));
        _downloadService = downloadService ?? throw new ArgumentNullException(nameof(downloadService));
        _retreivrCoreClient = retreivrCoreClient ?? throw new ArgumentNullException(nameof(retreivrCoreClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get plugin health/config summary.
    /// </summary>
    [HttpGet("health")]
    public ActionResult<object> GetHealth()
    {
        var config = Plugin.Instance?.GetEffectiveConfiguration();
        return Ok(new
        {
            plugin = "Retreivr",
            resolutionApiBaseUrl = config?.ResolutionApiBaseUrl,
            retreivrCoreBaseUrl = config?.RetreivrCoreBaseUrl,
            enableAvailabilityBadges = config?.EnableAvailabilityBadges ?? false,
            enableInstantResolvedPlayback = config?.EnableInstantResolvedPlayback ?? false,
            enableRetreivrDownloadActions = config?.EnableRetreivrDownloadActions ?? false
        });
    }

    /// <summary>
    /// Get the current persisted plugin configuration.
    /// </summary>
    [HttpGet("config")]
    public ActionResult<PluginConfiguration> GetConfig()
    {
        return Ok(Plugin.Instance?.GetEffectiveConfiguration() ?? new PluginConfiguration());
    }

    /// <summary>
    /// Persist plugin configuration directly through the plugin runtime.
    /// </summary>
    [HttpPost("config")]
    public ActionResult<PluginConfiguration> SaveConfig([FromBody] PluginConfiguration configuration)
    {
        if (Plugin.Instance is null)
        {
            return StatusCode(503);
        }

        configuration ??= new PluginConfiguration();
        var persisted = Plugin.Instance.PersistConfiguration(configuration);
        _logger.LogInformation(
            "Retreivr plugin config saved resolutionApiBaseUrl={ResolutionApiBaseUrl} retreivrCoreBaseUrl={RetreivrCoreBaseUrl} availabilityBadges={EnableAvailabilityBadges} instantPlayback={EnableInstantResolvedPlayback} downloadActions={EnableRetreivrDownloadActions}",
            persisted.ResolutionApiBaseUrl,
            persisted.RetreivrCoreBaseUrl,
            persisted.EnableAvailabilityBadges,
            persisted.EnableInstantResolvedPlayback,
            persisted.EnableRetreivrDownloadActions);
        return Ok(persisted);
    }

    /// <summary>
    /// Get plugin runtime and backend connectivity status.
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<object>> GetStatusAsync(CancellationToken cancellationToken)
    {
        var config = Plugin.Instance?.GetEffectiveConfiguration() ?? new PluginConfiguration();
        var resolutionHealthy = false;
        var coreHealthy = false;
        string? resolutionError = null;
        string? coreError = null;

        try
        {
            var resolution = await _availabilityService.GetHealthAsync(cancellationToken).ConfigureAwait(false);
            resolutionHealthy = resolution is not null;
        }
        catch (Exception ex)
        {
            resolutionError = ex.Message;
        }

        try
        {
            var health = await _retreivrCoreClient.GetHealthAsync(cancellationToken).ConfigureAwait(false);
            coreHealthy = health is not null;
        }
        catch (Exception ex)
        {
            coreError = ex.Message;
        }

        return Ok(new
        {
            resolutionApiBaseUrl = config.ResolutionApiBaseUrl,
            retreivrCoreBaseUrl = config.RetreivrCoreBaseUrl,
            resolutionApiHealthy = resolutionHealthy,
            retreivrCoreHealthy = coreHealthy,
            resolutionApiError = resolutionError,
            retreivrCoreError = coreError,
            retreivrUiUrl = _retreivrCoreClient.GetConfiguredBaseUrl()
        });
    }

    /// <summary>
    /// Resolve one recording MBID against the Resolution API.
    /// </summary>
    [HttpGet("resolve/recording/{mbid}")]
    public async Task<ActionResult<ResolutionAvailabilityResult>> ResolveRecordingAsync(
        [FromRoute, Required] string mbid,
        CancellationToken cancellationToken)
    {
        var result = await _availabilityService.GetAvailabilityAsync(mbid, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Resolve many recording MBIDs in bulk.
    /// </summary>
    [HttpPost("resolve/bulk")]
    public async Task<ActionResult<IReadOnlyList<ResolutionAvailabilityResult>>> ResolveBulkAsync(
        [FromBody] string[] mbids,
        CancellationToken cancellationToken)
    {
        var results = await _availabilityService.GetBulkAvailabilityAsync(mbids, cancellationToken).ConfigureAwait(false);
        return Ok(results);
    }

    /// <summary>
    /// Proxy music search to Retreivr Core.
    /// </summary>
    [HttpGet("music/search")]
    public async Task<ActionResult<object>> SearchMusicAsync(
        [FromQuery] string artist = "",
        [FromQuery] string album = "",
        [FromQuery] string track = "",
        [FromQuery] string mode = "auto",
        CancellationToken cancellationToken = default)
    {
        var payload = await _retreivrCoreClient.SearchMusicAsync(artist, album, track, mode, cancellationToken).ConfigureAwait(false);
        return payload is null ? StatusCode(503) : Ok(payload.RootElement.Clone());
    }

    /// <summary>
    /// Proxy album search to Retreivr Core.
    /// </summary>
    [HttpGet("music/albums/search")]
    public async Task<ActionResult<object>> SearchAlbumsAsync(
        [FromQuery] string q = "",
        [FromQuery(Name = "artist_mbid")] string artistMbid = "",
        CancellationToken cancellationToken = default)
    {
        var payload = await _retreivrCoreClient.SearchAlbumsAsync(q, artistMbid, cancellationToken).ConfigureAwait(false);
        return payload is null ? StatusCode(503) : Ok(payload.RootElement.Clone());
    }

    /// <summary>
    /// Proxy album tracks lookup to Retreivr Core.
    /// </summary>
    [HttpGet("music/albums/{releaseGroupMbid}/tracks")]
    public async Task<ActionResult<object>> GetAlbumTracksAsync(
        [FromRoute] string releaseGroupMbid,
        CancellationToken cancellationToken = default)
    {
        var payload = await _retreivrCoreClient.GetAlbumTracksAsync(releaseGroupMbid, cancellationToken).ConfigureAwait(false);
        return payload is null ? StatusCode(503) : Ok(payload.RootElement.Clone());
    }

    /// <summary>
    /// Proxy full-album download to Retreivr Core.
    /// </summary>
    [HttpPost("music/albums/{releaseGroupMbid}/download")]
    public async Task<ActionResult<object>> DownloadAlbumAsync(
        [FromRoute] string releaseGroupMbid,
        CancellationToken cancellationToken = default)
    {
        var payload = await _retreivrCoreClient.DownloadAlbumAsync(releaseGroupMbid, cancellationToken).ConfigureAwait(false);
        return payload is null ? StatusCode(503) : Ok(payload.RootElement.Clone());
    }

    /// <summary>
    /// Proxy track enqueue to Retreivr Core.
    /// </summary>
    [HttpPost("music/enqueue")]
    public async Task<ActionResult<object>> EnqueueMusicAsync(
        [FromBody] object payload,
        CancellationToken cancellationToken = default)
    {
        var response = await _retreivrCoreClient.EnqueueMusicPayloadAsync(payload, cancellationToken).ConfigureAwait(false);
        return response is null ? StatusCode(503) : Ok(response.RootElement.Clone());
    }

    /// <summary>
    /// Resolve a Jellyfin item by id using its MusicBrainz provider ids.
    /// </summary>
    [HttpGet("items/{itemId}/availability")]
    public async Task<ActionResult<object>> ResolveItemAvailabilityAsync(
        [FromRoute, Required] Guid itemId,
        CancellationToken cancellationToken)
    {
        var item = _libraryManager.GetItemById(itemId);
        if (item is null)
        {
            return NotFound();
        }

        var ids = MusicBrainzProviderIdResolver.Resolve(item);
        if (string.IsNullOrWhiteSpace(ids.RecordingMbid))
        {
            return Ok(new
            {
                itemId,
                itemName = item.Name,
                status = "not_found",
                reason = "missing_musicbrainz_recording_id"
            });
        }

        var result = await _availabilityService.GetAvailabilityAsync(ids.RecordingMbid!, cancellationToken).ConfigureAwait(false);
        return Ok(new
        {
            itemId,
            itemName = item.Name,
            recordingMbid = ids.RecordingMbid,
            releaseMbid = ids.ReleaseMbid,
            releaseGroupMbid = ids.ReleaseGroupMbid,
            availability = result
        });
    }

    /// <summary>
    /// Resolve many Jellyfin items by id using their MusicBrainz provider ids.
    /// </summary>
    [HttpPost("items/availability/bulk")]
    public async Task<ActionResult<IReadOnlyList<object>>> ResolveItemAvailabilityBulkAsync(
        [FromBody] Guid[] itemIds,
        CancellationToken cancellationToken)
    {
        var results = new List<object>();
        foreach (var itemId in itemIds)
        {
            var item = _libraryManager.GetItemById(itemId);
            if (item is null)
            {
                results.Add(new
                {
                    itemId,
                    status = "not_found",
                    reason = "item_not_found"
                });
                continue;
            }

            var ids = MusicBrainzProviderIdResolver.Resolve(item);
            if (string.IsNullOrWhiteSpace(ids.RecordingMbid))
            {
                results.Add(new
                {
                    itemId,
                    itemName = item.Name,
                    status = "not_found",
                    reason = "missing_musicbrainz_recording_id"
                });
                continue;
            }

            var availability = await _availabilityService.GetAvailabilityAsync(ids.RecordingMbid!, cancellationToken).ConfigureAwait(false);
            results.Add(new
            {
                itemId,
                itemName = item.Name,
                recordingMbid = ids.RecordingMbid,
                releaseMbid = ids.ReleaseMbid,
                releaseGroupMbid = ids.ReleaseGroupMbid,
                availability
            });
        }

        return Ok(results);
    }

    /// <summary>
    /// Enqueue a Jellyfin item in Retreivr Core.
    /// </summary>
    [HttpPost("items/{itemId}/download")]
    public async Task<ActionResult<RetreivrMusicEnqueueResponse>> EnqueueItemDownloadAsync(
        [FromRoute, Required] Guid itemId,
        CancellationToken cancellationToken)
    {
        var item = _libraryManager.GetItemById(itemId);
        if (item is null)
        {
            return NotFound();
        }

        var result = await _downloadService.EnqueueItemAsync(item, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return StatusCode(503);
        }

        return Ok(result);
    }
}
