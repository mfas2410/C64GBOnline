using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace C64GBOnline.WPF
{
    public sealed class RelayCommand<T> : ICommand
    {
        private readonly Func<T, Task<bool>> _canExecute;
        private readonly Func<T, Task> _execute;
        private Task? _task;

        [DebuggerStepThrough]
        public RelayCommand(Func<T, Task> execute, Func<T, Task<bool>>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? (_ => Task.FromResult(true));
        }

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
            Task.WaitAll(task);
            return task.IsCompletedSuccessfully && task.Result;
        }

        [DebuggerStepThrough]
        public void Execute(object? parameter) => _task = Task.Factory.StartNew(async () => await _execute((T)parameter!), TaskCreationOptions.AttachedToParent).Unwrap();
    }
}