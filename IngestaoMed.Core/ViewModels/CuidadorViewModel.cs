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

        private Cuidador? _cuidadorIdAtualObjeto;

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

        public async Task InicializarAsync()
        {
            var cuidadorId = 0;
            if (_configuracao.ConfiguracaoCuidador != null)
            {
                cuidadorId = _configuracao.ConfiguracaoCuidador.Id;
            }

            if (cuidadorId > 0)
            {
                _cuidadorIdAtualObjeto = await _cuidadorService.ObterPorIdAsync(cuidadorId);
            }

            EhEdicao = _cuidadorIdAtualObjeto != null;
            TextoBotaoAcao = EhEdicao ? "SALVAR ALTERAÇÕES" : "CONCLUIR CADASTRO";

            if (EhEdicao && _cuidadorIdAtualObjeto != null)
            {
                try
                {
                    NomeCuidador = _cuidadorIdAtualObjeto.Nome;
                    EmailCuidador = _cuidadorIdAtualObjeto.Email;
                    TelefoneCuidador = _cuidadorIdAtualObjeto.Telefone;

                    SenhaCuidador = string.Empty;
                    ConfirmacaoSenhaCuidador = string.Empty;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao carregar dados do cuidador: {ex.Message}");
                }
            }
            else
            {
                _cuidadorIdAtualObjeto = null;
                NomeCuidador = string.Empty;
                EmailCuidador = string.Empty;
                TelefoneCuidador = string.Empty;
                SenhaCuidador = string.Empty;
                ConfirmacaoSenhaCuidador = string.Empty;
            }
        }

        [RelayCommand]
        private async Task Voltar()
        {
            await _navegacao.GoToAsync("ListaPacientesPage");
        }

        [RelayCommand]
        private async Task SalvarCuidador()
        {
            if (!await ValidarFormularioAsync()) return;

            try
            {
                string telefoneApenasNumeros = Regex.Replace(TelefoneCuidador ?? string.Empty, @"[^\d]", "");

                // Se for Modo Edição (cuidador logado), garante que temos o objeto preenchido
                if (EhEdicao)
                {
                    if (_cuidadorIdAtualObjeto == null)
                    {
                        await _dialogService.DisplayAlert("Erro", "Erro ao recuperar os dados do cuidador logado.", "OK");
                        return;
                    }
                }
                else
                {
                    // Se NÃO for edição (Cadastro Novo), cria uma nova instância limpa
                    _cuidadorIdAtualObjeto = new Cuidador();
                }

                // Alimenta as propriedades na instância correta
                _cuidadorIdAtualObjeto.Nome = NomeCuidador;
                _cuidadorIdAtualObjeto.Email = EmailCuidador;
                _cuidadorIdAtualObjeto.Telefone = telefoneApenasNumeros;

                // Se uma nova senha foi digitada, atualiza o campo. 
                // Se for edição e ficou em branco, mantém o hash que veio do banco.
                if (!string.IsNullOrWhiteSpace(SenhaCuidador))
                {
                    _cuidadorIdAtualObjeto.PasswordHash = SenhaCuidador;
                }

                // O serviço agora decide internamente: se Id > 0 atualiza no banco, senão registra criptografando via AuthService
                bool sucesso = await _cuidadorService.SalvarCuidadorAsync(_cuidadorIdAtualObjeto);

                if (sucesso)
                {
                    // Atualiza o estado da sessão local no IConfigService com os novos dados salvos
                    _configuracao.ConfiguracaoCuidador = new ConfiguracaoCuidador
                    {
                        Id = _cuidadorIdAtualObjeto.Id,
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

            if (!EhEdicao)
            {
                if (string.IsNullOrWhiteSpace(SenhaCuidador) || string.IsNullOrWhiteSpace(ConfirmacaoSenhaCuidador))
                {
                    await _dialogService.DisplayAlert("Campo Obrigatório", "Por favor, preencha a senha e a confirmação de senha.", "OK");
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(SenhaCuidador) || !string.IsNullOrWhiteSpace(ConfirmacaoSenhaCuidador))
            {
                if (SenhaCuidador != ConfirmacaoSenhaCuidador)
                {
                    await _dialogService.DisplayAlert("Senhas Diferem", "A senha informada e a confirmação não coincidem. Verifique e tente novamente.", "OK");
                    return false;
                }
            }

            return true;
        }
    }
}