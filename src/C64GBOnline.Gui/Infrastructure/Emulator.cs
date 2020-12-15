using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using C64GBOnline.WPF;

namespace C64GBOnline.Gui.Infrastructure
{
    public sealed class Emulator : PropertyChangedBase, IDisposable
    {
        private readonly CancellationTokenSource _tokenSource = new();
        private string? _path;
        private Process? _process;

        public bool CanStart => _process is null && !string.IsNullOrEmpty(_path) && File.Exists(_path);

        public void Dispose()
        {
            _tokenSource.Cancel();
            _process?.CloseMainWindow();
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

        public async Task Start(string? imagePath)
        {
            if (!CanStart) throw new InvalidOperationException("Emulator not found!");
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath)) throw new InvalidOperationException("Image not found!");

            ProcessStartInfo startInfo = new(_path!, imagePath);
            startInfo.RedirectStandardError = true;
            _process = new Process();
            _process.StartInfo = startInfo;
            Refresh();
            if (_process.Start()) await _process.WaitForExitAsync();
            string stdErr = await _process.StandardError.ReadToEndAsync();
            _process.Dispose();
            _process = null;
            Refresh();
            if (!string.IsNullOrEmpty(stdErr) && !_tokenSource.IsCancellationRequested) throw new ApplicationException(stdErr);
        }

        private void Find(string localPath)
        {
            _path = Directory.GetFiles(localPath, "ccs64.exe", SearchOption.AllDirectories).SingleOrDefault();
            Refresh();
        }
    }
}