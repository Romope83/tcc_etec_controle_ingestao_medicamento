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
        public ObservableCollection<Medicamento> Medicamentos { get; } = new();

        // Atualize o construtor para receber o IDialogService
        public ListaMedicamentosViewModel(IMedicamentoService medicamentoService,
                                          INavigationService navigationService,
                                          IDialogService dialogService)
        {
            _medicamentoService = medicamentoService;
            _navigationService = navigationService;
            _dialogService = dialogService;
        }
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
            await _navigationService.GoToAsync("CadastroMedicamentoPage");
        }

            [RelayCommand]
            private async Task RemoverMedicamentoAsync(Medicamento medicamento)
            {
                if (medicamento == null) return;

                // 1. Pede confirmação ao usuário
                bool confirmar = await _dialogService.DisplayConfirmationAsync(
                    "Excluir",
                    $"Deseja realmente remover o medicamento {medicamento.NomeComercial}?",
                    "Sim", "Não");

                if (!confirmar) return;

                // 2. Remove do banco de dados
                bool sucesso = await _medicamentoService.RemoverMedicamentoAsync(medicamento);

                if (sucesso)
                {
                    // 3. Remove da interface em tempo real (MainThread para evitar travamentos)
                    
                    
                        Medicamentos.Remove(medicamento);
                    
                }
                else
                {
                    await _dialogService.DisplayAlert("Erro", "Não foi possível remover o medicamento.", "OK");
                }
            }
        }
    }
