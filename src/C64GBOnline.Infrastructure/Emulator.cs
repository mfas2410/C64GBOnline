namespace C64GBOnline.Infrastructure;

public sealed class Emulator : IEmulator, IDisposable
{
    private readonly string _emulatorExe;
    private readonly Encoding _encoding;
    private readonly IFtp _ftp;
    private readonly string _localPath;
    private readonly string _remoteEmulatorPath;
    private readonly CancellationTokenSource _tokenSource = new();
    private string? _path;
    private Process? _process;

    public Emulator(IFtp ftp, Encoding encoding, IOptions<LocalOptions> localOptions, IOptions<EmulatorOptions> emulatorOptions)
    {
        _ftp = ftp;
        _encoding = encoding;
        _localPath = localOptions.Value.Directory;
        _remoteEmulatorPath = emulatorOptions.Value.Zip;
        _emulatorExe = emulatorOptions.Value.Exe;
    }

    public bool CanStart
        => _process is null && !string.IsNullOrEmpty(_path) && File.Exists(_path);

    public void Dispose()
    {
        _tokenSource.Cancel();
        _process?.CloseMainWindow();
    }

    public async Task Initialize()
    {
        Directory.CreateDirectory(_localPath);
        _path = Find(_localPath, _emulatorExe);
        if (CanStart) return;

        try
        {
            await using Stream stream = await _ftp.GetStream(_remoteEmulatorPath);
            await Archive.Extract(stream, _localPath, _encoding, _tokenSource.Token);
        }
        catch (OperationCanceledException) { }

        _path = Find(_localPath, _emulatorExe);
    }

    public async Task Start(string? imagePath)
    {
        if (!CanStart) throw new InvalidOperationException("Emulator not found!");
        if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath)) throw new InvalidOperationException("Image not found!");

        ProcessStartInfo startInfo = new(_path!, imagePath);
        startInfo.RedirectStandardError = true;
        _process = new Process { StartInfo = startInfo };
        if (_process.Start()) await _process.WaitForExitAsync();
        string stdErr = await _process.StandardError.ReadToEndAsync();
        _process.Dispose();
        _process = null;
        if (!string.IsNullOrEmpty(stdErr) && !_tokenSource.IsCancellationRequested) throw new ApplicationException(stdErr);
    }

    private static string? Find(string localPath, string emulatorExe)
        => Directory.GetFiles(localPath, emulatorExe, SearchOption.AllDirectories).SingleOrDefault();
}