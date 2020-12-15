using System;
using System.IO;
using C64GBOnline.WPF;

namespace C64GBOnline.Gui.Models
{
    public sealed class Game : PropertyChangedBase
    {
        private bool _isDirty;

        public Game() => FtpFileInfo = null!; // For LiteDB

        public Game(FtpFileInfo ftpFileInfo) => FtpFileInfo = ftpFileInfo;

        public FtpFileInfo FtpFileInfo { get; init; }

        public GameDetails? GameDetails { get; set; }

        public long? Id => GameDetails?.GameInfo.UniqueID;

        public string FileName => FtpFileInfo.FullName;

        public string Genre => GameDetails?.GameInfo.Genre ?? string.Empty;

        public string Name => GameDetails?.GameInfo.Name ?? $"{Path.GetFileNameWithoutExtension(FileName)} (awaits updating)";

        public string Screenshot => GameDetails?.Screenshot ?? string.Empty;

        public string SID => GameDetails?.SID ?? string.Empty;

        public bool NeedsUpdating => _isDirty || GameDetails is null;

        public void UpdateFileInfo(DateTimeOffset lastWriteTime, long length)
        {
            if (!FtpFileInfo.LastWriteTime.Equals(lastWriteTime))
            {
                FtpFileInfo.LastWriteTime = lastWriteTime;
                _isDirty = true;
            }

            if (!FtpFileInfo.Length.Equals(length))
            {
                FtpFileInfo.Length = length;
                _isDirty = true;
            }

            if (_isDirty) Refresh();
        }

        public void UpdateGameDetails(GameDetails gameDetails)
        {
            GameDetails = gameDetails;
            Refresh();
        }
    }
}