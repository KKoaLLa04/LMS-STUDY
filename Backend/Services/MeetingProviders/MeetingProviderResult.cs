namespace Backend.Services.MeetingProviders;

public sealed record MeetingProviderResult(
    bool Success,
    string? MeetingId,
    string? JoinUrl,
    string? Password,
    string? HostUrl,
    string? ErrorMessage
)
{
    public static MeetingProviderResult Fail(string error) =>
        new(false, null, null, null, null, error);

    public static MeetingProviderResult Ok(string meetingId, string joinUrl, string? password, string? hostUrl = null) =>
        new(true, meetingId, joinUrl, password, hostUrl, null);
}
