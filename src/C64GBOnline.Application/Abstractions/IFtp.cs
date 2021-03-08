using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace C64GBOnline.Application.Abstractions
{
    public interface IFtp
    {
        Task<List<(string FullName, DateTime LastWriteTime, long Length)>> GetDirectoryListing(string remotePath, string mask);
        Task<Stream> GetStream(string remotePath);
    }
}