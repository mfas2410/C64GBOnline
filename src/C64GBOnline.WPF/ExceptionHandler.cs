namespace C64GBOnline.WPF;

public static class ExceptionHandler
{
    [DebuggerStepThrough]
    public static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs eventArgs, CancellationToken cancellationToken)
    {
        eventArgs.Handled = true;
        if (!cancellationToken.IsCancellationRequested) ShowException(eventArgs.Exception);
    }

    [DebuggerStepThrough]
    public static void OnTaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs eventArgs, CancellationToken cancellationToken)
    {
        eventArgs.SetObserved();
        if (!cancellationToken.IsCancellationRequested) ShowException(eventArgs.Exception);
    }

    [DebuggerStepThrough]
    internal static void ShowException(Exception? exception)
    {
        Dispatcher? currentDispatcher = Application.Current?.Dispatcher;
        if (currentDispatcher is null) return;
        if (!currentDispatcher.CheckAccess())
        {
            currentDispatcher.Invoke(() => ShowException(exception));
            return;
        }

        Window? currentMainWindow = Application.Current?.MainWindow;
        if (currentMainWindow is null) return;
        Exception? baseException = exception?.GetBaseException();
        string errorMessage = $"An unhandled exception occurred!{Environment.NewLine}Further use can lead to unexpected results.";
        if (baseException is not null) errorMessage += $"{Environment.NewLine}{Environment.NewLine}{baseException}";
        MessageBox.Show(currentMainWindow, errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
    }
}
