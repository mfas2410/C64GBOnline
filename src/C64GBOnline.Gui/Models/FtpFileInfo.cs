using System;

namespace C64GBOnline.Gui.Models
{
    public sealed class FtpFileInfo
    {
        public string FullName { get; init; }

        public DateTimeOffset LastWriteTime { get; set; }

        public long Length { get; set; }
    }
}