using C64GBOnline.Gui.Domain;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace C64GBOnline.Application.Abstractions
{
    public interface IGameService
    {
        Task<string?> DownloadGame(Game game, CancellationToken cancellationToken);
        Task<string?> DownloadMusic(Game game, CancellationToken cancellationToken);
        Task Initialize(CancellationToken stoppingToken);
        Task<MemoryStream> GetScreenshot(Game game);
        IEnumerable<Game> GetLocalGames();
        Task<IEnumerable<FtpFileInfo>> GetRemoteGames();
        Task UpdateGameDetails(Game game, CancellationToken cancellationToken);
    }
}