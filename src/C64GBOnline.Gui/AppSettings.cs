namespace C64GBOnline.Gui
{
    public sealed class AppSettings
    {
        public string Host { get; init; } = string.Empty;

        public string Emulator { get; init; } = string.Empty;

        public string EmulatorExe { get; init; } = string.Empty;

        public string MusicPlayer { get; init; } = string.Empty;

        public string MusicPlayerExe { get; init; } = string.Empty;

        public string RemoteGameDirectory { get; init; } = string.Empty;

        public string RemoteMusicDirectory { get; init; } = string.Empty;

        public string RemoteScreenshotsDirectory { get; init; } = string.Empty;

        public string LocalDirectory { get; init; } = string.Empty;
    }
}