using C64GBOnline.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WinSCP;
using EnumerationOptions = WinSCP.EnumerationOptions;

namespace C64GBOnline.Infrastructure
{
    public class Ftp : IFtp
    {
        private readonly string _host;

        public Ftp(string host) => _host = host;

        public Task<List<(string FullName, DateTime LastWriteTime, long Length)>> GetDirectoryListing(string remotePath, string mask) =>
            Task.Run(() =>
            {
                using Session session = new();
                session.Open(new SessionOptions { HostName = _host, Protocol = Protocol.Ftp, UserName = "anonymous" });
                IEnumerable<RemoteFileInfo> remoteFileInfos = session.EnumerateRemoteFiles(remotePath, mask, EnumerationOptions.AllDirectories | EnumerationOptions.EnumerateDirectories).Where(remoteFileInfo => !remoteFileInfo.IsDirectory);
                return remoteFileInfos.Select(remoteFileInfo => (remoteFileInfo.FullName, remoteFileInfo.LastWriteTime, remoteFileInfo.Length)).ToList();
            });

        // Remember to dispose the returned stream!
        public async Task<Stream> GetStream(string path)
        {
            string requestUriString = $"ftp://{_host}{path}";
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(requestUriString);
            request.Credentials = new NetworkCredential("anonymous", string.Empty);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            WebResponse webResponse = await request.GetResponseAsync();
            Stream responseStream = webResponse.GetResponseStream();
            return responseStream!;
        }
    }
}