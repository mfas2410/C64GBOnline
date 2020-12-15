using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Threading;
using C64GBOnline.WPF.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace C64GBOnline.WPF
{
    public static class View
    {
        private static readonly object DefaultModelValue = new();

        public static readonly DependencyProperty ModelProperty = DependencyProperty.RegisterAttached("Model", typeof(object), typeof(View), new PropertyMetadata(DefaultModelValue, (dependencyObject, eventArgs) =>
        {
            object? newValue = eventArgs.NewValue == DefaultModelValue ? null : eventArgs.NewValue;
            if (eventArgs.OldValue == newValue) return;
            if (newValue is null)
            {
                SetContentProperty(dependencyObject, null!);
                return;
            }

            FrameworkElement view = InitializeView(newValue.GetType());
            if (view is Window) throw new InvalidOperationException($"{view.GetType().Name} should be {nameof(UserControl)} or equivalent, not {nameof(Window)}");
            InitializeViewModel(newValue, view);
            view.DataContext = newValue;
            SetContentProperty(dependencyObject, view);
        }));

        public static IServiceProvider? ServiceProvider { private get; set; }

        [DebuggerStepThrough]
        public static object GetModel(DependencyObject obj) => obj.GetValue(ModelProperty);

        [DebuggerStepThrough]
        public static void SetModel(DependencyObject obj, object value) => obj.SetValue(ModelProperty, value);

        [DebuggerStepThrough]
        public static void Start<T>()
        {
            FrameworkElement uielement = InitializeView(typeof(T));
            if (!(uielement is Window view)) throw new InvalidOperationException($"{uielement.GetType().Name} is not a {nameof(Window)}");
            object viewModel = ServiceProvider!.GetRequiredService(typeof(T));
            InitializeViewModel(viewModel, view);
            view.DataContext = viewModel;
            view.Show();
        }

        private static FrameworkElement InitializeView(Type viewModelType)
        {
            Type viewType = GetViewTypeFromViewModel(viewModelType);
            if (ServiceProvider!.GetRequiredService(viewType) is not FrameworkElement view) throw new InvalidOperationException($"{viewType.Name} is not a {nameof(FrameworkElement)}");
            MethodInfo? initializer = viewType.GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);
            if (initializer != null) initializer.Invoke(view, null);
            return view;
        }

        private static void InitializeViewModel(object viewModel, FrameworkElement view)
        {
            if (viewModel is IDisposable disposable) Dispatcher.CurrentDispatcher.ShutdownStarted += (_, __) => disposable.Dispose();
            if (viewModel is IAsyncDisposable asyncDisposable) Dispatcher.CurrentDispatcher.ShutdownStarted += async (_, __) => await asyncDisposable.DisposeAsync();
            if (viewModel is IInitializable initializable) view.Loaded += (_, __) => initializable.Initialize();
            if (viewModel is IAsyncInitializable asyncInitializable) view.Loaded += async (_, __) => await asyncInitializable.InitializeAsync();
        }

        private static Type GetViewTypeFromViewModel(Type viewModelType)
        {
            string viewModelName = viewModelType.Name;
            string viewName = viewModelName.Substring(0, viewModelName.Length - 5);
            string? assemblyName = viewModelType.Assembly.GetName().Name;
            string viewFullName = $"{assemblyName}.Views.{viewName}, {assemblyName}";
            Type? viewType = Type.GetType(viewFullName);
            if (viewType is null) throw new InvalidOperationException($"Type with name {viewFullName} not found");
            return viewType;
        }

        private static void SetContentProperty(DependencyObject targetLocation, FrameworkElement view)
        {
            Type type = targetLocation.GetType();
            string propertyName = type.GetCustomAttribute<ContentPropertyAttribute>()?.Name ?? "Content";
            PropertyInfo? property = type.GetProperty(propertyName);
            if (property == null) throw new InvalidOperationException($"Unable to find a Content property on type {type.Name}. Make sure you're using 'View.Model' on a suitable container, e.g. a ContentControl");
            property.SetValue(targetLocation, view);
        }
    }
}