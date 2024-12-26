namespace C64GBOnline.Application;

public sealed class GameService : IGameService
{
    private readonly Encoding _encoding;
    private readonly IRepository<Game> _localGamesRepository;
    private readonly string _localPath;
    private readonly string _remoteGameDirectory;
    private readonly IFtp _remoteGameRepository;
    private readonly string _remoteMusicDirectory;
    private readonly string _remoteScreenshotsDirectory;

    public GameService(IRepository<Game> localGamesRepository, IFtp remoteGamesRepository, Encoding encoding, IOptions<LocalOptions> localOptions, IOptions<RemoteOptions> remoteOptions)
    {
        _localGamesRepository = localGamesRepository;
        _remoteGameRepository = remoteGamesRepository;
        _encoding = encoding;
        _localPath = localOptions.Value.Directory;
        _remoteGameDirectory = remoteOptions.Value.GamesDirectory;
        _remoteMusicDirectory = remoteOptions.Value.MusicDirectory;
        _remoteScreenshotsDirectory = remoteOptions.Value.ScreenshotsDirectory;
    }

    public async Task<string?> DownloadGame(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            string localFullName = Path.Combine(_localPath, fileName.TrimStart('/').Replace('/', '\\'));
            string localPath = Path.Combine(Path.GetDirectoryName(localFullName) ?? string.Empty, Path.GetFileNameWithoutExtension(localFullName));
            await using Stream stream = await _remoteGameRepository.GetStream(fileName);
            await Archive.Extract(stream, localPath, _encoding, cancellationToken);
            string? diskImage = Directory.GetFiles(localPath, "*.d64", SearchOption.AllDirectories).OrderBy(x => x).FirstOrDefault();
            string? tapeImage = Directory.GetFiles(localPath, "*.t64", SearchOption.AllDirectories).OrderBy(x => x).FirstOrDefault();
            return diskImage ?? tapeImage;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> DownloadMusic(string sid, CancellationToken cancellationToken)
    {
        try
        {
            string remotePath = $"{_remoteMusicDirectory}{sid.Replace('\\', '/')}";
            string localPath = Path.Combine(_localPath, remotePath.TrimStart('/').Replace('/', '\\'));
            if (File.Exists(localPath)) return localPath;

            await using Stream stream = await _remoteGameRepository.GetStream(remotePath);
            string? directory = Path.GetDirectoryName(localPath);
            if (string.IsNullOrEmpty(directory)) return null;

            Directory.CreateDirectory(directory);
            await using FileStream fileStream = new(localPath, FileMode.Create, FileAccess.Write, FileShare.Write, 4096, FileOptions.Asynchronous);
            await stream.CopyToAsync(fileStream, cancellationToken);
            return localPath;
        }
        catch
        {
            return null;
        }
    }

    public async Task Initialize(CancellationToken cancellationToken) => await _localGamesRepository.Initialize(x => x.FileName);

    public async Task<MemoryStream> GetScreenshot(string screenshot)
    {
        await using Stream stream = await _remoteGameRepository.GetStream($"{_remoteScreenshotsDirectory}{screenshot.Replace('\\', '/')}");
        MemoryStream memoryStream = new();
        await stream.CopyToAsync(memoryStream);
        return memoryStream;
    }

    public IEnumerable<Game> GetLocalGames() => _localGamesRepository.Get();

    public async Task<IEnumerable<FtpFileInfo>> GetRemoteGames(CancellationToken cancellationToken)
        => (await _remoteGameRepository.GetDirectoryListing(_remoteGameDirectory, "*.zip", cancellationToken))
            .Select(x => new FtpFileInfo(x.FullName, x.LastWriteTime, x.Length));

    public async Task<Game> UpdateGameDetails(Game game, CancellationToken cancellationToken)
    {
        string versionInfo = await GetGameInformation(game.FileName, cancellationToken);
        game.UpdateGameDetails(GameDetails.Create(versionInfo));
        await _localGamesRepository.Upsert(game);
        return game;
    }

    private async Task<string> GetGameInformation(string path, CancellationToken cancellationToken)
    {
        await using Stream stream = await _remoteGameRepository.GetStream(path);
        string versionInfo = await Archive.GetFileAsString(stream, "version.nfo", _encoding, cancellationToken);
        return versionInfo.RemoveDiacritics();
    }
}
