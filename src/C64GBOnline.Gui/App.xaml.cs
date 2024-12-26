namespace C64GBOnline.Gui;

public sealed partial class App
{
    private readonly CancellationTokenSource _stoppingTokenSource = new();
    private ServiceProvider? _serviceProvider;

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
        // Setup global unhandled exception handling
        if (Dispatcher is not null) Dispatcher.UnhandledException += (sender, args) => ExceptionHandler.OnDispatcherUnhandledException(sender, args, _stoppingTokenSource.Token);
        TaskScheduler.UnobservedTaskException += (sender, args) => ExceptionHandler.OnTaskSchedulerUnobservedTaskException(sender, args, _stoppingTokenSource.Token);

        // Setup dependency injection
        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());
        services.AddOptions<EmulatorOptions>().BindConfiguration("Emulator").Validate(EmulatorOptions.Validate).ValidateOnStart();
        services.AddOptions<FtpOptions>().BindConfiguration("Ftp").Validate(FtpOptions.Validate).ValidateOnStart();
        services.AddOptions<LocalOptions>().BindConfiguration("Local").Validate(LocalOptions.Validate).ValidateOnStart();
        services.AddOptions<MusicPlayerOptions>().BindConfiguration("MusicPlayer").Validate(MusicPlayerOptions.Validate).ValidateOnStart();
        services.AddOptions<RemoteOptions>().BindConfiguration("Remote").Validate(RemoteOptions.Validate).ValidateOnStart();

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
