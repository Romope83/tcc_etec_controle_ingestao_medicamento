using IngestaoMed.Core.Interfaces;

namespace IngestaoMed.Services
{
    public class ConfigService : IConfigService
    {
        private const string PrimeiroAcessoKey = "primeiro_acesso";
        private const string EmailCuidadorKey = "email_cuidador";

        public bool EhPrimeiroAcesso
        {
            get => Preferences.Default.Get(PrimeiroAcessoKey, true); // Padrão é true
            set => Preferences.Default.Set(PrimeiroAcessoKey, value);
        }

        public string? EmailCuidadorConfigurado
        {
            get => Preferences.Default.Get(EmailCuidadorKey, (string?)null);
            set => Preferences.Default.Set(EmailCuidadorKey, value);
        }
    }
}