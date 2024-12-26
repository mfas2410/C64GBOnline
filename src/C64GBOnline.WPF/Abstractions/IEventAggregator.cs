namespace C64GBOnline.WPF.Abstractions;

public interface IEventAggregator : IDisposable
{
    void Subscribe(IHandle handler);

    void Unsubscribe(IHandle handler);

    void Publish(object message);
}
