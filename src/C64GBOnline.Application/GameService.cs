using C64GBOnline.Application.Abstractions;
using C64GBOnline.Gui.Domain;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace C64GBOnline.Application
{
    public class GameService : IGameService
    {
        private readonly Encoding _encoding;
        private readonly IRepository<Game> _localGamesRepository;
        private readonly string _localPath;
        private readonly IFtp _remoteGameRepository;
        private readonly string _remoteGameDirectory;
        private readonly string _remoteMusicDirectory;
        private readonly string _remoteScreenshotsDirectory;

        public GameService(IRepository<Game> localGamesRepository, IFtp remoteGamesRepository, Encoding encoding, string localPath, string remoteGameDirectory, string remoteMusicDirectory, string remoteScreenshotsDirectory)
        {
            _localGamesRepository = localGamesRepository;
            _remoteGameRepository = remoteGamesRepository;
            _encoding = encoding;
            _localPath = localPath;
            _remoteGameDirectory = remoteGameDirectory;
            _remoteMusicDirectory = remoteMusicDirectory;
            _remoteScreenshotsDirectory = remoteScreenshotsDirectory;
        }

        public async Task<string?> DownloadGame(Game game, CancellationToken cancellationToken)
        {
            try
            {
                string localFullName = Path.Combine(_localPath, game.FileName.TrimStart('/').Replace('/', '\\'));
                string localPath = Path.Combine(Path.GetDirectoryName(localFullName) ?? string.Empty, Path.GetFileNameWithoutExtension(localFullName));
                await using Stream stream = await _remoteGameRepository.GetStream(game.FileName);
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

        public async Task<string?> DownloadMusic(Game game, CancellationToken cancellationToken)
        {
            try
            {
                string remotePath = $"{_remoteMusicDirectory}{game.SID.Replace('\\', '/')}";
                string localPath = Path.Combine(_localPath, remotePath.TrimStart('/').Replace('/', '\\'));
                if (File.Exists(localPath)) return localPath;

                await using Stream stream = await _remoteGameRepository.GetStream(remotePath);
                string? directory = Path.GetDirectoryName(localPath);
                if (string.IsNullOrEmpty(directory)) return null;

                Directory.CreateDirectory(directory);
                await using FileStream fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.Write, 4096, FileOptions.Asynchronous);
                await stream.CopyToAsync(fileStream, cancellationToken);
                return localPath;
            }
            catch
            {
                return null;
            }
        }

        public async Task Initialize(CancellationToken cancellationToken) => await _localGamesRepository.Initialize(x => x.FileName);

        public async Task<MemoryStream> GetScreenshot(Game game)
        {
            await using Stream stream = await _remoteGameRepository.GetStream($"{_remoteScreenshotsDirectory}{game.Screenshot.Replace('\\', '/')}");
            MemoryStream memoryStream = new();
            await stream.CopyToAsync(memoryStream);
            return memoryStream;
        }

        public IEnumerable<Game> GetLocalGames() => _localGamesRepository.Get();

        public async Task<IEnumerable<FtpFileInfo>> GetRemoteGames() => 
            (await _remoteGameRepository.GetDirectoryListing(_remoteGameDirectory, "*.zip"))
            .Select(x => new FtpFileInfo(x.FullName, x.LastWriteTime, x.Length));

        public async Task UpdateGameDetails(Game game, CancellationToken cancellationToken)
        {
            string versionInfo = await GetGameInformation(game.FileName, cancellationToken);
            game.UpdateGameDetails(GameDetails.Create(versionInfo));
            await _localGamesRepository.Upsert(game);
        }

        private async Task<string> GetGameInformation(string path, CancellationToken cancellationToken)
        {
            await using Stream stream = await _remoteGameRepository.GetStream(path);
            string versionInfo = await Archive.GetFileAsString(stream, "version.nfo", _encoding, cancellationToken);
            return versionInfo.RemoveDiacritics();
        }
    }
}