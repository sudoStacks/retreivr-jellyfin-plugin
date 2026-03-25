using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Mvc;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="RetreivrPluginController"/> class.
    /// </summary>
    public RetreivrPluginController(
        ILibraryManager libraryManager,
        ResolutionAvailabilityService availabilityService,
        RetreivrDownloadService downloadService)
    {
        _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
        _availabilityService = availabilityService ?? throw new ArgumentNullException(nameof(availabilityService));
        _downloadService = downloadService ?? throw new ArgumentNullException(nameof(downloadService));
    }

    /// <summary>
    /// Get plugin health/config summary.
    /// </summary>
    [HttpGet("health")]
    public ActionResult<object> GetHealth()
    {
        var config = Plugin.Instance?.Configuration;
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
