using Microsoft.Extensions.Logging;
using IngestaoMed.Data; // Namespace da pasta Data

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

            // --------------------------------------

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}