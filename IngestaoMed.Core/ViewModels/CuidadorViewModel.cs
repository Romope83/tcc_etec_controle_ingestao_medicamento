using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IngestaoMed.Core.ViewModels
{
    public partial class CuidadorViewModel : ObservableObject
    {
        private readonly INavigationService _navegacao;
        private readonly IConfigService _configuracao;
        private readonly ICuidadorService _cuidadorService;
        private readonly IDialogService _dialogService;

        private int _cuidadorIdAtual;

        [ObservableProperty] private string nomeCuidador = string.Empty;
        [ObservableProperty] private string emailCuidador = string.Empty;
        [ObservableProperty] private string telefoneCuidador = string.Empty;
        [ObservableProperty] private string senhaCuidador = string.Empty;
        [ObservableProperty] private string confirmacaoSenhaCuidador = string.Empty;
        [ObservableProperty] private bool ehEdicao = false;

        [ObservableProperty] private string textoBotaoAcao = "CONCLUIR CADASTRO";

        public CuidadorViewModel(
            INavigationService navegacao,
            IConfigService configuracao,
            ICuidadorService cuidadorService,
            IDialogService dialogService)
        {
            _navegacao = navegacao;
            _configuracao = configuracao;
            _cuidadorService = cuidadorService;
            _dialogService = dialogService;
        }

        public async Task InicializarAsync(int cuidadorId)
        {
            _cuidadorIdAtual = cuidadorId;
            EhEdicao = _cuidadorIdAtual > 0;

            TextoBotaoAcao = EhEdicao ? "SALVAR ALTERAÇÕES" : "CONCLUIR CADASTRO";

            if (EhEdicao)
            {
                // Código para carregar dados do cuidador se for modo edição...
            }
        }

        [RelayCommand]
        private async Task SalvarCuidador()
        {
            if (!await ValidarFormularioAsync()) return;

            try
            {
                string telefoneApenasNumeros = Regex.Replace(TelefoneCuidador ?? string.Empty, @"[^\d]", "");

                var cuidador = new Cuidador
                {
                    Id = _cuidadorIdAtual,
                    Nome = NomeCuidador,
                    Email = EmailCuidador,
                    Telefone = telefoneApenasNumeros,
                    PasswordHash = SenhaCuidador
                };

                bool sucesso = await _cuidadorService.SalvarCuidadorAsync(cuidador);

                if (sucesso)
                {
                    _configuracao.ConfiguracaoCuidador = new ConfiguracaoCuidador
                    {
                        Id = cuidador.Id,
                        NomeCuidador = NomeCuidador,
                        EmailCuidador = EmailCuidador,
                        AlertaAtivado = true,
                        LimiteSonecasParaAlerta = 3
                    };

                    _configuracao.EmailCuidadorConfigurado = EmailCuidador;
                    _configuracao.EhPrimeiroAcesso = false;

                    await _dialogService.DisplayAlert("Sucesso", EhEdicao ? "Cadastro atualizado com sucesso!" : "Cadastro concluído com sucesso!", "OK");
                    await _navegacao.GoToAsync("//ListaPacientesPage");
                }
                else
                {
                    await _dialogService.DisplayAlert("Erro", "Não foi possível salvar os dados do cuidador.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async Task<bool> ValidarFormularioAsync()
        {
            if (string.IsNullOrWhiteSpace(NomeCuidador))
            {
                await _dialogService.DisplayAlert("Campo Obrigatório", "Por favor, insira o nome do cuidador.", "OK");
                return false;
            }

            string telefoneLimpo = Regex.Replace(TelefoneCuidador ?? string.Empty, @"[^\d]", "");
            if (string.IsNullOrWhiteSpace(telefoneLimpo))
            {
                await _dialogService.DisplayAlert("Campo Obrigatório", "Por favor, insira o telefone do cuidador.", "OK");
                return false;
            }
            if (telefoneLimpo.Length < 11)
            {
                await _dialogService.DisplayAlert("Telefone Inválido", "O número de telefone deve conter no mínimo 11 dígitos (DDD + Número).", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmailCuidador))
            {
                await _dialogService.DisplayAlert("Campo Obrigatório", "Por favor, insira o e-mail do cuidador.", "OK");
                return false;
            }
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(EmailCuidador))
            {
                await _dialogService.DisplayAlert("E-mail Inválido", "Por favor, insira um endereço de e-mail válido.", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(SenhaCuidador) || string.IsNullOrWhiteSpace(ConfirmacaoSenhaCuidador))
            {
                await _dialogService.DisplayAlert("Campo Obrigatório", "Por favor, preencha a senha e a confirmação de senha.", "OK");
                return false;
            }
            if (SenhaCuidador != ConfirmacaoSenhaCuidador)
            {
                await _dialogService.DisplayAlert("Senhas Diferem", "A senha informada e a confirmação não coincidem. Verifique e tente novamente.", "OK");
                return false;
            }

            return true;
        }
    }
}