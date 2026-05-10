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
using Plugin.LocalNotification;

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
            builder.Services.AddTransient<ICalculadoraAdesao, CalculadoraAdesaoService>();
            builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
            builder.Services.AddScoped<IEmailOutboxProcessor, EmailOutboxProcessor>();
            builder.Services.AddScoped<IMediaManagerService, MediaManagerService>();
            // --- REGISTRO DE UI (VIEWS E VIEWMODELS) ---

            // Registre as ViewModels
            builder.Services.AddTransient<CadastroViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<CadastroMedicamentoViewModel>();
            builder.Services.AddTransient<ListaMedicamentosViewModel>();
            builder.Services.AddTransient<ListaPacientesViewModel>();
            builder.Services.AddTransient<CadastroPacienteViewModel>();
            builder.Services.AddTransient<AlarmeViewModel>();
            builder.Services.AddTransient<WelcomeViewModel>();
            // Registre as Pages
            builder.Services.AddTransient<CadastroPage>();
            builder.Services.AddTransient<CadastroMedicamentoPage>();
            builder.Services.AddTransient<ListaMedicamentosPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<ListaPacientesPage>();
            builder.Services.AddTransient<CadastroPacientePage>();
            builder.Services.AddTransient<WelcomePage>();
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