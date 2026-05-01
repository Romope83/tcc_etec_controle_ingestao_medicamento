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

        [ObservableProperty]
        private string nome;

        [ObservableProperty]
        private string telefone;

        [ObservableProperty]
        private string email;

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

        [RelayCommand]
        private async Task SalvarAsync()
        {
            if (string.IsNullOrWhiteSpace(Nome))
            {
                await _dialogService.DisplayAlert("Erro", "O nome é obrigatório.", "OK");
                return;
            }

            var paciente = new Paciente
            {
                Nome = Nome,
                Telefone = Telefone,
                Email = Email
            };

            if (!ValidarFormatoEmail(Email))
            {
                await _dialogService.DisplayAlert("E-mail Inválido", "Por favor, insira um e-mail com formato correto (ex@email.com).", "OK");
                return;
            }


            bool emailJaExiste = await _authService.ValidarEmail(Email);
            if (emailJaExiste)
            {
                await _dialogService.DisplayAlert("Erro", "Este e-mail já está cadastrado no sistema.", "OK");
                return;
            }


            bool sucesso = await _pacienteService.SalvarPacienteAsync(paciente);
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