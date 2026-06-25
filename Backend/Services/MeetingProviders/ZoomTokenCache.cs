namespace Backend.Services.MeetingProviders;

// Singleton — holds the current Zoom Bearer token across requests.
// Refreshes automatically when the token is within 60 seconds of expiry.
public sealed class ZoomTokenCache
{
    private string? _token;
    private DateTime _expiresAt = DateTime.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<string> GetTokenAsync(
        Func<Task<(string token, int expiresInSeconds)>> fetchFunc,
        CancellationToken ct = default)
    {
        if (_token is not null && DateTime.UtcNow < _expiresAt.AddSeconds(-60))
            return _token;

        await _lock.WaitAsync(ct);
        try
        {
            if (_token is not null && DateTime.UtcNow < _expiresAt.AddSeconds(-60))
                return _token;

            var (token, expiresIn) = await fetchFunc();
            _token = token;
            _expiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
            return _token;
        }
        finally
        {
            _lock.Release();
        }
    }
}
