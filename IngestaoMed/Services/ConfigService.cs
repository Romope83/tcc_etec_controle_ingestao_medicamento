using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Text.Json;

namespace IngestaoMed.Services
{
    public class ConfigService : IConfigService
    {
        private const string PrimeiroAcessoKey = "primeiro_acesso";
        private const string EmailCuidadorKey = "email_cuidador";
        private const string ConfigCuidadorKey = "config_cuidador";

        public bool EhPrimeiroAcesso
        {
            get => Preferences.Default.Get(PrimeiroAcessoKey, true);
            set => Preferences.Default.Set(PrimeiroAcessoKey, value);
        }

        public string? EmailCuidadorConfigurado
        {
            get => Preferences.Default.Get(EmailCuidadorKey, (string?)null);
            set => Preferences.Default.Set(EmailCuidadorKey, value);
        }

        public ConfiguracaoCuidador? ConfiguracaoCuidador
        {
            get
            {
                var json = Preferences.Default.Get(ConfigCuidadorKey, string.Empty);

                if (string.IsNullOrWhiteSpace(json))
                    return null;

                try
                {
                    return JsonSerializer.Deserialize<ConfiguracaoCuidador>(json);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                {
                    Preferences.Default.Remove(ConfigCuidadorKey);
                }
                else
                {
                    var json = JsonSerializer.Serialize(value);
                    Preferences.Default.Set(ConfigCuidadorKey, json);
                }
            }
        }

        public void RemoverSessaoCuidador()
        {
            Preferences.Default.Remove(ConfigCuidadorKey);
        }
    }
}