using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Retreivr.Jellyfin.Plugin.Api;

/// <summary>
/// Top-level Resolution API response for a recording MBID.
/// </summary>
public sealed class ResolutionRecordResponse
{
    [JsonPropertyName("schema_version")]
    public int SchemaVersion { get; set; }

    [JsonPropertyName("entity_type")]
    public string? EntityType { get; set; }

    [JsonPropertyName("mbid")]
    public string? Mbid { get; set; }

    [JsonPropertyName("availability")]
    public ResolutionAvailability? Availability { get; set; }

    [JsonPropertyName("best_source")]
    public ResolutionSource? BestSource { get; set; }

    [JsonPropertyName("sources")]
    public List<ResolutionSource> Sources { get; set; } = [];
}

/// <summary>
/// Bulk resolution response.
/// </summary>
public sealed class ResolutionBulkResponse
{
    [JsonPropertyName("schema_version")]
    public int SchemaVersion { get; set; }

    [JsonPropertyName("results")]
    public List<ResolutionRecordResponse> Results { get; set; } = [];
}

/// <summary>
/// Availability payload.
/// </summary>
public sealed class ResolutionAvailability
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("instant_available")]
    public bool InstantAvailable { get; set; }

    [JsonPropertyName("network_available")]
    public bool NetworkAvailable { get; set; }
}

/// <summary>
/// Resolved source payload.
/// </summary>
public sealed class ResolutionSource
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("source_id")]
    public string? SourceId { get; set; }

    [JsonPropertyName("duration")]
    public int? Duration { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("bitrate")]
    public int? Bitrate { get; set; }

    [JsonPropertyName("availability")]
    public ResolutionAvailability? Availability { get; set; }

    [JsonPropertyName("verification")]
    public ResolutionVerification? Verification { get; set; }
}

/// <summary>
/// Verification metadata.
/// </summary>
public sealed class ResolutionVerification
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("verification_count")]
    public int VerificationCount { get; set; }

    [JsonPropertyName("verified_by")]
    public List<string> VerifiedBy { get; set; } = [];

    [JsonPropertyName("last_verified_at")]
    public string? LastVerifiedAt { get; set; }
}
