using System;

namespace C64GBOnline.Gui.Domain
{
    public sealed record FtpFileInfo(
        string FullName,
        DateTimeOffset LastWriteTime,
        long Length
    );
}