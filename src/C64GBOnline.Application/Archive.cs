namespace C64GBOnline.Application;

public static class Archive
{
    public static async Task<string> GetFileAsString(Stream zipStream, string fileName, Encoding encoding, CancellationToken cancellationToken)
    {
        string content = string.Empty;
        using ZipArchive zipArchive = new(zipStream, ZipArchiveMode.Read, false, encoding);
        foreach (ZipArchiveEntry entry in zipArchive.Entries)
        {
            if (cancellationToken.IsCancellationRequested) break;
            if (!entry.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase)) continue;
            await using Stream stream = entry.Open();
            using StreamReader streamReader = new(stream, encoding);
            content = await streamReader.ReadToEndAsync();
        }

        return content;
    }

    public static async Task Extract(Stream zipStream, string localPath, Encoding encoding, CancellationToken cancellationToken)
    {
        using ZipArchive zipArchive = new(zipStream, ZipArchiveMode.Read, false, encoding);
        foreach (ZipArchiveEntry entry in zipArchive.Entries.Where(x => !string.IsNullOrEmpty(x.Name)))
        {
            if (cancellationToken.IsCancellationRequested) break;
            string localFileName = Path.Combine(localPath, entry.FullName.Replace('/', '\\'));
            string? path = Path.GetDirectoryName(localFileName);
            if (!string.IsNullOrEmpty(path)) Directory.CreateDirectory(path);
            await using Stream inputStream = entry.Open();
            await using FileStream outputStream = File.Create(localFileName);
            await inputStream.CopyToAsync(outputStream, cancellationToken);
        }
    }
}