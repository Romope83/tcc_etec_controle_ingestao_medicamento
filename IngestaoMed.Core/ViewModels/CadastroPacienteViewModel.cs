using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Text.RegularExpressions;
using IngestaoMed.Core.Services;






namespace IngestaoMed.Core.ViewModels
{
    public partial class CadastroPacienteViewModel : ObservableObject
    {
        private readonly IPacienteService _pacienteService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IAuthService _authService;

        [ObservableProperty] private string nome = string.Empty;
        [ObservableProperty] private string telefone = string.Empty;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private DateTime dataNascimento = DateTime.Today.AddYears(-20);
        [ObservableProperty] private string? fotoPerfilPath;
        [ObservableProperty] private bool ehEdicao = false;


        public CadastroPacienteViewModel(IPacienteService pacienteService,
                                       INavigationService navigationService,
                                       IDialogService dialogService,
                                       IAuthService authService)
        {
            _pacienteService = pacienteService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _authService = authService;

        }
        partial void OnTelefoneChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            // Remove tudo que não é dígito para processar
            var numeros = Regex.Replace(value, @"[^\d]", "");

            if (numeros.Length > 11) numeros = numeros.Substring(0, 11);

            // Aplica a formatação enquanto o usuário digita
            if (numeros.Length >= 11)
            {
                Telefone = $"({numeros.Substring(0, 2)}) {numeros.Substring(2, 5)}-{numeros.Substring(7)}";
            }
            else if (numeros.Length >= 7)
            {
                Telefone = $"({numeros.Substring(0, 2)}) {numeros.Substring(2, 4)}-{numeros.Substring(6)}";
            }
        }

        [RelayCommand]
        private async Task SalvarAsync()
        {
            // Validações
            if (string.IsNullOrWhiteSpace(Nome))
            {
                await _dialogService.DisplayAlert("Erro", "O nome é obrigatório.", "OK");
                return;
            }

            if (!ValidarFormatoEmail(Email))
            {
                await _dialogService.DisplayAlert("E-mail Inválido", "Por favor, insira um e-mail válido.", "OK");
                return;
            }

            // Verificação de e-mail no sistema
            bool emailJaExiste = await _authService.ValidarEmail(Email);
            if (emailJaExiste)
            {
                await _dialogService.DisplayAlert("Erro", "Este e-mail já está cadastrado.", "OK");
                return;
            }

            var paciente = new Paciente
            {
                Nome = Nome,
                Telefone = Telefone,
                Email = Email,
                DataNascimento = DataNascimento,
                FotoPerfilPath = FotoPerfilPath
            };

            bool sucesso = await _pacienteService.SalvarPacienteAsync(paciente);

            if (sucesso)
            {
                await _dialogService.DisplayAlert("Sucesso", "Paciente cadastrado com sucesso!", "OK");
                // Retorna para a lista de pacientes automaticamente
                await _navigationService.GoToAsync("..");
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