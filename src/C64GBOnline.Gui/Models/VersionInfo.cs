using System;
using System.Collections.Generic;

namespace C64GBOnline.Gui.Models
{
    public sealed class VersionInfo
    {
        public VersionInfo() { } // For LiteDb

        public VersionInfo(Dictionary<string, string> info)
        {
            CrackedCrunched = info["Cracked/Crunched"];
            GameLength = info["Game Length"];
            Trainers = int.Parse(info[nameof(Trainers)]);
            HighScoreSaver = info["High Score Saver"].Equals("Yes", StringComparison.OrdinalIgnoreCase);
            LoadingScreen = info["Loading Screen"].Equals("Yes", StringComparison.OrdinalIgnoreCase);
            IncludedDocs = info["Included Docs"].Equals("Yes", StringComparison.OrdinalIgnoreCase);
            TrueDriveEmul = info["True Drive Emul."].Equals("Yes", StringComparison.OrdinalIgnoreCase);
            PalNTSC = info["Pal/NTSC"];
            Comment = info["Comment"];
        }

        public string CrackedCrunched { get; set; }

        public string GameLength { get; set; }

        public int Trainers { get; set; }

        public bool HighScoreSaver { get; set; }

        public bool LoadingScreen { get; set; }

        public bool IncludedDocs { get; set; }

        public bool TrueDriveEmul { get; set; }

        public string PalNTSC { get; set; }

        public string Comment { get; set; }
    }
}