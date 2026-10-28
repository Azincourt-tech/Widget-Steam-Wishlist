using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using SteamWishlistWidget.Constants;
using SteamWishlistWidget.Models;
using SteamWishlistWidget.Platform;
using SteamWishlistWidget.Services;
using SteamWishlistWidget.Services.Abstractions;
using SteamWishlistWidget.Services.Demo;
using SteamWishlistWidget.Themes;
using SteamWishlistWidget.ViewModels;
using SteamWishlistWidget.Views;

namespace SteamWishlistWidget;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Dispatcher.UIThread.UnhandledException += (_, e) =>
            {
                // Erro inesperado na interface: registra e mantém o widget aberto.
                ErrorLog.Write("Erro não tratado na interface.", e.Exception);
                e.Handled = true;
            };

            desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnMainWindowClose;
            desktop.MainWindow = CreateMainWindow(StartupOptions.Parse(desktop.Args ?? []));
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Composição: cada serviço é criado uma vez aqui e entregue a quem precisa.
    /// </summary>
    private static MainWindow CreateMainWindow(StartupOptions options)
    {
        ISettingsStore store;
        ISteamApiClient steam;
        IStartupRegistration startup;
        INotificationService notifications;
        if (options.Demo)
        {
            // Dados fictícios, sem tocar no config.json, no registro nem na conta Steam do usuário.
            var demoSettings = new AppSettings { SteamId = DemoSteamApiClient.DemoSteamId, Theme = options.Theme ?? ThemeNames.Glass };
            store = new InMemorySettingsStore(demoSettings);
            steam = new DemoSteamApiClient();
            startup = new InMemoryStartupRegistration();
            notifications = new NoNotificationService();
        }
        else
        {
            store = new SettingsStore(AppConstants.SettingsFile, PlatformServices.CreateSecretProtector());
            steam = new SteamApiClient();
            startup = PlatformServices.CreateStartupRegistration();
            notifications = PlatformServices.CreateNotificationService();
        }

        var settings = store.Load();
        var themes = new ThemeService();
        var dialogs = new DialogService(steam, store, startup, themes);

        // As cores precisam existir antes da primeira janela ser criada.
        themes.Apply(settings.Theme);

        var viewModel = new WidgetViewModel(settings, steam, store, new StoreLauncher(), themes, dialogs, notifications, new AvaloniaUiTimer());
        return new MainWindow(viewModel);
    }
}
