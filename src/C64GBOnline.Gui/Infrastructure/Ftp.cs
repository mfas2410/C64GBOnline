using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WinSCP;
using EnumerationOptions = WinSCP.EnumerationOptions;

namespace C64GBOnline.Gui.Infrastructure
{
    public static class Ftp
    {
        public static Task<List<(string fullName, DateTime lastWriteTime, long length)>> GetDirectoryListing(string hostName, string remotePath, string mask) =>
            Task.Run(() =>
            {
                using Session session = new();
                session.Open(new SessionOptions { HostName = hostName, Protocol = Protocol.Ftp, UserName = "anonymous" });
                IEnumerable<RemoteFileInfo> remoteFileInfos = session.EnumerateRemoteFiles(remotePath, mask, EnumerationOptions.AllDirectories | EnumerationOptions.EnumerateDirectories).Where(remoteFileInfo => !remoteFileInfo.IsDirectory);
                return remoteFileInfos.Select(remoteFileInfo => (remoteFileInfo.FullName, remoteFileInfo.LastWriteTime, remoteFileInfo.Length)).ToList();
            });

        // Remember to dispose the returned stream!
        public static async Task<Stream> GetStream(string hostName, string remotePath)
        {
            Exception? exception;
            Stream? responseStream = null;
            do
            {
                WebResponse? webResponse = null;
                try
                {
                    exception = null;
                    webResponse = null;
                    responseStream = null;
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{hostName}{remotePath}");
                    request.Credentials = new NetworkCredential("anonymous", string.Empty);
                    request.Method = WebRequestMethods.Ftp.DownloadFile;
                    webResponse = await request.GetResponseAsync();
                    responseStream = webResponse.GetResponseStream();
                }
                catch (WebException webException)
                {
                    if (responseStream is not null) await responseStream.DisposeAsync();
                    webResponse?.Dispose();
                    exception = webException;
                }
            } while (exception is not null);

            return responseStream!;
        }
    }
}