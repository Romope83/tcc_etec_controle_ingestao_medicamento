using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;

namespace IngestaoMed.Core.ViewModels
{
    public partial class ListaMedicamentosViewModel : ObservableObject
    {
        private readonly IMedicamentoService _medicamentoService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        // Coleção completa (fonte de verdade)
        private List<Medicamento> _todosMedicamentos = new();

        // Coleção filtrada exibida na tela
        public ObservableCollection<Medicamento> Medicamentos { get; } = new();

        [ObservableProperty]
        private string _textoBusca = string.Empty;

        // Sempre que TextoBusca mudar, o CommunityToolkit chama este método automaticamente
        partial void OnTextoBuscaChanged(string value) => AplicarFiltro(value);

        public ListaMedicamentosViewModel(IMedicamentoService medicamentoService,
                                          INavigationService navigationService,
                                          IDialogService dialogService)
        {
            _medicamentoService = medicamentoService;
            _navigationService = navigationService;
            _dialogService = dialogService;
        }

        public ListaMedicamentosViewModel(IMedicamentoService medicamentoService,
                                          INavigationService navigationService)
        {
            _medicamentoService = medicamentoService;
            _navigationService = navigationService;
        }

        private void AplicarFiltro(string busca)
        {
            var filtrados = string.IsNullOrWhiteSpace(busca)
                ? _todosMedicamentos
                : _todosMedicamentos.Where(m =>
                    m.NomeComercial.Contains(busca, StringComparison.OrdinalIgnoreCase) ||
                    m.Dosagem.Contains(busca, StringComparison.OrdinalIgnoreCase) ||
                    m.FormaIngestao.Contains(busca, StringComparison.OrdinalIgnoreCase));

            Medicamentos.Clear();
            foreach (var med in filtrados)
                Medicamentos.Add(med);
        }

        [RelayCommand]
        private async Task CarregarMedicamentosAsync()
        {
            var lista = await _medicamentoService.ObterTodosAsync();

            // Salva a lista completa para uso no filtro
            _todosMedicamentos = lista.ToList();

            // Aplica o filtro atual (importante para quando a página recarrega com busca ativa)
            AplicarFiltro(TextoBusca);
        }

        [RelayCommand]
        private async Task NavegarParaCadastroAsync()
        {
            await _navigationService.GoToAsync("CadastroMedicamentoPage");
        }

        [RelayCommand]
        private async Task RemoverMedicamentoAsync(Medicamento medicamento)
        {
            if (medicamento == null) return;

            bool confirmar = await _dialogService.DisplayConfirmationAsync(
                "Excluir",
                $"Deseja realmente remover o medicamento {medicamento.NomeComercial}?",
                "Sim", "Não");

            if (!confirmar) return;

            bool sucesso = await _medicamentoService.RemoverMedicamentoAsync(medicamento);

            if (sucesso)
            {
                // Remove das duas coleções para manter consistência
                _todosMedicamentos.Remove(medicamento);
                Medicamentos.Remove(medicamento);
            }
            else
            {
                await _dialogService.DisplayAlert("Erro", "Não foi possível remover o medicamento.", "OK");
            }
        }
    }
}