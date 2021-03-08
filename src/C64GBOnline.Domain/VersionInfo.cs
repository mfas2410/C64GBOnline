using System;
using System.Collections.Generic;

namespace C64GBOnline.Gui.Domain
{
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
            VersionInfo versionInfo = new VersionInfo(
                crackedCrunched,
                gameLength,
                string.IsNullOrEmpty(trainers) ? (int?)null : int.Parse(trainers),
                string.IsNullOrEmpty(highScoreSaver) ? (bool?)null : highScoreSaver.Equals("Yes", StringComparison.OrdinalIgnoreCase) ? true : false,
                string.IsNullOrEmpty(loadingScreen) ? (bool?)null : loadingScreen.Equals("Yes", StringComparison.OrdinalIgnoreCase) ? true : false,
                string.IsNullOrEmpty(includedDocs) ? (bool?)null : includedDocs.Equals("Yes", StringComparison.OrdinalIgnoreCase) ? true : false,
                string.IsNullOrEmpty(trueDriveEmul) ? (bool?)null : trueDriveEmul.Equals("Yes", StringComparison.OrdinalIgnoreCase) ? true : false,
                palNtsc,
                comment
            );
            return versionInfo;
        }
    }
}