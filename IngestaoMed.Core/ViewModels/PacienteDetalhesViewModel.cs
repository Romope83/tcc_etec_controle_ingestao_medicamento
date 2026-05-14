using CommunityToolkit.Mvvm.ComponentModel;
using IngestaoMed.Core.Data;
using IngestaoMed.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngestaoMed.Core.ViewModels
{
    public partial class PacienteDetalhesViewModel: ObservableObject
    {
        private readonly IDatabaseContext _db;
        private readonly IPacienteService _pacienteService;

        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private Paciente? _pacienteSelecionado;
        public PacienteDetalhesViewModel(IDatabaseContext db, IPacienteService pacienteService)
        {
            _db = db;
            _pacienteService = pacienteService;
        }
        public ObservableCollection<Tratamento> Tratamentos { get; } = new();

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
                foreach (var t in PacienteSelecionado!.Tratamentos)
                {
                    Tratamentos.Add(t);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar detalhes: {ex.Message}");
            }
        }
        public async Task CarregarDetalhesPacienteAsync(int pacienteId)
        {
            await _pacienteService.ObterDetalhesCompletosAsync(pacienteId);

        }
    }
}
