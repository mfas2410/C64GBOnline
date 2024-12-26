namespace C64GBOnline.WPF.Abstractions;

public interface IHandle;

public interface IHandle<in T> : IHandle
{
    void Handle(T message);
}
