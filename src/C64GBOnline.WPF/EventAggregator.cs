namespace C64GBOnline.WPF;

public sealed class EventAggregator : IEventAggregator
{
    private readonly Lock _padLock = new();
    private readonly Dictionary<Type, List<Handler>> _subscriptions = new();

    [DebuggerStepThrough]
    public void Subscribe(IHandle handler)
    {
        lock (_padLock)
        {
            foreach (Type implementation in handler.GetType().GetInterfaces().Where(handledType => handledType.IsGenericType && typeof(IHandle).IsAssignableFrom(handledType)))
            {
                Type messageType = implementation.GetGenericArguments()[0];
                MethodInfo methodInfo = implementation.GetMethod("Handle") ?? throw new InvalidOperationException("Missing Handle method");
                _subscriptions.TryAdd(messageType, []);
                _subscriptions[messageType].Add(new Handler(handler, methodInfo));
            }
        }
    }

    [DebuggerStepThrough]
    public void Unsubscribe(IHandle handler)
    {
        lock (_padLock)
        {
            foreach (Type implementation in handler.GetType().GetInterfaces().Where(handledType => handledType.IsGenericType && typeof(IHandle).IsAssignableFrom(handledType)))
            {
                Type messageType = implementation.GetGenericArguments()[0];
                foreach (Handler registeredHandler in _subscriptions[messageType].ToList().Where(registeredHandler => registeredHandler.Instance?.Equals(handler) ?? false))
                {
                    List<Handler> subscription = _subscriptions[messageType];
                    subscription.Remove(registeredHandler);
                    if (subscription.Count == 0) _subscriptions.Remove(messageType);
                }
            }
        }
    }

    [DebuggerStepThrough]
    public void Publish(object message)
    {
        lock (_padLock)
        {
            _subscriptions.TryGetValue(message.GetType().UnderlyingSystemType, out List<Handler>? handlers);
            handlers?.ForEach(handler => handler.Invoke(message));
        }
    }

    [DebuggerStepThrough]
    public void Dispose()
    {
        foreach ((Type type, List<Handler> handlers) in _subscriptions.ToList())
        {
            foreach (Handler handler in handlers.ToList())
            {
                handlers.Remove(handler);
                handler.Dispose();
            }

            _subscriptions.Remove(type);
        }
    }

    private sealed class Handler(IHandle handler, MethodInfo methodInfo) : IDisposable
    {
        private MethodInfo? _methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
        public IHandle? Instance { get; private set; } = handler ?? throw new ArgumentNullException(nameof(handler));

        public void Dispose()
        {
            _methodInfo = null;
            Instance = null;
        }

        public void Invoke(object message) => _methodInfo?.Invoke(Instance, [message]);
    }
}
