using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backend.Configuration;
using Microsoft.Extensions.Options;

namespace Backend.Services.MeetingProviders;

public sealed class ZoomMeetingService : IMeetingProviderService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ZoomOptions _opts;
    private readonly ZoomTokenCache _tokenCache;
    private readonly ILogger<ZoomMeetingService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ZoomMeetingService(
        IHttpClientFactory httpClientFactory,
        IOptions<MeetingProviderOptions> options,
        ZoomTokenCache tokenCache,
        ILogger<ZoomMeetingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _opts = options.Value.Zoom;
        _tokenCache = tokenCache;
        _logger = logger;
    }

    public async Task<MeetingProviderResult> CreateMeetingAsync(CreateMeetingRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await GetBearerTokenAsync(ct);
            var client = _httpClientFactory.CreateClient("ZoomApi");

            var body = new
            {
                topic = request.Topic,
                agenda = request.Agenda,
                type = 2, // scheduled meeting
                start_time = request.StartTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                duration = request.DurationMinutes,
                settings = new { waiting_room = false }
            };

            using var req = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_opts.ApiBaseUrl}/users/{_opts.DefaultHostEmail}/meetings");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Content = new StringContent(
                JsonSerializer.Serialize(body, JsonOpts),
                Encoding.UTF8, "application/json");

            using var resp = await client.SendAsync(req, ct);
            var json = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Zoom API trả về {StatusCode}: {Body}", resp.StatusCode, json);
                return MeetingProviderResult.Fail($"Zoom API lỗi ({(int)resp.StatusCode})");
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var meetingId = root.GetProperty("id").GetInt64().ToString();
            var joinUrl = root.GetProperty("join_url").GetString()!;
            var password = root.TryGetProperty("password", out var pwd) ? pwd.GetString() : null;
            var hostUrl = root.TryGetProperty("start_url", out var host) ? host.GetString() : null;

            return MeetingProviderResult.Ok(meetingId, joinUrl, password, hostUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo Zoom meeting cho '{Topic}'", request.Topic);
            return MeetingProviderResult.Fail("Không thể kết nối Zoom API. Vui lòng thử lại.");
        }
    }

    public async Task<bool> DeleteMeetingAsync(string meetingId, CancellationToken ct = default)
    {
        try
        {
            var token = await GetBearerTokenAsync(ct);
            var client = _httpClientFactory.CreateClient("ZoomApi");

            using var req = new HttpRequestMessage(HttpMethod.Delete, $"{_opts.ApiBaseUrl}/meetings/{meetingId}");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var resp = await client.SendAsync(req, ct);
            return resp.IsSuccessStatusCode || (int)resp.StatusCode == 404;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa Zoom meeting {MeetingId}", meetingId);
            return false;
        }
    }

    private async Task<string> GetBearerTokenAsync(CancellationToken ct)
    {
        return await _tokenCache.GetTokenAsync(async () =>
        {
            var client = _httpClientFactory.CreateClient("ZoomToken");

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_opts.ClientId}:{_opts.ClientSecret}"));

            using var req = new HttpRequestMessage(HttpMethod.Post, _opts.TokenUrl);
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            req.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "account_credentials"),
                new KeyValuePair<string, string>("account_id", _opts.AccountId)
            });

            using var resp = await client.SendAsync(req, ct);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var token = root.GetProperty("access_token").GetString()!;
            var expiresIn = root.GetProperty("expires_in").GetInt32();
            return (token, expiresIn);
        }, ct);
    }
}
