namespace C64GBOnline.Domain.Model.Game;

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

    public string Name => GameDetails is null
        ? $"{Path.GetFileNameWithoutExtension(FileName)} (awaits updating)"
        : $"{GameDetails.GameInfo.Name}{(_isDirty ? " (awaits updating)" : string.Empty)}";

    public string Screenshot => GameDetails?.Screenshot ?? string.Empty;

    public string SID => GameDetails?.SID ?? string.Empty;

    public bool NeedsUpdating => _isDirty || GameDetails is null;

    public bool UpdateFileInfo(FtpFileInfo ftpFileInfo)
    {
        if (FtpFileInfo.Equals(ftpFileInfo)) return false;
        FtpFileInfo = ftpFileInfo;
        _isDirty = true;
        return true;
    }

    public void UpdateGameDetails(GameDetails gameDetails)
    {
        GameDetails = gameDetails;
        _isDirty = false;
    }
}
