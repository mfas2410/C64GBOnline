namespace C64GBOnline.Application.Abstractions;

public interface IMusicPlayer
{
    Task Initialize();
    void Start(string path);
    Task Stop();
}
