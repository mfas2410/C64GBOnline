using System.Threading.Tasks;

namespace C64GBOnline.WPF.Abstractions
{
    public interface IAsyncInitializable
    {
        Task InitializeAsync();
    }
}