namespace C64GBOnline.Gui.ViewModels;

public sealed class MainViewModel : PropertyChangedBase, IAsyncInitializable, IDisposable
{
    private readonly IEmulator _emulator;
    private readonly ConcurrentDictionary<string, GameModel> _games = new();
    private readonly ConcurrentObservableCollection<GameModel> _gamesCollection = new();
    private readonly CollectionViewSource _gamesCollectionViewSource;
    private readonly IGameService _gameService;
    private readonly IMusicPlayer _musicPlayer;
    private readonly ProgressBarViewModel _progressBarViewModel;
    private string _filter = string.Empty;
    private CancellationTokenSource? _getGameResourcesTokenSource;
    private GameModel? _selectedGame;
    private BitmapImage? _selectedGameImage;
    private string? _selectedGroup = null;
    private ICommand? _startEmulatorCommand;

    public MainViewModel(IGameService gameService, IEmulator emulator, IMusicPlayer musicPlayer, ProgressBarViewModel progressBarViewModel)
    {
        _gameService = gameService;
        _emulator = emulator;
        _musicPlayer = musicPlayer;
        _progressBarViewModel = progressBarViewModel;

        _gamesCollectionViewSource = new CollectionViewSource { IsLiveFilteringRequested = true, IsLiveGroupingRequested = true, IsLiveSortingRequested = true, Source = _gamesCollection };
        _gamesCollectionViewSource.SortDescriptions.Add(new SortDescription(nameof(GameModel.Name), ListSortDirection.Ascending));
    }

    public ICommand StartEmulatorCommand =>
        _startEmulatorCommand ??= new RelayCommand<GameModel>(
            async game =>
            {
                string? localGamePath = await _gameService.DownloadGame(game.FileName, _getGameResourcesTokenSource!.Token);
                if (!string.IsNullOrEmpty(localGamePath))
                {
                    await _musicPlayer.Stop();
                    await _emulator.Start(localGamePath);
                }

                // TODO: Cleanup local files
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
            {
                _gamesCollectionViewSource.View.Filter = null;
            }
            else
            {
                _gamesCollectionViewSource.View.Filter = game => ((GameModel)game).Name.Contains(_filter, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    public string? SelectedGroup
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

    public GameModel? SelectedGame
    {
        get => _selectedGame;
        set
        {
            _getGameResourcesTokenSource?.Cancel();
            _getGameResourcesTokenSource?.Dispose();
            _getGameResourcesTokenSource = null;
            if (!Set(ref _selectedGame, value) || _selectedGame is null) return;
            _getGameResourcesTokenSource = new CancellationTokenSource();
            _ = GetGameResources(_selectedGame, _getGameResourcesTokenSource.Token);
        }
    }

    public BitmapImage? SelectedGameImage
    {
        get => _selectedGameImage;
        set => Set(ref _selectedGameImage, value);
    }

    public async ValueTask InitializeAsync(CancellationToken stoppingToken)
    {
        ReportProgress(TaskbarItemProgressState.Normal, "Initializing music player...", 0);
        await _musicPlayer.Initialize();
        ReportProgress(TaskbarItemProgressState.Normal, "Initializing emulator...", 0.33m);
        await _emulator.Initialize();
        ReportProgress(TaskbarItemProgressState.Normal, "Fetching cached games...", 0.66m);
        await _gameService.Initialize(stoppingToken);
        foreach (Game item in _gameService.GetLocalGames())
        {
            _games.TryAdd(item.FileName, new GameModel(item));
        }

        _gamesCollection.Add(_games.Values.ToArray());
        ReportProgress(TaskbarItemProgressState.Normal, "Initial initialization done", 1);
        _ = GetRemoteGamesList(stoppingToken);
    }

    public void Dispose() => _getGameResourcesTokenSource?.Cancel();

    private async Task GetRemoteGamesList(CancellationToken stoppingToken)
    {
        ReportProgress(TaskbarItemProgressState.Indeterminate, "Fetching files from remote...", 0);

        IEnumerable<FtpFileInfo> remoteGames = await _gameService.GetRemoteGames(stoppingToken);
        _gamesCollection.IsNotifying = false;
        foreach (FtpFileInfo item in remoteGames)
        {
            _games.AddOrUpdate(item.FullName,
                _ =>
                {
                    GameModel gameModel = new(new Game(item));
                    _gamesCollection.Add(gameModel);
                    return gameModel;
                },
                (_, oldValue) =>
                {
                    oldValue.UpdateFileInfo(item);
                    return oldValue;
                });
        }

        _gamesCollection.IsNotifying = true;

        List<GameModel> gamesToUpdate = _games.Values.Where(game => game.NeedsUpdating).OrderBy(game => game.Name).ToList();
        int alreadyUpToDate = _games.Count - gamesToUpdate.Count;
        for (int index = 0; index < gamesToUpdate.Count; index++)
        {
            GameModel game = gamesToUpdate[index];
            if (!game.NeedsUpdating) continue;
            game.Game = await _gameService.UpdateGameDetails(game.Game, stoppingToken);
            ReportProgress(TaskbarItemProgressState.Normal, $"Updated {game.Name} ({gamesToUpdate.Count - index + 1} remaining)", (alreadyUpToDate + index + 1m) / _games.Count);
        }
    }

    private void ReportProgress(TaskbarItemProgressState state, string text, decimal value)
    {
        _progressBarViewModel.State = state;
        _progressBarViewModel.Text = text;
        _progressBarViewModel.Value = value;
    }

    private async Task GetGameResources(GameModel game, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;

        if (game.NeedsUpdating)
        {
            game.Game = await _gameService.UpdateGameDetails(game.Game, cancellationToken);
            _gamesCollection.Refresh();
        }

        if (cancellationToken.IsCancellationRequested || game.NeedsUpdating) return;

        BitmapImage image;
        try
        {
            image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = await _gameService.GetScreenshot(game.Screenshot);
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
        SelectedGameImage = image;

        if (!_emulator.CanStart) return;

        if (string.IsNullOrEmpty(game.SID))
        {
            await _musicPlayer.Stop();
            return;
        }

        string? sidPath = await _gameService.DownloadMusic(game.SID, cancellationToken);
        if (!string.IsNullOrEmpty(sidPath))
        {
            _musicPlayer.Start(sidPath);
        }
        else
        {
            await _musicPlayer.Stop();
        }
    }
}