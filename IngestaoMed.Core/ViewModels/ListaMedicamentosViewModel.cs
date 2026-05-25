using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IngestaoMed.Core.ViewModels
{
    public partial class ListaMedicamentosViewModel : ObservableObject
    {
        private readonly IMedicamentoService _medicamentoService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        private List<Medicamento> _listaCompleta = new();

        public ObservableCollection<Medicamento> Medicamentos { get; } = new();

        [ObservableProperty]
        private string _textoBusca = string.Empty;

        public ListaMedicamentosViewModel(IMedicamentoService medicamentoService,
                                          INavigationService navigationService,
                                          IDialogService dialogService)
        {
            _medicamentoService = medicamentoService;
            _navigationService = navigationService;
            _dialogService = dialogService;
        }

        [RelayCommand]
        private async Task CarregarMedicamentosAsync()
        {
            // Busca a lista atualizada do banco de dados
            _listaCompleta = await _medicamentoService.ObterTodosAsync();

            // Aplica o filtro existente (caso o usuário tenha digitado algo antes do refresh)
            FiltrarLista();
        }

        // Executa automaticamente sempre que a propriedade TextoBusca for alterada no XAML
        partial void OnTextoBuscaChanged(string value)
        {
            FiltrarLista();
        }

        private void FiltrarLista()
        {
            Medicamentos.Clear();

            var resultado = string.IsNullOrWhiteSpace(TextoBusca)
                ? _listaCompleta
                : _listaCompleta.Where(m => m.NomeComercial != null &&
                                            m.NomeComercial.Contains(TextoBusca, StringComparison.OrdinalIgnoreCase));

            foreach (var med in resultado)
            {
                Medicamentos.Add(med);
            }
        }

        [RelayCommand]
        private async Task NavegarParaCadastroAsync(object? param)
        {
            // Se o parâmetro for um ID válido (int), envia na QueryString para abrir em modo edição
            if (param is int id && id > 0)
            {
                await _navigationService.GoToAsync($"MedicamentoPage?id={id}");
            }
            else
            {
                // Caso contrário (clique no botão flutuante de "+"), abre o formulário limpo
                await _navigationService.GoToAsync("MedicamentoPage");
            }
        }
    }
}