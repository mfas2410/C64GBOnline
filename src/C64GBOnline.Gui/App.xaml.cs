using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using C64GBOnline.Application;
using C64GBOnline.Application.Abstractions;
using C64GBOnline.Gui.Domain;
using C64GBOnline.Gui.ViewModels;
using C64GBOnline.Gui.Views;
using C64GBOnline.Infrastructure;
using C64GBOnline.WPF;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace C64GBOnline.Gui
{
    public sealed partial class App
    {
        private readonly CancellationTokenSource _stoppingTokenSource = new();
        private ServiceProvider? _serviceProvider;

        static App()
        {
            // This code is used to test the app when using other cultures.
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new("da-DK");

            // Ensure the current culture passed into bindings is the OS culture. By default, WPF uses en-US as the culture, regardless of the system settings.
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            // Set animation framerate application wide.
            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 60 });
        }

        protected override void OnStartup(StartupEventArgs eventArgs)
        {
            // Setup global unhandled exception handling
            if (Dispatcher is not null) Dispatcher.UnhandledException += (sender, args) => ExceptionHandler.OnDispatcherUnhandledException(sender, args, _stoppingTokenSource.Token);
            TaskScheduler.UnobservedTaskException += (sender, args) => ExceptionHandler.OnTaskSchedulerUnobservedTaskException(sender, args, _stoppingTokenSource.Token);

            // Read appsettings.json
            AppSettings appSettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().Get<AppSettings>(options => options.BindNonPublicProperties = true);

            // Setup dependency injection
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton(_ => Encoding.GetEncoding("ISO-8859-1"));
            services.AddSingleton<IFtp>(
                _ => new Ftp(
                    appSettings.Host)
                );
            services.AddSingleton<IEmulator>(
                provider => new Emulator(
                    provider.GetRequiredService<IFtp>(),
                    provider.GetRequiredService<Encoding>(),
                    appSettings.LocalDirectory,
                    appSettings.Emulator,
                    appSettings.EmulatorExe)
                );
            services.AddSingleton<IMusicPlayer>(
                provider => new MusicPlayer(
                    provider.GetRequiredService<IFtp>(),
                    provider.GetRequiredService<Encoding>(),
                    appSettings.LocalDirectory,
                    appSettings.MusicPlayer,
                    appSettings.MusicPlayerExe)
                );
            services.AddSingleton<IGameService>(
                provider => new GameService(
                    provider.GetRequiredService<IRepository<Game>>(),
                    provider.GetRequiredService<IFtp>(),
                    provider.GetRequiredService<Encoding>(),
                    appSettings.LocalDirectory,
                    appSettings.RemoteGameDirectory,
                    appSettings.RemoteMusicDirectory,
                    appSettings.RemoteScreenshotsDirectory)
                );
            services.AddSingleton<IRepository<Game>>(
                _ => new JsonRepository<Game>(
                    Path.Combine(appSettings.LocalDirectory, "cache.json"))
                );
            services.AddSingleton<MainView>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<ProgressBarView>();
            services.AddSingleton<ProgressBarViewModel>();
            services.AddSingleton<ShellView>();
            services.AddSingleton<ShellViewModel>();
            _serviceProvider = services.BuildServiceProvider();

            // Start application
            View.Start<ShellViewModel>(_serviceProvider, _stoppingTokenSource.Token);
            base.OnStartup(eventArgs);
        }

        protected override void OnExit(ExitEventArgs eventArgs)
        {
            _stoppingTokenSource.Cancel();
            _serviceProvider?.DisposeAsync().AsTask().GetAwaiter().GetResult();
            base.OnExit(eventArgs);
        }
    }
}