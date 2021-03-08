namespace C64GBOnline.Application.Options;

public sealed class RemoteOptions
{
    public string GamesDirectory { get; init; } = null!;

    public string MusicDirectory { get; init; } = null!;

    public string ScreenshotsDirectory { get; init; } = null!;
}