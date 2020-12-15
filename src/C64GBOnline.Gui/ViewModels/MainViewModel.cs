using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using System.Windows.Threading;
using C64GBOnline.Gui.Infrastructure;
using C64GBOnline.Gui.Messages;
using C64GBOnline.Gui.Models;
using C64GBOnline.WPF;
using C64GBOnline.WPF.Abstractions;

namespace C64GBOnline.Gui.ViewModels
{
    public sealed class MainViewModel : PropertyChangedBase, IInitializable, IAsyncDisposable
    {
        private readonly AppSettings _appSettings;
        private readonly Emulator _emulator;
        private readonly Encoding _encoding;
        private readonly IEventAggregator _eventAggregator;
        private readonly Repository<Game> _gameCache;
        private readonly ConcurrentDictionary<string, Game> _games = new();
        private readonly ConcurrentObservableCollection<Game> _gamesCollection;
        private readonly CollectionViewSource _gamesCollectionViewSource;
        private readonly CancellationTokenSource _getRemoteGamesTokenSource;
        private readonly string _localPath;
        private readonly SidPlayer _sidPlayer;
        private string _filter;
        private CancellationTokenSource? _getGameResourcesTokenSource;
        private Game? _selectedGame;
        private BitmapImage _selectedGameImage;
        private string _selectedGroup;
        private ICommand _startEmulatorCommand;

        public MainViewModel(IEventAggregator eventAggregator, AppSettings appSettings)
        {
            string applicationFullName = Assembly.GetExecutingAssembly().Location;
            _emulator = new Emulator();
            _encoding = Encoding.GetEncoding("ISO-8859-1");
            _gameCache = new Repository<Game>($"{Path.GetFileNameWithoutExtension(applicationFullName)}.db");
            _gamesCollection = new ConcurrentObservableCollection<Game>();
            _gamesCollectionViewSource = new CollectionViewSource { IsLiveFilteringRequested = true, IsLiveGroupingRequested = true, IsLiveSortingRequested = true, Source = _gamesCollection };
            _gamesCollectionViewSource.SortDescriptions.Add(new SortDescription(nameof(Game.Name), ListSortDirection.Ascending));
            _getRemoteGamesTokenSource = new CancellationTokenSource();
            _localPath = Path.GetDirectoryName(applicationFullName) ?? @"C:\C64GBOnline.Gui\";
            _sidPlayer = new SidPlayer();
            _eventAggregator = eventAggregator;
            _appSettings = appSettings;
        }

        public ICommand StartEmulatorCommand =>
            _startEmulatorCommand ??= new RelayCommand<object>(
                async _ =>
                {
                    await _sidPlayer.Stop();
                    await StartEmulator(_appSettings.Host, _selectedGame!.FileName, _localPath, _encoding, _getGameResourcesTokenSource!.Token);
                },
                _ => Task.FromResult(SelectedGame is not null && _emulator.CanStart)
            );

        public string Filter
        {
            get => _filter;
            set
            {
                if (!Set(ref _filter, value)) return;
                if (string.IsNullOrEmpty(_filter))
                    _gamesCollectionViewSource.View.Filter = null;
                else
                    _gamesCollectionViewSource.View.Filter = x => ((Game)x).Name.Contains(_filter, StringComparison.OrdinalIgnoreCase);
            }
        }

        public string SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (!Set(ref _selectedGroup, value)) return;
                _gamesCollectionViewSource.GroupDescriptions.Clear();
                if (!string.IsNullOrEmpty(_selectedGroup)) _gamesCollectionViewSource.GroupDescriptions.Add(new PropertyGroupDescription(_selectedGroup));
            }
        }

        public ICollectionView GamesCollection => _gamesCollectionViewSource.View;

        public Game? SelectedGame
        {
            get => _selectedGame;
            set
            {
                _getGameResourcesTokenSource?.Cancel();
                _getGameResourcesTokenSource?.Dispose();
                _getGameResourcesTokenSource = new CancellationTokenSource();
                Set(ref _selectedGame, value);
                if (_selectedGame is null) return;
                _ = GetGameResources(_selectedGame, _appSettings.Host, _localPath, _gameCache, _encoding, _getGameResourcesTokenSource.Token);
            }
        }

        public BitmapImage SelectedGameImage
        {
            get => _selectedGameImage;
            set => Set(ref _selectedGameImage, value);
        }

        public async ValueTask DisposeAsync()
        {
            await _sidPlayer.DisposeAsync();
            _emulator.Dispose();
            _getRemoteGamesTokenSource.Cancel();
            _getGameResourcesTokenSource?.Cancel();
            _gameCache.Dispose();
        }

        public void Initialize()
        {
            _gameCache.Initialize();
            string localPath = Path.Combine(_localPath, "gamebase_64");
            Directory.CreateDirectory(localPath);
            _ = _emulator.Download(_appSettings.Host, _appSettings.Emulator, localPath, _encoding);
            _ = _sidPlayer.Download(_appSettings.Host, _appSettings.SidPlayer, localPath, _encoding);
            GetLocalGames(_gameCache);
            _ = GetRemoteGamesList(_appSettings.Host, _appSettings.GamesDirectory, _gameCache, _encoding, _getRemoteGamesTokenSource.Token);
        }

        private void GetLocalGames(Repository<Game> gameCache) => _gamesCollection.Add(gameCache.FindAll().Where(game => _games.TryAdd(game.FileName, game)).ToArray());

        private async Task GetRemoteGamesList(string hostName, string remotePath, Repository<Game> gameCache, Encoding encoding, CancellationToken cancellationToken)
        {
            _eventAggregator.Publish(new ProgressBarMessage($"Fetching games from ftp://{hostName}{remotePath}", state: TaskbarItemProgressState.Indeterminate));
            List<(string, DateTime, long)> remoteGames = await Ftp.GetDirectoryListing(hostName, remotePath, "*.zip");

            _gamesCollection.IsNotifying = false;
            foreach ((string fullName, DateTime lastWriteTime, long length) in remoteGames)
            {
                _games.AddOrUpdate(fullName,
                    _ =>
                    {
                        Game game = new(new FtpFileInfo { FullName = fullName, LastWriteTime = lastWriteTime, Length = length });
                        _gamesCollection.Add(game);
                        return game;
                    },
                    (_, oldValue) =>
                    {
                        oldValue.UpdateFileInfo(lastWriteTime, length);
                        return oldValue;
                    });
            }

            _gamesCollection.IsNotifying = true;
            List<Game> gamesToUpdate = _games.Values.Where(game => game.NeedsUpdating).ToList();
            for (var index = 0; index < gamesToUpdate.Count; index++)
            {
                Game game = gamesToUpdate[index];
                if (!game.NeedsUpdating) continue;
                await UpdateGameDetails(game, hostName, encoding, cancellationToken);
                gameCache.Upsert(game);
                _eventAggregator.Publish(new ProgressBarMessage($"Updated {game.Name} ({gamesToUpdate.Count - index + 1} remaining)", (byte)((index + 1) * 100 / gamesToUpdate.Count), TaskbarItemProgressState.Normal));
            }
        }

        private async Task GetGameResources(Game game, string hostName, string localPath, Repository<Game> gameCache, Encoding encoding, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;

            if (game.NeedsUpdating)
            {
                await UpdateGameDetails(game, hostName, encoding, cancellationToken);
                gameCache.Upsert(game);
            }

            if (cancellationToken.IsCancellationRequested || game.NeedsUpdating) return;

            BitmapImage image;
            try
            {
                image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                Uri uri = new($"ftp://{hostName}/gamebase_64/Screenshots/{game.Screenshot.Replace('\\', '/')}");
                image.UriSource = uri;
                image.EndInit();
            }
            catch
            {
                image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(@"pack://application:,,,/Resources/NoPictureAvailable.png", UriKind.RelativeOrAbsolute);
                image.EndInit();
            }

            if (cancellationToken.IsCancellationRequested) return;

            image.Freeze();
            await Application.Current.Dispatcher.InvokeAsync(() => SelectedGameImage = image, DispatcherPriority.Render);

            if (!_emulator.CanStart) return;

            if (string.IsNullOrEmpty(game.SID))
            {
                await _sidPlayer.Stop();
                return;
            }

            string remotePath = $"/gamebase_64/C64Music/{game.SID.Replace('\\', '/')}";
            string localFullName = Path.Combine(localPath, remotePath.TrimStart('/').Replace('/', '\\'));
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(localFullName)!);
                await using Stream inputStream = await Ftp.GetStream(hostName, remotePath);
                await using FileStream outputStream = File.Create(localFullName);
                await inputStream.CopyToAsync(outputStream, cancellationToken);
                _sidPlayer.Start(localFullName);
            }
            catch (OperationCanceledException) { }
        }

        private static async Task UpdateGameDetails(Game game, string hostName, Encoding encoding, CancellationToken cancellationToken)
        {
            string versionInfo = await GetGameInformation(game.FileName, hostName, encoding, cancellationToken);
            game.UpdateGameDetails(GameDetails.Create(versionInfo));
        }

        private static async Task<string> GetGameInformation(string fullName, string hostName, Encoding encoding, CancellationToken cancellationToken)
        {
            await using Stream stream = await Ftp.GetStream(hostName, fullName);
            string versionInfo = await Archive.GetFileAsString(stream, "version.nfo", encoding, cancellationToken);
            return versionInfo;
        }

        private async Task StartEmulator(string hostName, string remotePath, string localPath, Encoding encoding, CancellationToken cancellationToken)
        {
            string localFullName = Path.Combine(localPath, remotePath.TrimStart('/').Replace('/', '\\'));
            localPath = Path.Combine(Path.GetDirectoryName(localFullName) ?? string.Empty, Path.GetFileNameWithoutExtension(localFullName));
            await using Stream stream = await Ftp.GetStream(hostName, remotePath);
            await Archive.Extract(stream, localPath, encoding, cancellationToken);
            string? diskImage = Directory.GetFiles(localPath, "*.d64", SearchOption.AllDirectories).OrderBy(x => x).FirstOrDefault();
            string? tapeImage = Directory.GetFiles(localPath, "*.t64", SearchOption.AllDirectories).OrderBy(x => x).FirstOrDefault();
            await _emulator.Start(diskImage ?? tapeImage);
            Directory.Delete(localPath, true);
        }
    }
}