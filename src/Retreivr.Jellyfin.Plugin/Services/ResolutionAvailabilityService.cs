using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Retreivr.Jellyfin.Plugin.Api;

namespace Retreivr.Jellyfin.Plugin.Services;

/// <summary>
/// Higher-level availability service for Jellyfin consumers.
/// </summary>
public sealed class ResolutionAvailabilityService
{
    private readonly ResolutionApiClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResolutionAvailabilityService"/> class.
    /// </summary>
    /// <param name="client">Resolution API client.</param>
    public ResolutionAvailabilityService(ResolutionApiClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Resolve one MBID into a normalized availability result.
    /// </summary>
    public async Task<ResolutionAvailabilityResult> GetAvailabilityAsync(string recordingMbid, CancellationToken cancellationToken = default)
    {
        var response = await _client.ResolveRecordingAsync(recordingMbid, cancellationToken).ConfigureAwait(false);
        return ToAvailabilityResult(response);
    }

    /// <summary>
    /// Resolve many MBIDs into normalized availability results.
    /// </summary>
    public async Task<IReadOnlyList<ResolutionAvailabilityResult>> GetBulkAvailabilityAsync(
        IEnumerable<string> recordingMbids,
        CancellationToken cancellationToken = default)
    {
        var mbids = recordingMbids?.Where(static item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
            ?? [];

        if (mbids.Length == 0)
        {
            return [];
        }

        var response = await _client.ResolveBulkAsync(mbids, cancellationToken).ConfigureAwait(false);
        if (response?.Results is null)
        {
            return [];
        }

        return response.Results.Select(ToAvailabilityResult).ToArray();
    }

    private static ResolutionAvailabilityResult ToAvailabilityResult(ResolutionRecordResponse? response)
    {
        var status = NormalizeStatus(response?.Availability?.Status);
        return new ResolutionAvailabilityResult(
            response?.Mbid ?? string.Empty,
            status,
            response?.BestSource?.Url,
            response?.BestSource?.Source,
            response?.BestSource?.SourceId,
            response?.Availability?.InstantAvailable ?? false);
    }

    private static string NormalizeStatus(string? status)
    {
        return status switch
        {
            "verified" => "verified",
            "pending" => "pending",
            "local_only" => "local_only",
            _ => "not_found"
        };
    }
}

/// <summary>
/// Normalized Jellyfin-side availability result.
/// </summary>
/// <param name="RecordingMbid">MusicBrainz recording MBID.</param>
/// <param name="Status">Normalized status.</param>
/// <param name="BestSourceUrl">Best resolved source URL.</param>
/// <param name="BestSource">Resolved source type.</param>
/// <param name="BestSourceId">Resolved source id.</param>
/// <param name="InstantPlayable">Whether the item can be played immediately from a resolved source.</param>
public sealed record ResolutionAvailabilityResult(
    string RecordingMbid,
    string Status,
    string? BestSourceUrl,
    string? BestSource,
    string? BestSourceId,
    bool InstantPlayable);
