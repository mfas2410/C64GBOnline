namespace C64GBOnline.Infrastructure;

public sealed class Ftp : IFtp
{
    private readonly string _host;

    public Ftp(IOptions<FtpOptions> options)
        => _host = options.Value.Host;

    public Task<List<(string FullName, DateTime LastWriteTime, long Length)>> GetDirectoryListing(string path, string mask, CancellationToken cancellationToken) =>
        Task.Run(() =>
        {
            using Session session = new();
            session.Open(new SessionOptions { HostName = _host, Protocol = Protocol.Ftp, UserName = "anonymous" });
            IEnumerable<RemoteFileInfo> remoteFileInfos = session.EnumerateRemoteFiles(path, mask, WinSCP.EnumerationOptions.AllDirectories | WinSCP.EnumerationOptions.EnumerateDirectories).Where(remoteFileInfo => !remoteFileInfo.IsDirectory);
            return remoteFileInfos.Select(remoteFileInfo => (remoteFileInfo.FullName, remoteFileInfo.LastWriteTime, remoteFileInfo.Length)).ToList();
        }, cancellationToken);

    // Remember to dispose the returned stream!
    public async Task<Stream> GetStream(string fullName)
    {
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{_host}{fullName}");
        request.Credentials = new NetworkCredential("anonymous", string.Empty);
        request.Method = WebRequestMethods.Ftp.DownloadFile;
        WebResponse webResponse = await request.GetResponseAsync();
        return webResponse.GetResponseStream();
    }
}