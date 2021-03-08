using System;
using System.Collections.Generic;

namespace C64GBOnline.Gui.Domain
{
    public sealed record GameDetails(
        int? GBVersion,
        string? Filename,
        string? Screenshot,
        string? SID,
        string Notes,
        GameInfo GameInfo,
        VersionInfo VersionInfo
    )
    {
        public static GameDetails Create(string info)
        {
            string section = string.Empty;
            string notes = string.Empty;
            Dictionary<string, string> generalInfo = new();
            Dictionary<string, string> gameInfo = new();
            Dictionary<string, string> versionInfo = new();

            string[] lines = info.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (line.StartsWith('=') || line.StartsWith('-')) continue;

                string[] tags = line.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (tags.Length < 2)
                {
                    section = tags[0];
                    continue;
                }

                switch (section)
                {
                    case "":
                    {
                        generalInfo.Add(tags[0], tags[1].TrimStart());
                        break;
                    }
                    case "GAME INFO":
                    {
                        gameInfo.Add(tags[0], tags[^1].TrimStart());
                        break;
                    }
                    case "VERSION INFO":
                    {
                        versionInfo.Add(tags[0], tags[1].TrimStart());
                        break;
                    }
                    case "NOTES":
                    {
                        notes += " " + line;
                        break;
                    }
                }
            }

            generalInfo.TryGetValue("GB-Version", out string? gbVersionString);
            bool gbVersionParsed = int.TryParse(gbVersionString, out int gbVersion);
            generalInfo.TryGetValue(nameof(Filename), out string? filename);
            generalInfo.TryGetValue(nameof(Screenshot), out string? screenshot);
            generalInfo.TryGetValue(nameof(SID), out string? sid);
            return new GameDetails(
                gbVersionParsed ? gbVersion : (int?)null,
                filename,
                screenshot,
                sid,
                notes,
                GameInfo.Create(gameInfo),
                VersionInfo.Create(versionInfo)
            );
        }
    }
}