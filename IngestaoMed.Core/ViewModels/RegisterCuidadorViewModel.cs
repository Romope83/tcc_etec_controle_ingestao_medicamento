/*using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Security.Cryptography;
using System.Text;

namespace IngestaoMed.Core.ViewModels
{
    public partial class RegisterCuidadorViewModel : ObservableObject
    {
        private readonly INavigationService _navegacao;
        private readonly IConfigService _configuracao;
        private readonly IRepository<Cuidador> _cuidadorRepository;

        [ObservableProperty]
        private string nomeCuidador = string.Empty;

        [ObservableProperty]
        private string emailCuidador = string.Empty;

        [ObservableProperty]
        private string telefoneCuidador = string.Empty;

        [ObservableProperty]
        private string senhaCuidador = string.Empty;

        public RegisterCuidadorViewModel(
            INavigationService navegacao,
            IConfigService configuracao,
            IRepository<Cuidador> cuidadorRepository)
        {
            _navegacao = navegacao;
            _configuracao = configuracao;
            _cuidadorRepository = cuidadorRepository;
        }

        [RelayCommand]
        private async Task SalvarCuidador()
        {
            if (string.IsNullOrWhiteSpace(NomeCuidador) ||
                string.IsNullOrWhiteSpace(EmailCuidador) ||
                string.IsNullOrWhiteSpace(SenhaCuidador))
            {
                return;
            }

            try
            {
                string senhaHasheada = GerarHashSha256(SenhaCuidador);

                var novoCuidador = new Cuidador
                {
                    Nome = NomeCuidador,
                    Email = EmailCuidador,
                    Telefone = TelefoneCuidador,
                    PasswordHash = senhaHasheada
                };

                await _cuidadorRepository.InsertAsync(novoCuidador);

                _configuracao.ConfiguracaoCuidador = new ConfiguracaoCuidador
                {
                    Id = novoCuidador.Id,
                    NomeCuidador = NomeCuidador,
                    EmailCuidador = EmailCuidador,
                    AlertaAtivado = true,
                    LimiteSonecasParaAlerta = 3
                };

                _configuracao.EmailCuidadorConfigurado = EmailCuidador;
                _configuracao.EhPrimeiroAcesso = false;

                await _navegacao.GoToAsync("//MainPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private string GerarHashSha256(string senha)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
            var builder = new StringBuilder();
            foreach (var b in bytes) builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
}*/