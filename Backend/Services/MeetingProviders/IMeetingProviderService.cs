namespace Backend.Services.MeetingProviders;

public interface IMeetingProviderService
{
    Task<MeetingProviderResult> CreateMeetingAsync(CreateMeetingRequest request, CancellationToken ct = default);
    Task<bool> DeleteMeetingAsync(string meetingId, CancellationToken ct = default);
}
