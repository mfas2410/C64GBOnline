namespace C64GBOnline.Domain.Model.Game;

public sealed record GameInfo(
    long? UniqueID,
    string? Name,
    string? Published,
    string? Developer,
    string? Coding,
    string? Graphics,
    string? Music,
    string? Language,
    string? Genre,
    string? Players,
    string? Control,
    string? Comment
)
{
    public static GameInfo Create(Dictionary<string, string> info)
    {
        info.TryGetValue("Unique-ID", out string? uniqueId);
        info.TryGetValue(nameof(Name), out string? name);
        info.TryGetValue(nameof(Published), out string? published);
        info.TryGetValue(nameof(Developer), out string? developer);
        info.TryGetValue(nameof(Coding), out string? coding);
        info.TryGetValue(nameof(Graphics), out string? graphics);
        info.TryGetValue(nameof(Music), out string? music);
        info.TryGetValue(nameof(Language), out string? language);
        info.TryGetValue(nameof(Genre), out string? genre);
        info.TryGetValue(nameof(Players), out string? players);
        info.TryGetValue(nameof(Control), out string? control);
        info.TryGetValue(nameof(Comment), out string? comment);
        GameInfo gameInfo = new(
            string.IsNullOrEmpty(uniqueId) ? null : long.Parse(uniqueId),
            name,
            published,
            developer,
            coding,
            graphics,
            music,
            language,
            genre,
            players,
            control,
            comment
        );
        return gameInfo;
    }
}
