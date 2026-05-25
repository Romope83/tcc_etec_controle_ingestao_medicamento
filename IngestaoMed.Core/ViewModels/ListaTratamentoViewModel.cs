using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Collections.ObjectModel;

namespace IngestaoMed.Core.ViewModels
{
    public partial class ListaTratamentosViewModel : ObservableObject
    {
        private readonly ITratamentoService _tratamentoService;
        private readonly INavigationService _navigation;
        private readonly IDialogService _dialog;

        private List<Tratamento> _todosTratamentos = new();

        public ObservableCollection<Tratamento> TratamentosFiltrados { get; } = new();

        [ObservableProperty]
        private string _textoBusca = string.Empty;

        partial void OnTextoBuscaChanged(string value) => AplicarFiltro(value);

        public ListaTratamentosViewModel(
            ITratamentoService tratamentoService,
            INavigationService navigation,
            IDialogService dialog)
        {
            _tratamentoService = tratamentoService;
            _navigation = navigation;
            _dialog = dialog;
        }

        public async Task CarregarTratamentosAsync()
        {
            var lista = await _tratamentoService.ObterTodosAsync();
            _todosTratamentos = lista;
            AplicarFiltro(TextoBusca);
        }

        private void AplicarFiltro(string busca)
        {
            var filtrados = string.IsNullOrWhiteSpace(busca)
                ? _todosTratamentos
                : _todosTratamentos.Where(t =>
                    t.Nome != null &&
                    t.Nome.Contains(busca, StringComparison.OrdinalIgnoreCase));

            TratamentosFiltrados.Clear();
            foreach (var t in filtrados)
                TratamentosFiltrados.Add(t);
        }

        [RelayCommand]
        private async Task SelecionarTratamentoAsync(int tratamentoId)
        {
            if (tratamentoId <= 0) return;
            await _navigation.GoToAsync($"TratamentoPage?tratamentoId={tratamentoId}");
        }

        [RelayCommand]
        private async Task NovoTratamentoAsync()
        {
            await _navigation.GoToAsync("TratamentoPage");
        }

        [RelayCommand]
        private async Task RemoverTratamentoAsync(Tratamento tratamento)
        {
            if (tratamento == null) return;

            bool confirmar = await _dialog.DisplayConfirmationAsync(
                "Excluir",
                $"Deseja remover o tratamento \"{tratamento.Nome}\"?",
                "Sim", "Não");

            if (!confirmar) return;

            bool sucesso = await _tratamentoService.ExcluirTratamentoAsync(tratamento) > 0;
            if (sucesso)
            {
                _todosTratamentos.Remove(tratamento);
                TratamentosFiltrados.Remove(tratamento);
            }
            else
            {
                await _dialog.DisplayAlert("Erro", "Não foi possível remover o tratamento.", "OK");
            }
        }
    }
}