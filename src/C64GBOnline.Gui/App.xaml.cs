using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using C64GBOnline.Gui.ViewModels;
using C64GBOnline.WPF;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace C64GBOnline.Gui
{
    public sealed partial class App
    {
        private bool _isShuttingDown;

        static App()
        {
            // This code is used to test the app when using other cultures.
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new CultureInfo("da-DK");

            // Ensure the current culture passed into bindings is the OS culture. By default, WPF uses en-US as the culture, regardless of the system settings.
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            // Set animation framerate application wide.
            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 60 });
        }

        protected override void OnStartup(StartupEventArgs eventArgs)
        {
            // Setup dependency injection
            IServiceCollection collection = new ServiceCollection();
            collection.AddSingleton(new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
                .Get<AppSettings>(options => options.BindNonPublicProperties = true)
            );
            collection.AddSingleton<IEventAggregator, EventAggregator>();
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.FullName is not null && type.FullName.StartsWith(GetType().Namespace + ".ViewModels.") && type.FullName.EndsWith("ViewModel")) collection.AddTransient(type);
                if (type.FullName is not null && type.FullName.StartsWith(GetType().Namespace + ".Views.") && type.FullName.EndsWith("View")) collection.AddTransient(type);
            }

            View.ServiceProvider = collection.BuildServiceProvider();

            // Setup global unhandled exception handling
            if (!(Dispatcher is null)) Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedTaskException;

            // Start application
            View.Start<ShellViewModel>();
            base.OnStartup(eventArgs);
        }

        protected override void OnExit(ExitEventArgs eventArgs)
        {
            _isShuttingDown = true;
            if (!(Dispatcher is null)) Dispatcher.UnhandledException -= OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException -= OnTaskSchedulerUnobservedTaskException;
            base.OnExit(eventArgs);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs eventArgs)
        {
            if (_isShuttingDown) return;
            ExceptionHandler.OnDispatcherUnhandledException(sender, eventArgs);
        }

        private void OnTaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs eventArgs)
        {
            if (_isShuttingDown) return;
            ExceptionHandler.OnTaskSchedulerUnobservedTaskException(sender, eventArgs);
        }
    }
}