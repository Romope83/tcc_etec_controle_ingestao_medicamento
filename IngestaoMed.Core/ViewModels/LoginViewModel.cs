using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Services;

namespace IngestaoMed.Core.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string senha = string.Empty;

        public LoginViewModel(IAuthService authService, IDialogService dialogService, INavigationService navigationService)
        {
            _authService = authService;
            _dialogService = dialogService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task Entrar()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
            {
                await _dialogService.DisplayAlert("Erro", "Preencha todos os campos.", "OK");
                return;
            }

            bool sucesso = await _authService.ValidarLogin(Email, Senha);

            if (sucesso)
            {
                await _navigationService.GoToAsync("//CadastroMedPage");
            }
            else
            {
                await _dialogService.DisplayAlert("Erro", "E-mail ou senha inválidos.", "OK");
            }
        }
    }
}