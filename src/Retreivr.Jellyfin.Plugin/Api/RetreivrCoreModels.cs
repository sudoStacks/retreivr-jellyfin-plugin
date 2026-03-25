using System.Text.Json.Serialization;

namespace Retreivr.Jellyfin.Plugin.Api;

/// <summary>
/// Payload used to enqueue a music recording in Retreivr Core.
/// </summary>
public sealed class RetreivrMusicEnqueueRequest
{
    [JsonPropertyName("recording_mbid")]
    public string RecordingMbid { get; set; } = string.Empty;

    [JsonPropertyName("release_mbid")]
    public string? ReleaseMbid { get; set; }

    [JsonPropertyName("release_group_mbid")]
    public string? ReleaseGroupMbid { get; set; }

    [JsonPropertyName("artist")]
    public string? Artist { get; set; }

    [JsonPropertyName("track")]
    public string? Track { get; set; }

    [JsonPropertyName("album")]
    public string? Album { get; set; }

    [JsonPropertyName("track_number")]
    public int? TrackNumber { get; set; }

    [JsonPropertyName("disc_number")]
    public int? DiscNumber { get; set; }

    [JsonPropertyName("duration_ms")]
    public int? DurationMs { get; set; }

    [JsonPropertyName("media_mode")]
    public string MediaMode { get; set; } = "music";
}

/// <summary>
/// Retreivr enqueue response.
/// </summary>
public sealed class RetreivrMusicEnqueueResponse
{
    [JsonPropertyName("created")]
    public bool Created { get; set; }

    [JsonPropertyName("job_id")]
    public string? JobId { get; set; }

    [JsonPropertyName("dedupe_reason")]
    public string? DedupeReason { get; set; }
}
