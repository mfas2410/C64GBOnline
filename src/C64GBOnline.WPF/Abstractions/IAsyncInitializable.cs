using System.Threading;
using System.Threading.Tasks;

namespace C64GBOnline.WPF.Abstractions
{
    public interface IAsyncInitializable
    {
        ValueTask InitializeAsync(CancellationToken stoppingToken);
    }
}