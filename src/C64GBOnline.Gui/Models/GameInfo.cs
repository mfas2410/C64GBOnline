using System.Collections.Generic;

namespace C64GBOnline.Gui.Models
{
    public sealed class GameInfo
    {
        public GameInfo() { } // For LiteDb

        public GameInfo(Dictionary<string, string> info)
        {
            UniqueID = long.Parse(info["Unique-ID"]);
            Name = info[nameof(Name)];
            Published = info[nameof(Published)];
            Developer = info[nameof(Developer)];
            Coding = info[nameof(Coding)];
            Graphics = info[nameof(Graphics)];
            Music = info[nameof(Music)];
            Language = info[nameof(Language)];
            Genre = info[nameof(Genre)];
            Players = info[nameof(Players)];
            Control = info[nameof(Control)];
            Comment = info[nameof(Comment)];
        }

        public long UniqueID { get; set; }

        public string Name { get; set; }

        public string Published { get; set; }

        public string Developer { get; set; }

        public string Coding { get; set; }

        public string Graphics { get; set; }

        public string Music { get; set; }

        public string Language { get; set; }

        public string Genre { get; set; }

        public string Players { get; set; }

        public string Control { get; set; }

        public string Comment { get; set; }
    }
}