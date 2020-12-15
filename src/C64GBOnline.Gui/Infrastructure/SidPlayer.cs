using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace C64GBOnline.Gui.Infrastructure
{
    public sealed class SidPlayer : IAsyncDisposable
    {
        private readonly CancellationTokenSource _tokenSource = new();
        private string? _currentSid;
        private string? _path;
        private Process? _process;

        private bool CanStart => !string.IsNullOrEmpty(_path) && File.Exists(_path);

        public async ValueTask DisposeAsync()
        {
            _tokenSource.Cancel();
            await Stop();
        }

        public async Task Download(string hostName, string remotePath, string localPath, Encoding encoding)
        {
            Find(localPath);
            if (CanStart) return;

            try
            {
                await using Stream stream = await Ftp.GetStream(hostName, remotePath);
                await Archive.Extract(stream, localPath, encoding, _tokenSource.Token);
            }
            catch (OperationCanceledException) { }

            Find(localPath);
        }

        public void Start(string sidPath)
        {
            if (!CanStart) throw new InvalidOperationException("SID Player not found!");
            if (string.IsNullOrEmpty(sidPath) || !File.Exists(sidPath)) throw new InvalidOperationException("SID not found!");

            ProcessStartInfo startInfo = new(_path!, sidPath);
            Process process = new();
            process.StartInfo = startInfo;
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

        private void Find(string localPath) => _path = Directory.GetFiles(localPath, "sidplay2w.exe", SearchOption.AllDirectories).SingleOrDefault();

        private static void Delete(string? path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
            try
            {
                File.Delete(path);
            }
            catch (IOException) { }
        }
    }
}