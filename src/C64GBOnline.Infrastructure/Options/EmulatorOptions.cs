namespace C64GBOnline.Infrastructure.Options;

public sealed class EmulatorOptions
{
    public string Exe { get; init; } = null!;

    public string Zip { get; init; } = null!;

    public static bool Validate(EmulatorOptions instance)
        => !string.IsNullOrWhiteSpace(instance.Exe) &&
           !string.IsNullOrWhiteSpace(instance.Zip);
}
