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

        public ObservableCollection<Medicamento> Medicamentos { get; } = new();

        public ListaMedicamentosViewModel(IMedicamentoService medicamentoService, INavigationService navigationService)
        {
            _medicamentoService = medicamentoService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task CarregarMedicamentosAsync()
        {
            var lista = await _medicamentoService.ObterTodosAsync();

            Medicamentos.Clear();
            foreach (var med in lista)
            {
                Medicamentos.Add(med);
            }
        }

        [RelayCommand]
        private async Task NavegarParaCadastroAsync()
        {
            await _navigationService.GoToAsync("CadastroMedPage");
        }
    }
}