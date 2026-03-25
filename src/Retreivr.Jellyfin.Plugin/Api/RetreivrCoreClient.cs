using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Retreivr.Jellyfin.Plugin.Configuration;

namespace Retreivr.Jellyfin.Plugin.Api;

/// <summary>
/// Client for selected Retreivr Core actions.
/// </summary>
public sealed class RetreivrCoreClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly PluginConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetreivrCoreClient"/> class.
    /// </summary>
    public RetreivrCoreClient(HttpClient httpClient, PluginConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Enqueue a music track download in Retreivr Core.
    /// </summary>
    public async Task<RetreivrMusicEnqueueResponse?> EnqueueMusicTrackAsync(
        RetreivrMusicEnqueueRequest payload,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return await PostJsonAsync<RetreivrMusicEnqueueResponse>("api/music/enqueue", payload, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Check Retreivr Core health.
    /// </summary>
    public Task<JsonDocument?> GetHealthAsync(CancellationToken cancellationToken = default)
        => GetJsonAsync<JsonDocument>("health", cancellationToken);

    /// <summary>
    /// Proxy MusicBrainz-backed music search.
    /// </summary>
    public Task<JsonDocument?> SearchMusicAsync(string artist, string album, string track, string mode, CancellationToken cancellationToken = default)
    {
        var query = $"api/music/search?artist={Uri.EscapeDataString(artist ?? string.Empty)}&album={Uri.EscapeDataString(album ?? string.Empty)}&track={Uri.EscapeDataString(track ?? string.Empty)}&mode={Uri.EscapeDataString(mode ?? "auto")}&offset=0&limit=100";
        return GetJsonAsync<JsonDocument>(query, cancellationToken);
    }

    /// <summary>
    /// Proxy album search.
    /// </summary>
    public Task<JsonDocument?> SearchAlbumsAsync(string query, string artistMbid, CancellationToken cancellationToken = default)
    {
        var url = $"api/music/albums/search?q={Uri.EscapeDataString(query ?? string.Empty)}&limit=50";
        if (!string.IsNullOrWhiteSpace(artistMbid))
        {
            url += $"&artist_mbid={Uri.EscapeDataString(artistMbid)}";
        }

        return GetJsonAsync<JsonDocument>(url, cancellationToken);
    }

    /// <summary>
    /// Fetch track listing for a release group.
    /// </summary>
    public Task<JsonDocument?> GetAlbumTracksAsync(string releaseGroupMbid, CancellationToken cancellationToken = default)
        => GetJsonAsync<JsonDocument>($"api/music/albums/{Uri.EscapeDataString(releaseGroupMbid)}/tracks?limit=200", cancellationToken);

    /// <summary>
    /// Queue a full album in Retreivr Core.
    /// </summary>
    public Task<JsonDocument?> DownloadAlbumAsync(string releaseGroupMbid, CancellationToken cancellationToken = default)
        => PostJsonAsync<JsonDocument>("api/music/album/download", new { release_group_mbid = releaseGroupMbid, media_mode = "music" }, cancellationToken);

    /// <summary>
    /// Enqueue an explicit music payload in Retreivr Core.
    /// </summary>
    public Task<JsonDocument?> EnqueueMusicPayloadAsync(object payload, CancellationToken cancellationToken = default)
        => PostJsonAsync<JsonDocument>("api/music/enqueue", payload, cancellationToken);

    /// <summary>
    /// Resolve the configured base URL for opening the Retreivr UI directly.
    /// </summary>
    public string? GetConfiguredBaseUrl()
    {
        var baseUrl = (_configuration.RetreivrCoreBaseUrl ?? string.Empty).Trim().TrimEnd('/');
        return string.IsNullOrWhiteSpace(baseUrl) ? null : baseUrl;
    }

    private Task<T?> GetJsonAsync<T>(string relativePath, CancellationToken cancellationToken = default)
        => SendAsync<T>(HttpMethod.Get, relativePath, null, cancellationToken);

    private Task<T?> PostJsonAsync<T>(string relativePath, object payload, CancellationToken cancellationToken = default)
    {
        var content = JsonSerializer.Serialize(payload, JsonOptions);
        return SendAsync<T>(HttpMethod.Post, relativePath, new StringContent(content, Encoding.UTF8, "application/json"), cancellationToken);
    }

    private async Task<T?> SendAsync<T>(HttpMethod method, string relativePath, HttpContent? content, CancellationToken cancellationToken)
    {
        var baseUrl = (_configuration.RetreivrCoreBaseUrl ?? string.Empty).Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return default;
        }

        using var request = new HttpRequestMessage(method, $"{baseUrl}/{relativePath}");
        if (!string.IsNullOrWhiteSpace(_configuration.RetreivrCoreApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.RetreivrCoreApiKey);
        }

        if (content is not null)
        {
            request.Content = content;
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, _configuration.RequestTimeoutSeconds)));

        using var response = await _httpClient.SendAsync(request, timeoutCts.Token).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(timeoutCts.Token).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, timeoutCts.Token).ConfigureAwait(false);
    }
}
