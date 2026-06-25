using Backend.Models;

namespace Backend.Services.MeetingProviders;

public sealed class MeetingProviderFactory
{
    private readonly IServiceProvider _sp;

    public MeetingProviderFactory(IServiceProvider sp) => _sp = sp;

    public IMeetingProviderService? Resolve(MeetingPlatform platform) =>
        platform switch
        {
            MeetingPlatform.Zoom or MeetingPlatform.GoogleMeet =>
                _sp.GetKeyedService<IMeetingProviderService>(platform.ToString()),
            _ => null
        };
}
