using IngestaoMed.Core.Data; // Namespace da pasta Data
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.ViewModels;
using IngestaoMed.Services;
using IngestaoMed.Views;
using Microsoft.Extensions.Logging;

namespace IngestaoMed
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // --- SERVIÇOS DE INFRAESTRUTURA E INTERFACE ---

            builder.Services.AddSingleton<IDialogService, MauiDialogService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();

            // Define o caminho do arquivo .db3
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "IngestaoMed.db3");
            System.Diagnostics.Debug.Write(dbPath);
            // Registra o Banco
            builder.Services.AddSingleton<IDatabaseContext>(s =>
                ActivatorUtilities.CreateInstance<DatabaseContext>(s, dbPath));

            // --- SERVIÇOS DE NEGÓCIO ---

            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IMedicamentoService, MedicamentoService>();

            // --- REGISTRO DE UI (VIEWS E VIEWMODELS) ---

            // Registre as ViewModels
            builder.Services.AddTransient<CadastroViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<CadastroMedicamentoViewModel>();
            builder.Services.AddTransient<CadastroMedicamentoPage>();

            // Registre as Pages
            builder.Services.AddTransient<CadastroPage>();

            // STARTUP
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<App>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}