using CommunityToolkit.Maui;
using IngestaoMed;
using IngestaoMed.Core.Data; // Namespace da pasta Data
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.ViewModels;
using IngestaoMed.Interfaces;
using IngestaoMed.Services;
using IngestaoMed.Services.Notifications;
using IngestaoMed.Views;
using IngestaoMed.Views.Onboarding;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Plugin.LocalNotification;
#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using WinRT.Interop; // Necessário para o WindowNative
#endif
namespace IngestaoMed
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseLocalNotification()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureLifecycleEvents(events =>
                {
#if WINDOWS
                events.AddWindows(windows => windows
                    .OnWindowCreated(window =>
                    {
                        window.ExtendsContentIntoTitleBar = false;
                        var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                        var id = Win32Interop.GetWindowIdFromWindow(handle);
                        var appWindow = AppWindow.GetFromWindowId(id);

                        // Define as dimensões (Largura, Altura)
                        appWindow.Resize(new SizeInt32(450, 800));
                    }));
#endif

                });

            // --- SERVIÇOS DE INFRAESTRUTURA E INTERFACE ---

            builder.Services.AddSingleton<IDialogService, MauiDialogService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<IVibrationService, VibrationService>();
            builder.Services.AddSingleton<IAlarmService, AlarmService>();
            builder.Services.AddSingleton<INotificationActionService, NotificationActionService>();
            builder.Services.AddSingleton<INotificationMapper, NotificationMapper>();
            builder.Services.AddSingleton<ISnoozeService, SnoozeService>();
            builder.Services.AddScoped<IEmailService, MailKitService>();
            builder.Services.AddSingleton<IFileStorageService, FileSystemService>();
            builder.Services.AddSingleton<IConfigService, ConfigService>();
            // Define o caminho do arquivo .db3
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "IngestaoMed.db3");
            System.Diagnostics.Debug.Write(dbPath);
            // Registra o Banco
            builder.Services.AddSingleton<IDatabaseContext>(s =>
                ActivatorUtilities.CreateInstance<DatabaseContext>(s, dbPath));

            // --- SERVIÇOS DE NEGÓCIO ---

            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IMedicamentoService, MedicamentoService>();
            builder.Services.AddSingleton<IPacienteService, PacienteService>();
            builder.Services.AddSingleton<ILogService, LogService>();
            builder.Services.AddSingleton<IMediaPickerService, MauiMediaPickerService>();  
            builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
            builder.Services.AddTransient<ICalculadoraAdesao, CalculadoraAdesaoService>();
            builder.Services.AddScoped<IEmailOutboxProcessor, EmailOutboxProcessor>();
            builder.Services.AddScoped<IMediaManagerService, MediaManagerService>(); 
            builder.Services.AddScoped<ITratamentoService, TratamentoService>();
            // --- REGISTRO DE UI (VIEWS E VIEWMODELS) ---

            // Registre as ViewModels
            builder.Services.AddTransient<CadastroViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<MedicamentoViewModel>();
            builder.Services.AddTransient<ListaMedicamentosViewModel>();
            builder.Services.AddTransient<ListaPacientesViewModel>();
            builder.Services.AddTransient<PacienteViewModel>();
            builder.Services.AddTransient<AlarmeViewModel>();
            builder.Services.AddTransient<WelcomeViewModel>();
            builder.Services.AddTransient<TratamentoViewModel>();
            builder.Services.AddTransient<PacienteDetalhesViewModel>();
            //builder.Services.AddTransient<EdicaoTratamentoViewModel>();
            builder.Services.AddTransient<AgendamentoViewModel>();

            //builder.Services.AddTransient<RegisterCuidadorViewModel>();
            // Registre as Pages
            builder.Services.AddTransient<CadastroPage>();
            builder.Services.AddTransient<MedicamentoPage>();
            builder.Services.AddTransient<ListaMedicamentosPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<ListaPacientesPage>();
            builder.Services.AddTransient<PacientePage>();
            builder.Services.AddTransient<WelcomePage>();
            builder.Services.AddTransient<RegisterCuidadorPage>();
            builder.Services.AddTransient<PacienteDetalhesPage>();
            // STARTUP
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<App>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app =  builder.Build();

            var notificationService = app.Services.GetRequiredService<INotificationActionService>();
            notificationService.RegistrarAcoes();

            return app;
        }
    }
}