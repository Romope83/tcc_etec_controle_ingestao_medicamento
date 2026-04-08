using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Interfaces;
using System.Text.RegularExpressions;

namespace IngestaoMed.Core.ViewModels
{
    public partial class CadastroViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private string _nome = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _senha = string.Empty;

        [ObservableProperty]
        private string? _telefone;


        public CadastroViewModel(IAuthService authService, IDialogService dialogService)
        {
            _authService = authService;
            _dialogService = dialogService;
        }

        [RelayCommand]
        private async Task SalvarCadastro()
        {
            // 1. Validação de Campos Vazios
            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
            {
                await _dialogService.DisplayAlert("Erro", "Preencha todos os campos obrigatórios.", "OK");
                return;
            }

            // 2. Validação de Formato de E-mail (Regex)
            if (!ValidarFormatoEmail(Email))
            {
                await _dialogService.DisplayAlert("E-mail Inválido", "Por favor, insira um e-mail com formato correto (ex@email.com).", "OK");
                return;
            }

            // 3. Critérios Mínimos de Senha (6 caracteres)
            if (Senha.Length < 6)
            {
                await _dialogService.DisplayAlert("Senha Fraca", "A senha deve ter pelo menos 6 caracteres.", "OK");
                return;
            }

            // 4. Verificação de E-mail Duplicado (Regra de Negócio no Banco)
            bool emailJaExiste = await _authService.ValidarEmail(Email);
            if (emailJaExiste)
            {
                await _dialogService.DisplayAlert("Erro", "Este e-mail já está cadastrado no sistema.", "OK");
                return;
            }

            // Se passou por tudo, aí sim salvamos
            var novoCuidador = new Cuidador { Nome = Nome, Email = Email, Telefone = Telefone, PasswordHash = string.Empty };
            bool sucesso = await _authService.RegistrarCuidador(novoCuidador, Senha);

            if (sucesso)
            {
                await _dialogService.DisplayAlert("Sucesso", "Cuidador cadastrado com sucesso!", "OK");
                // Futuro: Navegar para Login ou Home
            }
        }

        // Método auxiliar para Regex de e-mail
        private bool ValidarFormatoEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }
    }
}