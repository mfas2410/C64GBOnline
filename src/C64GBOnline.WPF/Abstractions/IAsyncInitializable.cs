namespace C64GBOnline.WPF.Abstractions;

public interface IAsyncInitializable
{
    ValueTask InitializeAsync(CancellationToken stoppingToken);
}
