namespace Backend.Services.MeetingProviders;

public sealed record CreateMeetingRequest(
    string Topic,
    string? Agenda,
    DateTime StartTimeUtc,
    int DurationMinutes
);
