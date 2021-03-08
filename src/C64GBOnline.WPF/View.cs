namespace C64GBOnline.WPF;

public static class View
{
    private static readonly object DefaultModelValue = new();

    public static readonly DependencyProperty ModelProperty = DependencyProperty.RegisterAttached("Model", typeof(object), typeof(View), new PropertyMetadata(DefaultModelValue, (dependencyObject, eventArgs) =>
    {
        object? newValue = eventArgs.NewValue == DefaultModelValue ? null : eventArgs.NewValue;
        if (eventArgs.OldValue == newValue) return;
        if (newValue is null)
        {
            SetContentProperty(dependencyObject, null);
            return;
        }

        FrameworkElement view = InitializeView(newValue.GetType());
        if (view is Window) throw new InvalidOperationException($"{view.GetType().Name} should be {nameof(UserControl)} or equivalent, not {nameof(Window)}");
        InitializeViewModel(newValue, view);
        SetContentProperty(dependencyObject, view);
    }));

    private static CancellationToken _stoppingToken;
    private static IServiceProvider? _serviceProvider;

    [DebuggerStepThrough]
    public static object GetModel(DependencyObject obj) => obj.GetValue(ModelProperty);

    [DebuggerStepThrough]
    public static void SetModel(DependencyObject obj, object value) => obj.SetValue(ModelProperty, value);

    [DebuggerStepThrough]
    public static void Start<T>(IServiceProvider serviceProvider, CancellationToken stoppingToken)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _stoppingToken = stoppingToken;
        FrameworkElement uielement = InitializeView(typeof(T));
        if (uielement is not Window view) throw new InvalidOperationException($"{uielement.GetType().Name} is not a {nameof(Window)}");
        object viewModel = _serviceProvider!.GetRequiredService(typeof(T));
        InitializeViewModel(viewModel, view);
        view.Show();
    }

    private static FrameworkElement InitializeView(Type viewModelType)
    {
        Type viewType = GetViewTypeFromViewModel(viewModelType);
        if (_serviceProvider!.GetRequiredService(viewType) is not FrameworkElement view) throw new InvalidOperationException($"{viewType.Name} is not a {nameof(FrameworkElement)}");
        viewType.GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance)?.Invoke(view, null);
        return view;
    }

    private static void InitializeViewModel(object viewModel, FrameworkElement view)
    {
        if (viewModel is IDisposable disposable) Dispatcher.CurrentDispatcher.ShutdownStarted += (_, __) => disposable.Dispose();
        if (viewModel is IAsyncDisposable asyncDisposable) Dispatcher.CurrentDispatcher.ShutdownStarted += async (_, __) => await CallDisposer(asyncDisposable);
        if (viewModel is IInitializable initializable) view.Loaded += (_, __) => initializable.Initialize();
        if (viewModel is IAsyncInitializable asyncInitializable) view.Loaded += async (_, __) => await CallInitializer(asyncInitializable, _stoppingToken);
        view.DataContext = viewModel;
    }

    private static Type GetViewTypeFromViewModel(Type viewModelType)
    {
        string viewFullName = $"{viewModelType.FullName?.Replace(".ViewModels", ".Views")[..^5]}, {viewModelType.Assembly.GetName().Name}";
        return Type.GetType(viewFullName) ?? throw new InvalidOperationException($"Type with name {viewFullName} not found");
    }

    private static void SetContentProperty(DependencyObject targetLocation, FrameworkElement? view)
    {
        Type type = targetLocation.GetType();
        string propertyName = type.GetCustomAttribute<ContentPropertyAttribute>()?.Name ?? "Content";
        PropertyInfo property = type.GetProperty(propertyName) ?? throw new InvalidOperationException($"Unable to find a Content property on type {type.Name}. Make sure you're using 'View.Model' on a suitable container, e.g. a ContentControl");
        property.SetValue(targetLocation, view);
    }

    private static async Task CallInitializer(IAsyncInitializable asyncInitializable, CancellationToken stoppingToken)
    {
        try
        {
            await asyncInitializable.InitializeAsync(stoppingToken);
        }
        catch (Exception exception)
        {
            ExceptionHandler.ShowException(exception);
        }
    }

    private static async Task CallDisposer(IAsyncDisposable asyncDisposable)
    {
        try
        {
            await asyncDisposable.DisposeAsync();
        }
        catch (Exception exception)
        {
            ExceptionHandler.ShowException(exception);
        }
    }
}