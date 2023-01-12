namespace C64GBOnline.Application.Options;

public sealed class LocalOptions
{
    public string Directory { get; init; } = null!;

    public static bool Validate(LocalOptions instance)
        => !string.IsNullOrWhiteSpace(instance.Directory);
}
