namespace C64GBOnline.Gui.Models;

public sealed class GameModel : PropertyChangedBase
{
    private Game _game = null!;

    public GameModel(Game game) => Game = game;

    public Game Game
    {
        get => _game;
        set
        {
            _game = value;
            Refresh();
        }
    }

    public long? Id => Game.Id;

    public string FileName => Game.FileName;

    public string Genre => Game.Genre;

    public string Name => Game.Name;

    public string Screenshot => Game.Screenshot;

    public string SID => Game.SID;

    public bool NeedsUpdating => Game.NeedsUpdating;

    internal void UpdateFileInfo(FtpFileInfo ftpFileInfo)
    {
        if (_game.UpdateFileInfo(ftpFileInfo)) Refresh();
    }
}
