using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Models;
using IngestaoMed.Services;

namespace IngestaoMed.ViewModels
{
    public partial class CadastroViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _nome = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string? _telefone;

        public CadastroViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task SalvarCadastro()
        {
            // Validação simples (Regra de Negócio)
            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Email))
            {
                await Shell.Current.DisplayAlert("Erro", "Nome e E-mail são obrigatórios.", "OK");
                return;
            }

            var novoCuidador = new Cuidador
            {
                Nome = Nome,
                Email = Email,
                Telefone = Telefone
            };

            bool sucesso = await _authService.RegistrarCuidador(novoCuidador);

            if (sucesso)
            {
                await Shell.Current.DisplayAlert("Sucesso", "Cuidador cadastrado!", "OK");
                // Aqui depois faremos a navegação para a Main Page
            }
            else
            {
                await Shell.Current.DisplayAlert("Erro", "Falha ao salvar no banco.", "OK");
            }
        }
    }
}