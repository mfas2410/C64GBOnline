namespace C64GBOnline.Gui;

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

        // Setup dependency injection
        IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        IServiceCollection services = new ServiceCollection();

        services.Configure<EmulatorOptions>(configuration.GetSection(nameof(EmulatorOptions)));
        services.Configure<FtpOptions>(configuration.GetSection(nameof(FtpOptions)));
        services.Configure<LocalOptions>(configuration.GetSection(nameof(LocalOptions)));
        services.Configure<MusicPlayerOptions>(configuration.GetSection(nameof(MusicPlayerOptions)));
        services.Configure<RemoteOptions>(configuration.GetSection(nameof(RemoteOptions)));

        services.AddSingleton(_ => Encoding.GetEncoding("ISO-8859-1"));
        services.AddSingleton<IFtp, Ftp>();
        services.AddSingleton<IEmulator, Emulator>();
        services.AddSingleton<IMusicPlayer, MusicPlayer>();
        services.AddSingleton<IGameService, GameService>();
        services.AddSingleton<IRepository<Game>, JsonRepository<Game>>();
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