using System;
using System.Collections.Generic;

namespace C64GBOnline.Gui.Models
{
    public sealed class GameDetails
    {
        public GameDetails() { } // For LiteDb

        private GameDetails(Dictionary<string, string> info, string notes, GameInfo gameInfo, VersionInfo versionInfo)
        {
            GBVersion = int.Parse(info["GB-Version"]);
            Filename = info[nameof(Filename)];
            Screenshot = info[nameof(Screenshot)];
            SID = info[nameof(SID)];
            Notes = notes;
            GameInfo = gameInfo;
            VersionInfo = versionInfo;
        }

        public int GBVersion { get; set; }

        public string Filename { get; set; }

        public string Screenshot { get; set; }

        public string SID { get; set; }

        public string Notes { get; set; }

        public GameInfo GameInfo { get; set; }

        public VersionInfo VersionInfo { get; set; }

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

            return new GameDetails(generalInfo, notes, new GameInfo(gameInfo), new VersionInfo(versionInfo));
        }
    }
}