using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace C64GBOnline.WPF
{
    public abstract class PropertyChangedBase : INotifyPropertyChanged
    {
        public bool IsNotifying { get; set; } = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        [DebuggerStepThrough]
        protected void Refresh() => NotifyOfPropertyChange(string.Empty);

        [DebuggerStepThrough]
        protected bool Set<T>(ref T oldValue, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(oldValue, newValue)) return false;
            oldValue = newValue;
            NotifyOfPropertyChange(propertyName);
            return true;
        }

        [DebuggerStepThrough]
        protected void NotifyOfPropertyChange([CallerMemberName] string? propertyName = null)
        {
            if (IsNotifying) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));
        }
    }
}