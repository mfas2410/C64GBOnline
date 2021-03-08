namespace C64GBOnline.Gui.ViewModels;

public sealed class ProgressBarViewModel : PropertyChangedBase
{
    private bool _isIndeterminate;
    private TaskbarItemProgressState _state = TaskbarItemProgressState.None;
    private string _text = string.Empty;
    private decimal _value;

    public bool IsIndeterminate
    {
        get => _isIndeterminate;
        private set => Set(ref _isIndeterminate, value);
    }

    public TaskbarItemProgressState State
    {
        get => _state;
        set
        {
            Set(ref _state, value);
            IsIndeterminate = State == TaskbarItemProgressState.Indeterminate;
        }
    }

    public string Text
    {
        get => _text;
        set => Set(ref _text, value);
    }

    public decimal Value
    {
        get => _value;
        set
        {
            if (value < 0 || value > 1) throw new ArgumentException("Value must be between 0 and 1, both included", nameof(Value));
            Set(ref _value, value);
        }
    }
}