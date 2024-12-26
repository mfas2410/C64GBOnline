namespace C64GBOnline.Gui.ViewModels;

public sealed class ShellViewModel : PropertyChangedBase
{
    private ICommand? _exitCommand;

    public ShellViewModel(MainViewModel mainViewModel, ProgressBarViewModel progressBarViewModel)
    {
        MainViewModel = mainViewModel;
        ProgressBarViewModel = progressBarViewModel;
    }

    public ICommand ExitCommand => _exitCommand ??= new RelayCommand<object>(async _ => await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => System.Windows.Application.Current.Shutdown()));

    public MainViewModel MainViewModel { get; }

    public ProgressBarViewModel ProgressBarViewModel { get; }
}
