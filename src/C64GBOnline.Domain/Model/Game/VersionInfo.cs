namespace C64GBOnline.Domain.Model.Game;

public sealed record VersionInfo(
    string? CrackedCrunched,
    string? GameLength,
    int? Trainers,
    bool? HighScoreSaver,
    bool? LoadingScreen,
    bool? IncludedDocs,
    bool? TrueDriveEmul,
    string? PalNTSC,
    string? Comment
)
{
    public static VersionInfo Create(Dictionary<string, string> info)
    {
        info.TryGetValue("Cracked/Crunched", out string? crackedCrunched);
        info.TryGetValue("Game Length", out string? gameLength);
        info.TryGetValue("Trainers", out string? trainers);
        info.TryGetValue("High Score Saver", out string? highScoreSaver);
        info.TryGetValue("Loading Screen", out string? loadingScreen);
        info.TryGetValue("Included Docs", out string? includedDocs);
        info.TryGetValue("True Drive Emul.", out string? trueDriveEmul);
        info.TryGetValue("Pal/NTSC", out string? palNtsc);
        info.TryGetValue("Comment", out string? comment);
        VersionInfo versionInfo = new(
            crackedCrunched,
            gameLength,
            string.IsNullOrEmpty(trainers) ? null : int.Parse(trainers),
            string.IsNullOrEmpty(highScoreSaver) ? null : highScoreSaver.Equals("Yes", StringComparison.OrdinalIgnoreCase),
            string.IsNullOrEmpty(loadingScreen) ? null : loadingScreen.Equals("Yes", StringComparison.OrdinalIgnoreCase),
            string.IsNullOrEmpty(includedDocs) ? null : includedDocs.Equals("Yes", StringComparison.OrdinalIgnoreCase),
            string.IsNullOrEmpty(trueDriveEmul) ? null : trueDriveEmul.Equals("Yes", StringComparison.OrdinalIgnoreCase),
            palNtsc,
            comment
        );
        return versionInfo;
    }
}
