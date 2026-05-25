using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Text.RegularExpressions;

namespace IngestaoMed.Core.ViewModels
{
    public partial class CadastroViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IDialogService _dialogService;
        private readonly IConfigService _configService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _nome = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _senha = string.Empty;

        [ObservableProperty]
        private string? _telefone;

        [ObservableProperty]
        private DateTime _dataNascimento = DateTime.Now.AddYears(-30);

        [ObservableProperty]
        private string? _fotoPerfilPath = "dotnet_bot.png";

        public CadastroViewModel(
            IAuthService authService,
            IDialogService dialogService,
            IConfigService configService,
            INavigationService navigationService)
        {
            _authService = authService;
            _dialogService = dialogService;
            _configService = configService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task SalvarCadastro()
        {
            // Validações locais e de banco
            if (!await ValidarCamposAsync()) return;

            var novoCuidador = new Cuidador
            {
                Nome = Nome,
                Email = Email,
                Telefone = Telefone,
                PasswordHash = string.Empty // O Hash é processado internamente pelo IAuthService
            };

            bool sucesso = await _authService.RegistrarCuidador(novoCuidador, Senha);

            if (sucesso)
            {
                // Atualiza o estado global de configuração no IConfigService
                _configService.EhPrimeiroAcesso = false;
                _configService.EmailCuidadorConfigurado = Email;

                await _dialogService.DisplayAlert("Sucesso", "Perfil configurado com sucesso!", "OK");

                // Navegação via interface desacoplada
                await _navigationService.GoToAsync("//ListaPacientePage");
            }
            else
            {
                await _dialogService.DisplayAlert("Erro", "Não foi possível realizar o cadastro. Tente novamente mais tarde.", "OK");
            }
        }

        private async Task<bool> ValidarCamposAsync()
        {
            // 1. Validação de preenchimento
            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
            {
                await _dialogService.DisplayAlert("Campos Obrigatórios", "Por favor, preencha nome, e-mail e senha.", "OK");
                return false;
            }

            // 2. Validação de formato de e-mail
            if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                await _dialogService.DisplayAlert("E-mail Inválido", "Por favor, insira um endereço de e-mail válido.", "OK");
                return false;
            }

            // 3. Validação de segurança da senha
            if (Senha.Length < 6)
            {
                await _dialogService.DisplayAlert("Senha Curta", "A senha deve conter no mínimo 6 caracteres para sua segurança.", "OK");
                return false;
            }

            // 4. Verificação de e-mail duplicado no banco de dados
            bool emailJaExiste = await _authService.ValidarEmail(Email);
            if (emailJaExiste)
            {
                await _dialogService.DisplayAlert("E-mail em uso", "Este e-mail já está cadastrado. Tente recuperar sua senha ou use outro e-mail.", "OK");
                return false;
            }

            return true;
        }
    }
}