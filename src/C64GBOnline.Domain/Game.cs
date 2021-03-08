using System.IO;

namespace C64GBOnline.Gui.Domain
{
    public sealed class Game
    {
        private bool _isDirty;

        public Game(FtpFileInfo ftpFileInfo, GameDetails? gameDetails = null)
        {
            FtpFileInfo = ftpFileInfo;
            GameDetails = gameDetails;
        }

        public FtpFileInfo FtpFileInfo { get; private set; }

        public GameDetails? GameDetails { get; private set; }

        public long? Id => GameDetails?.GameInfo.UniqueID;

        public string FileName => FtpFileInfo.FullName;

        public string Genre => GameDetails?.GameInfo.Genre ?? string.Empty;

        public string Name => GameDetails?.GameInfo.Name ?? $"{Path.GetFileNameWithoutExtension(FileName)} (awaits updating)";

        public string Screenshot => GameDetails?.Screenshot ?? string.Empty;

        public string SID => GameDetails?.SID ?? string.Empty;

        public bool NeedsUpdating => _isDirty || GameDetails is null;

        public void UpdateFileInfo(FtpFileInfo ftpFileInfo)
        {
            if (FtpFileInfo.Equals(ftpFileInfo)) return;
            FtpFileInfo = ftpFileInfo;
            _isDirty = true;
        }

        public void UpdateGameDetails(GameDetails gameDetails)
        {
            GameDetails = gameDetails;
            _isDirty = false;
        }
    }
}