using Backend.Configuration;
using Microsoft.Extensions.Options;

namespace Backend.Services.MeetingProviders;

// Placeholder — Google Meet integration requires Google.Apis.Calendar.v3 NuGet package
// and a Google Cloud service account. Implement when credentials are available.
public sealed class GoogleMeetService : IMeetingProviderService
{
    private readonly GoogleMeetOptions _opts;
    private readonly ILogger<GoogleMeetService> _logger;

    public GoogleMeetService(IOptions<MeetingProviderOptions> options, ILogger<GoogleMeetService> logger)
    {
        _opts = options.Value.GoogleMeet;
        _logger = logger;
    }

    public Task<MeetingProviderResult> CreateMeetingAsync(CreateMeetingRequest request, CancellationToken ct = default)
    {
        _logger.LogWarning("Google Meet integration chưa được triển khai đầy đủ.");
        return Task.FromResult(MeetingProviderResult.Fail(
            "Google Meet integration chưa sẵn sàng. Vui lòng cấu hình service account."));
    }

    public Task<bool> DeleteMeetingAsync(string meetingId, CancellationToken ct = default)
        => Task.FromResult(false);
}
