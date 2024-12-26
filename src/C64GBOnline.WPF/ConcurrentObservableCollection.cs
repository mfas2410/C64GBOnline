namespace C64GBOnline.WPF;

public class ConcurrentObservableCollection<T> : ObservableCollection<T>
{
    private readonly Lock _padLock = new();
    private bool _isNotifying = true;

    [DebuggerStepThrough]
    public ConcurrentObservableCollection() { }

    [DebuggerStepThrough]
    public ConcurrentObservableCollection(params T[] items) => Add(items);

    public bool IsNotifying
    {
        [DebuggerStepThrough]
        get
        {
            lock (_padLock)
            {
                return _isNotifying;
            }
        }
        [DebuggerStepThrough]
        set
        {
            lock (_padLock)
            {
                if (EqualityComparer<bool>.Default.Equals(_isNotifying, value)) return;
                _isNotifying = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsNotifying)));
                if (_isNotifying) InvokeOnDispatcher(OnAllChanged);
            }
        }
    }

    [DebuggerStepThrough]
    public new void Add(T item) => Add([item]);

    [DebuggerStepThrough]
    public void Add(params T[] items)
    {
        if (items.Length == 0) return;
        lock (_padLock)
        {
            int startingIndex = Items.Count;
            foreach (T item in items)
            {
                Items.Add(item);
            }

            if (_isNotifying) InvokeOnDispatcher(() => OnAddOrInsertChanged(items.Length, items[0], startingIndex));
        }
    }

    [DebuggerStepThrough]
    public new void Clear()
    {
        if (Items.Count == 0) return;
        lock (_padLock)
        {
            if (Items.Count == 0) return;
            Items.Clear();
            if (_isNotifying) InvokeOnDispatcher(OnAllChanged);
        }
    }

    [DebuggerStepThrough]
    public new void Insert(int index, T item) => Insert(index, [item]);

    [DebuggerStepThrough]
    public void Insert(int index, params T[] items)
    {
        if (items.Length == 0) return;
        lock (_padLock)
        {
            foreach (T item in items.Reverse())
            {
                Items.Insert(index, item);
            }

            if (_isNotifying) InvokeOnDispatcher(() => OnAddOrInsertChanged(items.Length, items[0], index));
        }
    }

    [DebuggerStepThrough]
    public new void Move(int oldIndex, int newIndex) => Move((oldIndex, newIndex));

    [DebuggerStepThrough]
    public void Move(params (int oldIndex, int newIndex)[] indexes)
    {
        if (indexes.Length == 0) return;
        lock (_padLock)
        {
            T? item = default;
            int oldIndex = 0;
            int newIndex = 0;
            foreach ((int oi, int ni) in indexes)
            {
                oldIndex = oi;
                newIndex = ni;
                item = Items[oldIndex];
                Items.RemoveAt(oldIndex);
                Items.Insert(newIndex, item);
            }

            if (_isNotifying) InvokeOnDispatcher(() => OnMoveChanged(indexes.Length, item, newIndex, oldIndex));
        }
    }

    [DebuggerStepThrough]
    public void Refresh()
    {
        lock (_padLock)
        {
            InvokeOnDispatcher(OnAllChanged);
        }
    }

    [DebuggerStepThrough]
    public new void Remove(T item) => Remove([item]);

    [DebuggerStepThrough]
    public void Remove(params T[] items)
    {
        if (items.Length == 0) return;
        lock (_padLock)
        {
            foreach (T item in items)
            {
                Items.Remove(item);
            }

            if (_isNotifying) InvokeOnDispatcher(() => OnRemoveChanged(items.Length, items[0]));
        }
    }

    [DebuggerStepThrough]
    public new void RemoveAt(int index) => RemoveAt([index]);

    [DebuggerStepThrough]
    public void RemoveAt(params int[] indexes)
    {
        if (indexes.Length == 0) return;
        lock (_padLock)
        {
            T? changedItem = default;
            foreach (int index in indexes.OrderByDescending(x => x))
            {
                changedItem = Items[index];
                Items.RemoveAt(index);
            }

            if (_isNotifying) InvokeOnDispatcher(() => OnRemoveChanged(indexes.Length, changedItem));
        }
    }

    [DebuggerStepThrough]
    public void Replace(T oldItem, T newItem) => Replace((oldItem, newItem));

    [DebuggerStepThrough]
    public void Replace(params (T, T)[] items)
    {
        if (items.Length == 0) return;
        lock (_padLock)
        {
            T? newItem = default;
            T? oldItem = default;
            foreach ((T oi, T ni) in items)
            {
                oldItem = oi;
                newItem = ni;
                Items[Items.IndexOf(oldItem)] = newItem;
            }

            if (_isNotifying) InvokeOnDispatcher(() => OnReplaceChanged(items.Length, newItem, oldItem));
        }
    }

    [DebuggerStepThrough]
    public void ReplaceAt(int index, T item) => ReplaceAt((index, item));

    [DebuggerStepThrough]
    public void ReplaceAt(params (int, T)[] items)
    {
        if (items.Length == 0) return;
        lock (_padLock)
        {
            T? newItem = default;
            T? oldItem = default;
            foreach ((int index, T item) in items)
            {
                oldItem = Items[index];
                newItem = item;
                Items[index] = item;
            }

            if (_isNotifying) InvokeOnDispatcher(() => OnReplaceChanged(items.Length, newItem, oldItem));
        }
    }

    [DebuggerStepThrough]
    private static void InvokeOnDispatcher(Action action)
    {
        Dispatcher? currentDispatcher = Application.Current?.Dispatcher;
        if (currentDispatcher?.CheckAccess() == false)
        {
            currentDispatcher.Invoke(action);
        }
        else
        {
            action.Invoke();
        }
    }

    [DebuggerStepThrough]
    private void OnAllChanged()
    {
        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
        OnCollectionReset();
    }

    [DebuggerStepThrough]
    private void OnAddOrInsertChanged(int changes, T item, int index)
    {
        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
        OnCollectionChanged(changes == 1
            ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index)
            : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    [DebuggerStepThrough]
    private void OnMoveChanged(int changes, T? item, int newIndex, int oldIndex)
    {
        OnIndexerPropertyChanged();
        OnCollectionChanged(changes == 1
            ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex)
            : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    [DebuggerStepThrough]
    private void OnRemoveChanged(int changes, T? item)
    {
        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
        OnCollectionChanged(changes == 1
            ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item)
            : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    [DebuggerStepThrough]
    private void OnReplaceChanged(int changes, T? newItem, T? oldItem)
    {
        OnIndexerPropertyChanged();
        OnCollectionChanged(changes == 1
            ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem)
            : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    [DebuggerStepThrough]
    private void OnCountPropertyChanged() => OnPropertyChanged(new PropertyChangedEventArgs(nameof(Items.Count)));

    [DebuggerStepThrough]
    private void OnIndexerPropertyChanged() => OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

    [DebuggerStepThrough]
    private void OnCollectionReset() => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
}
