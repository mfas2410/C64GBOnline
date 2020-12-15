using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace C64GBOnline.WPF
{
    public sealed class ConcurrentObservableCollection<T> : ObservableCollection<T>
    {
        private readonly object _padLock = new();
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
                    if (!_isNotifying) return;
                    InvokeOnDispatcher(() =>
                    {
                        OnCountPropertyChanged();
                        OnIndexerPropertyChanged();
                        OnCollectionReset();
                    });
                }
            }
        }

        [DebuggerStepThrough]
        public new void Add(T item) => Add(new[] { item });

        [DebuggerStepThrough]
        public void Add(params T[] items)
        {
            lock (_padLock)
            {
                int startingIndex = Items.Count;
                foreach (T item in items)
                {
                    Items.Add(item);
                }

                if (!_isNotifying) return;
                InvokeOnDispatcher(() =>
                {
                    OnCountPropertyChanged();
                    OnIndexerPropertyChanged();
                    OnCollectionChanged(items.Length == 1
                        ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items[0], startingIndex)
                        : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                });
            }
        }

        [DebuggerStepThrough]
        public new void Clear()
        {
            if (Items.Count == 0) return;
            lock (_padLock)
            {
                Items.Clear();
                if (!_isNotifying) return;
                InvokeOnDispatcher(() =>
                {
                    OnCountPropertyChanged();
                    OnIndexerPropertyChanged();
                    OnCollectionReset();
                });
            }
        }

        [DebuggerStepThrough]
        public new void Insert(int index, T item) => Insert(index, new[] { item });

        [DebuggerStepThrough]
        public void Insert(int index, params T[] items)
        {
            lock (_padLock)
            {
                foreach (T item in items.Reverse())
                {
                    Items.Insert(index, item);
                }

                if (!_isNotifying) return;
                InvokeOnDispatcher(() =>
                {
                    OnCountPropertyChanged();
                    OnIndexerPropertyChanged();
                    OnCollectionChanged(items.Length == 1
                        ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items[0], index)
                        : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                });
            }
        }

        [DebuggerStepThrough]
        public new void Move(int oldIndex, int newIndex) => Move((oldIndex, newIndex));

        [DebuggerStepThrough]
        public void Move(params (int oldIndex, int newIndex)[] indexes)
        {
            lock (_padLock)
            {
                T item = default;
                var oldIndex = 0;
                var newIndex = 0;
                foreach ((int oldIndex, int newIndex) tuple in indexes)
                {
                    oldIndex = tuple.oldIndex;
                    newIndex = tuple.newIndex;
                    item = Items[oldIndex];
                    Items.RemoveAt(oldIndex);
                    Items.Insert(newIndex, item);
                }

                if (!_isNotifying) return;
                InvokeOnDispatcher(() =>
                {
                    OnIndexerPropertyChanged();
                    OnCollectionChanged(indexes.Length == 1
                        ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex)
                        : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                });
            }
        }

        [DebuggerStepThrough]
        public void Refresh()
        {
            lock (_padLock)
            {
                InvokeOnDispatcher(() =>
                {
                    OnCountPropertyChanged();
                    OnIndexerPropertyChanged();
                    OnCollectionReset();
                });
            }
        }

        [DebuggerStepThrough]
        public new void Remove(T item) => Remove(new[] { item });

        [DebuggerStepThrough]
        public void Remove(params T[] items)
        {
            lock (_padLock)
            {
                foreach (T item in items)
                {
                    Items.Remove(item);
                }

                if (!_isNotifying) return;
                InvokeOnDispatcher(() =>
                {
                    OnCountPropertyChanged();
                    OnIndexerPropertyChanged();
                    OnCollectionChanged(items.Length == 1
                        ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items[0])
                        : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                });
            }
        }

        [DebuggerStepThrough]
        public new void RemoveAt(int index) => RemoveAt(new[] { index });

        [DebuggerStepThrough]
        public void RemoveAt(params int[] indexes)
        {
            lock (_padLock)
            {
                T changedItem = default;
                foreach (int index in indexes.OrderByDescending(x => x))
                {
                    changedItem = Items[index];
                    Items.RemoveAt(index);
                }

                if (!_isNotifying) return;
                InvokeOnDispatcher(() =>
                {
                    OnCountPropertyChanged();
                    OnIndexerPropertyChanged();
                    OnCollectionChanged(indexes.Length == 1
                        ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItem)
                        : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                });
            }
        }

        [DebuggerStepThrough]
        public void Replace(T oldItem, T newItem) => Replace((oldItem, newItem));

        [DebuggerStepThrough]
        public void Replace(params (T, T)[] items)
        {
            lock (_padLock)
            {
                T newItem = default;
                T oldItem = default;
                foreach ((T oi, T ni) in items)
                {
                    oldItem = oi;
                    newItem = ni;
                    Items[Items.IndexOf(oldItem)] = newItem;
                }

                if (!_isNotifying) return;
                InvokeOnDispatcher(() =>
                {
                    OnIndexerPropertyChanged();
                    OnCollectionChanged(items.Length == 1
                        ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem)
                        : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                });
            }
        }

        [DebuggerStepThrough]
        public void ReplaceAt(int index, T item) => ReplaceAt((index, item));

        [DebuggerStepThrough]
        public void ReplaceAt(params (int, T)[] items)
        {
            lock (_padLock)
            {
                T newItem = default;
                T oldItem = default;
                foreach ((int index, T item) in items)
                {
                    oldItem = Items[index];
                    newItem = item;
                    Items[index] = item;
                }

                if (!_isNotifying) return;
                InvokeOnDispatcher(() =>
                {
                    OnIndexerPropertyChanged();
                    OnCollectionChanged(items.Length == 1
                        ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem)
                        : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                });
            }
        }

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

        private void OnCountPropertyChanged() => OnPropertyChanged(new PropertyChangedEventArgs(nameof(Items.Count)));

        private void OnIndexerPropertyChanged() => OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

        private void OnCollectionReset() => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}