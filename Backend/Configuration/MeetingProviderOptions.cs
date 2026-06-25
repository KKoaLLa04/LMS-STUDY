namespace Backend.Configuration;

public sealed class MeetingProviderOptions
{
    public ZoomOptions Zoom { get; set; } = new();
    public GoogleMeetOptions GoogleMeet { get; set; } = new();
}

public sealed class ZoomOptions
{
    public bool Enabled { get; set; }
    public string AccountId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string TokenUrl { get; set; } = "https://zoom.us/oauth/token";
    public string ApiBaseUrl { get; set; } = "https://api.zoom.us/v2";
    public string DefaultHostEmail { get; set; } = "me";
}

public sealed class GoogleMeetOptions
{
    public bool Enabled { get; set; }
    public string ServiceAccountKeyPath { get; set; } = string.Empty;
    public string ImpersonatedUserEmail { get; set; } = string.Empty;
}
