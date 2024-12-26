namespace C64GBOnline.Application.Abstractions;

public interface IEmulator
{
    bool CanStart { get; }
    Task Initialize();
    Task Start(string? imagePath);
}
