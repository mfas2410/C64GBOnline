namespace C64GBOnline.Application.Abstractions;

public interface IGameService
{
    Task<string?> DownloadGame(string fileName, CancellationToken cancellationToken);
    Task<string?> DownloadMusic(string sid, CancellationToken cancellationToken);
    Task Initialize(CancellationToken stoppingToken);
    Task<MemoryStream> GetScreenshot(string screenshot);
    IEnumerable<Game> GetLocalGames();
    Task<IEnumerable<FtpFileInfo>> GetRemoteGames(CancellationToken cancellationToken);
    Task<Game> UpdateGameDetails(Game game, CancellationToken cancellationToken);
}