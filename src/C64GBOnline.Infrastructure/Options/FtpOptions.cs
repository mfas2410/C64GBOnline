namespace C64GBOnline.Infrastructure.Options;

public sealed class FtpOptions
{
    public string Host { get; init; } = null!;

    public static bool Validate(FtpOptions instance)
        => !string.IsNullOrWhiteSpace(instance.Host);
}
