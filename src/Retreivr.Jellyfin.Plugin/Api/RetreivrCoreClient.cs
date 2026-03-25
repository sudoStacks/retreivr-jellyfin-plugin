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
        var baseUrl = (_configuration.RetreivrCoreBaseUrl ?? string.Empty).Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return default;
        }

        var content = JsonSerializer.Serialize(payload, JsonOptions);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/music/enqueue")
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrWhiteSpace(_configuration.RetreivrCoreApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.RetreivrCoreApiKey);
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, _configuration.RequestTimeoutSeconds)));

        using var response = await _httpClient.SendAsync(request, timeoutCts.Token).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(timeoutCts.Token).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<RetreivrMusicEnqueueResponse>(stream, JsonOptions, timeoutCts.Token).ConfigureAwait(false);
    }
}
