using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PDFEditorApp.Core.Services;
using PDFEditorApp.ViewModels;
using PDFEditorApp.Views;
using System.Windows;

// Disambiguate from System.Windows.Forms.Application
using Application = System.Windows.Application;

namespace PDFEditorApp;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(b => b.AddDebug().SetMinimumLevel(LogLevel.Information));

        // Services
        services.AddSingleton<IConversionService, ConversionService>();
        services.AddSingleton<ICompressService, CompressService>();
        services.AddSingleton<IMergeService, MergeService>();
        services.AddSingleton<ISplitService, SplitService>();
        services.AddSingleton<IReorganizeService, ReorganizeService>();

        // ViewModels
        services.AddSingleton<ConvertViewModel>();
        services.AddSingleton<CompressViewModel>();
        services.AddSingleton<MergeViewModel>();
        services.AddSingleton<SplitViewModel>();
        services.AddSingleton<ReorganizeViewModel>();
        services.AddSingleton<MainViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
