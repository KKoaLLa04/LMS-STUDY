namespace Backend.DTOs;

public class UploadResultDto
{
    public string Url { get; set; } = string.Empty;
}

public class PresignVideoRequestDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

public class PresignResultDto
{
    public string UploadUrl { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}

public class PlaybackUrlResultDto
{
    public string Url { get; set; } = string.Empty;
}
