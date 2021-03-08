namespace C64GBOnline.Application.Abstractions;

public interface IFtp
{
    Task<List<(string FullName, DateTime LastWriteTime, long Length)>> GetDirectoryListing(string remotePath, string mask, CancellationToken cancellationToken);
    Task<Stream> GetStream(string remotePath);
}