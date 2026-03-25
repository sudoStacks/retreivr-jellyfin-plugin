using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Entities;

namespace Retreivr.Jellyfin.Plugin.Services;

/// <summary>
/// Extracts MusicBrainz provider ids from Jellyfin items.
/// </summary>
public static class MusicBrainzProviderIdResolver
{
    private static readonly string[] RecordingKeys =
    [
        "MusicBrainzTrack",
        "MusicBrainzRecording",
        "MusicBrainzRecordingId"
    ];

    private static readonly string[] ReleaseKeys =
    [
        "MusicBrainzAlbum",
        "MusicBrainzRelease",
        "MusicBrainzReleaseId"
    ];

    private static readonly string[] ReleaseGroupKeys =
    [
        "MusicBrainzReleaseGroup",
        "MusicBrainzReleaseGroupId"
    ];

    /// <summary>
    /// Resolve MusicBrainz ids from a Jellyfin item.
    /// </summary>
    public static MusicBrainzProviderIds Resolve(BaseItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return new MusicBrainzProviderIds(
            FirstProviderId(item, RecordingKeys),
            FirstProviderId(item, ReleaseKeys),
            FirstProviderId(item, ReleaseGroupKeys));
    }

    private static string? FirstProviderId(BaseItem item, IEnumerable<string> keys)
    {
        var providerIds = item.ProviderIds;
        if (providerIds is null)
        {
            return null;
        }

        foreach (var key in keys)
        {
            if (!providerIds.TryGetValue(key, out var value))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }
}

/// <summary>
/// Known MusicBrainz ids associated with a Jellyfin item.
/// </summary>
/// <param name="RecordingMbid">MusicBrainz recording id.</param>
/// <param name="ReleaseMbid">MusicBrainz release id.</param>
/// <param name="ReleaseGroupMbid">MusicBrainz release-group id.</param>
public sealed record MusicBrainzProviderIds(
    string? RecordingMbid,
    string? ReleaseMbid,
    string? ReleaseGroupMbid);
