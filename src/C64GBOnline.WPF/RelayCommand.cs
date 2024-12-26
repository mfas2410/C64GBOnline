namespace C64GBOnline.WPF;

[method: DebuggerStepThrough]
public sealed class RelayCommand<T>(Func<T, Task> execute, Func<T, Task<bool>>? canExecute = null) : ICommand
{
    private readonly Func<T, Task<bool>> _canExecute = canExecute ?? (_ => Task.FromResult(true));
    private readonly Func<T, Task> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private Task? _task;

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    [DebuggerStepThrough]
    public bool CanExecute(object? parameter)
    {
        if (_task?.IsCompleted == false) return false;
        Task<bool> task = Task.Factory.StartNew(async () => await _canExecute((T)parameter!), TaskCreationOptions.AttachedToParent).Unwrap();
        task.Wait();
        return task is { IsCompletedSuccessfully: true, Result: true };
    }

    [DebuggerStepThrough]
    public void Execute(object? parameter) => _task = Task.Factory.StartNew(async () => await _execute((T)parameter!), TaskCreationOptions.AttachedToParent).Unwrap();
}
