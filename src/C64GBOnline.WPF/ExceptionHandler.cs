using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace C64GBOnline.WPF
{
    public static class ExceptionHandler
    {
        [DebuggerStepThrough]
        public static void OnDispatcherUnhandledException(object _, DispatcherUnhandledExceptionEventArgs eventArgs)
        {
            eventArgs.Handled = true;
            ShowException(eventArgs.Exception);
        }

        [DebuggerStepThrough]
        public static void OnTaskSchedulerUnobservedTaskException(object? _, UnobservedTaskExceptionEventArgs eventArgs)
        {
            eventArgs.SetObserved();
            ShowException(eventArgs.Exception);
        }

        [DebuggerStepThrough]
        private static void ShowException(Exception? exception)
        {
            Dispatcher? currentDispatcher = Application.Current?.Dispatcher;
            if (currentDispatcher == null) return;
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
}