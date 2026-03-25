using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using Retreivr.Jellyfin.Plugin.Api;

namespace Retreivr.Jellyfin.Plugin.Services;

/// <summary>
/// Dispatches Jellyfin items to Retreivr Core for acquisition.
/// </summary>
public sealed class RetreivrDownloadService
{
    private readonly RetreivrCoreClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetreivrDownloadService"/> class.
    /// </summary>
    public RetreivrDownloadService(RetreivrCoreClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Enqueue a Jellyfin item in Retreivr using MusicBrainz-backed metadata.
    /// </summary>
    public Task<RetreivrMusicEnqueueResponse?> EnqueueItemAsync(BaseItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        var ids = MusicBrainzProviderIdResolver.Resolve(item);
        if (string.IsNullOrWhiteSpace(ids.RecordingMbid))
        {
            throw new InvalidOperationException("Item does not carry a MusicBrainz recording id.");
        }

        return _client.EnqueueMusicTrackAsync(
            new RetreivrMusicEnqueueRequest
            {
                RecordingMbid = ids.RecordingMbid!,
                ReleaseMbid = ids.ReleaseMbid,
                ReleaseGroupMbid = ids.ReleaseGroupMbid,
                Artist = ReadStringProperty(item, "AlbumArtist"),
                Track = item.Name,
                Album = ReadStringProperty(item, "Album"),
                TrackNumber = item.IndexNumber,
                DiscNumber = item.ParentIndexNumber,
                DurationMs = item.RunTimeTicks.HasValue ? (int?)(item.RunTimeTicks.Value / TimeSpan.TicksPerMillisecond) : null,
                MediaMode = "music"
            },
            cancellationToken);
    }

    private static string? ReadStringProperty(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        if (property is null || property.PropertyType != typeof(string))
        {
            return null;
        }

        return property.GetValue(target) as string;
    }
}
