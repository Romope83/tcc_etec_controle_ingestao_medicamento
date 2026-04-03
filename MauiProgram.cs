using IngestaoMed.Data; // Namespace da pasta Data
using IngestaoMed.Services;
using IngestaoMed.ViewModels;
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

            // --- CONFIGURAÇÃO DO BANCO DE DADOS ---

            // Define o caminho do arquivo .db3 na pasta de dados local do dispositivo
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "IngestaoMed.db3");

            // Registra o DatabaseContext como Singleton
            // O uso de ActivatorUtilities permite passar o dbPath para o construtor
            builder.Services.AddSingleton<DatabaseContext>(s =>
                ActivatorUtilities.CreateInstance<DatabaseContext>(s, dbPath));


            // --- SERVIÇOS DE NEGÓCIO ---

            // Registra a Interface e a Implementação como Singleton
            builder.Services.AddSingleton<IAuthService, AuthService>();


            // --- REGISTRO DE UI (VIEWS E VIEWMODELS) ---

            // Usamos Transient para que a tela seja "limpa" toda vez que entrarmos nela
            builder.Services.AddTransient<CadastroViewModel>();
            builder.Services.AddTransient<CadastroPage>();

            //  STARTUP:
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<App>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}