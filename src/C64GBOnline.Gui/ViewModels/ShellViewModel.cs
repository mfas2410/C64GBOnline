using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using C64GBOnline.Gui.Messages;
using C64GBOnline.WPF;

namespace C64GBOnline.Gui.ViewModels
{
    public sealed class ShellViewModel : PropertyChangedBase, IDisposable, IHandle<ProgressBarMessage>
    {
        private readonly IEventAggregator _eventAggregator;
        private ICommand? _exitCommand;
        private bool _progressBarIsIndeterminate;
        private TaskbarItemProgressState _progressBarState;
        private string _progressBarText;
        private decimal _progressBarValue;

        public ShellViewModel(IEventAggregator eventAggregator, MainViewModel mainViewModel)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            Main = mainViewModel;
            _progressBarIsIndeterminate = false;
            _progressBarText = string.Empty;
        }

        public ICommand ExitCommand => _exitCommand ??= new RelayCommand<object>(async _ => await Application.Current.Dispatcher.InvokeAsync(() => Application.Current.Shutdown()));

        public MainViewModel Main { get; }

        public string ProgressBarText
        {
            get => _progressBarText;
            set => Set(ref _progressBarText, value);
        }

        public decimal ProgressBarValue
        {
            get => _progressBarValue;
            set => Set(ref _progressBarValue, value);
        }

        public bool ProgressBarIsIndeterminate
        {
            get => _progressBarIsIndeterminate;
            set => Set(ref _progressBarIsIndeterminate, value);
        }

        public TaskbarItemProgressState ProgressBarState
        {
            get => _progressBarState;
            set => Set(ref _progressBarState, value);
        }

        public void Dispose() => _eventAggregator.Unsubscribe(this);

        public void Handle(ProgressBarMessage message)
        {
            ProgressBarText = message.Text ?? ProgressBarText;
            ProgressBarValue = message.PercentComplete / 100m ?? ProgressBarValue;
            ProgressBarIsIndeterminate = message.State == TaskbarItemProgressState.Indeterminate;
            ProgressBarState = message.State ?? ProgressBarState;
        }
    }
}