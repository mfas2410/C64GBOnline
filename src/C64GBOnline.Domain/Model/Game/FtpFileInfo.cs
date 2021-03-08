namespace C64GBOnline.Domain.Model.Game;

public sealed record FtpFileInfo(
    string FullName,
    DateTimeOffset LastWriteTime,
    long Length
);