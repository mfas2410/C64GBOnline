namespace C64GBOnline.Infrastructure;

public sealed class MusicPlayer : IMusicPlayer, IAsyncDisposable
{
    private readonly Encoding _encoding;
    private readonly IFtp _ftp;
    private readonly string _localPath;
    private readonly string _playerExe;
    private readonly string _remotePlayerPath;
    private readonly CancellationTokenSource _tokenSource = new();
    private string? _currentSid;
    private string? _path;
    private Process? _process;

    public MusicPlayer(IFtp ftp, Encoding encoding, IOptions<LocalOptions> localOptions, IOptions<MusicPlayerOptions> musicPlayerOptions)
    {
        _ftp = ftp;
        _encoding = encoding;
        _localPath = localOptions.Value.Directory;
        _remotePlayerPath = musicPlayerOptions.Value.Zip;
        _playerExe = musicPlayerOptions.Value.Exe;
    }

    private bool CanStart
        => !string.IsNullOrEmpty(_path) && File.Exists(_path);

    public async ValueTask DisposeAsync()
    {
        _tokenSource.Cancel();
        await Stop();
    }

    public async Task Initialize()
    {
        Directory.CreateDirectory(_localPath);
        _path = Find(_localPath, _playerExe);
        if (CanStart) return;

        try
        {
            await using Stream stream = await _ftp.GetStream(_remotePlayerPath);
            await Archive.Extract(stream, _localPath, _encoding, _tokenSource.Token);
        }
        catch (OperationCanceledException) { }

        _path = Find(_localPath, _playerExe);
    }

    public void Start(string sidPath)
    {
        if (!CanStart) throw new InvalidOperationException("SID Player not found!");
        if (string.IsNullOrEmpty(sidPath) || !File.Exists(sidPath)) throw new InvalidOperationException("SID not found!");

        ProcessStartInfo startInfo = new(_path!, sidPath);
        Process process = new() { StartInfo = startInfo };
        process.Start();
        if (_process?.HasExited == true || _process is null) _process = process;
        Delete(_currentSid);
        _currentSid = sidPath;
    }

    public async Task Stop()
    {
        if (_process?.HasExited == false)
        {
            _process.CloseMainWindow();
            await _process.WaitForExitAsync();
            _process.Dispose();
            _process = null;
        }

        Delete(_currentSid);
        _currentSid = null;
    }

    private static string? Find(string localPath, string playerExe) => Directory.GetFiles(localPath, playerExe, SearchOption.AllDirectories).SingleOrDefault();

    private static void Delete(string? path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
        try
        {
            File.Delete(path);
            string? dirPath = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dirPath) && !Directory.EnumerateFileSystemEntries(dirPath).Any()) Directory.Delete(dirPath);
        }
        catch (IOException) { }
    }
}
