using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Collections.ObjectModel;

namespace IngestaoMed.Core.ViewModels
{
    public partial class PacienteDetalhesViewModel : ObservableObject
    {
        private readonly IDatabaseContext _db;
        private readonly IPacienteService _pacienteService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private Paciente? _pacienteSelecionado;

        public PacienteDetalhesViewModel(IDatabaseContext db, IPacienteService pacienteService, INavigationService navigationService)
        {
            _db = db;
            _pacienteService = pacienteService;
            _navigationService = navigationService;
        }

        public ObservableCollection<Tratamento> Tratamentos { get; } = new();

        // Inicialização chamada pela View
        public async Task InicializarAsync(int pacienteId)
        {
            Id = pacienteId;
            await CarregarDadosAsync(pacienteId);
        }

        private async Task CarregarDadosAsync(int pacienteId)
        {
            try
            {
                PacienteSelecionado = await _pacienteService.ObterDetalhesCompletosAsync(pacienteId);

                Tratamentos.Clear();
                if (PacienteSelecionado?.Tratamentos != null)
                {
                    foreach (var t in PacienteSelecionado.Tratamentos)
                    {
                        Tratamentos.Add(t);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar detalhes: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NovoTratamentoAsync()
        {
            // Envia o pacienteId para que o novo tratamento saiba a quem pertence
            await _navigationService.GoToAsync($"TratamentoPage?pacienteId={Id}");
        }

        [RelayCommand]
        private async Task EditarTratamentoAsync(int tratamentoId)
        {
            // Envia o tratamentoId para carregar os dados existentes na página de edição
            await _navigationService.GoToAsync($"TratamentoPage?tratamentoId={tratamentoId}");
        }

        [RelayCommand]
        private async Task EditarPacienteAsync(int pacienteId)
        {
            // Navega para a página de cadastro em modo edição
            await _navigationService.GoToAsync($"PacientePage?id={pacienteId}");
        }

        [RelayCommand]
        private async Task SelecionarMedicamentoVinculadoAsync(MedicamentoTratamento medicamento)
        {
            if (medicamento == null) return;

            await _navigationService.GoToAsync($"AgendamentoPage?tratamentoId={medicamento.TratamentoId}&medicamentoTratamentoId={medicamento.Id}");
        }
    }
}