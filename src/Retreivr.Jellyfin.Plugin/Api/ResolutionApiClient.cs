using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Retreivr.Jellyfin.Plugin.Configuration;

namespace Retreivr.Jellyfin.Plugin.Api;

/// <summary>
/// Thin client for the Retreivr Resolution API.
/// </summary>
public sealed class ResolutionApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly PluginConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResolutionApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">Shared HTTP client.</param>
    /// <param name="configuration">Plugin configuration.</param>
    public ResolutionApiClient(HttpClient httpClient, PluginConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Resolve a single recording MBID.
    /// </summary>
    public Task<ResolutionRecordResponse?> ResolveRecordingAsync(string mbid, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(mbid))
        {
            throw new ArgumentException("MBID is required.", nameof(mbid));
        }

        return SendAsync<ResolutionRecordResponse>($"resolve/recording/{Uri.EscapeDataString(mbid.Trim())}", null, cancellationToken);
    }

    /// <summary>
    /// Resolve many recording MBIDs in a single request.
    /// </summary>
    public Task<ResolutionBulkResponse?> ResolveBulkAsync(string[] mbids, CancellationToken cancellationToken = default)
    {
        if (mbids is null || mbids.Length == 0)
        {
            throw new ArgumentException("At least one MBID is required.", nameof(mbids));
        }

        var payload = JsonSerializer.Serialize(new
        {
            mbids = mbids.Where(static item => !string.IsNullOrWhiteSpace(item)).ToArray()
        });

        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        return SendAsync<ResolutionBulkResponse>("resolve/bulk", content, cancellationToken);
    }

    private async Task<T?> SendAsync<T>(string relativePath, HttpContent? content, CancellationToken cancellationToken)
    {
        var baseUrl = (_configuration.ResolutionApiBaseUrl ?? string.Empty).Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return default;
        }

        using var request = new HttpRequestMessage(content is null ? HttpMethod.Get : HttpMethod.Post, $"{baseUrl}/{relativePath}");
        if (!string.IsNullOrWhiteSpace(_configuration.ResolutionApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.ResolutionApiKey);
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
