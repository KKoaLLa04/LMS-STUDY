namespace Backend.Services.MeetingProviders;

// Used in development when real API credentials are not configured.
// Returns deterministic fake data so the full create-classroom flow can be tested end-to-end.
public sealed class FakeMeetingProviderService : IMeetingProviderService
{
    public Task<MeetingProviderResult> CreateMeetingAsync(CreateMeetingRequest request, CancellationToken ct = default)
    {
        var fakeId = $"FAKE-{Math.Abs(request.Topic.GetHashCode()):D9}";
        var result = MeetingProviderResult.Ok(
            meetingId: fakeId,
            joinUrl: $"https://zoom.us/j/{fakeId}",
            password: "fake123",
            hostUrl: $"https://zoom.us/s/{fakeId}"
        );
        return Task.FromResult(result);
    }

    public Task<bool> DeleteMeetingAsync(string meetingId, CancellationToken ct = default)
        => Task.FromResult(true);
}
