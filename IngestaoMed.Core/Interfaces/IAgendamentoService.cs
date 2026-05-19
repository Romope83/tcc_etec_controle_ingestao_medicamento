using IngestaoMed.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IngestaoMed.Core.Interfaces
{
    public interface IAgendamentoService
    {
        Task<MedicamentoTratamento?> ObterVinculoPorIdAsync(int medicamentoTratamentoId);
        Task<Medicamento?> ObterMedicamentoPorIdAsync(int medicamentoId);
        Task<List<Medicamento>> ObterTodosMedicamentosAsync();
        Task<List<Agendamento>> ObterDosesPorVinculoIdAsync(int medicamentoTratamentoId);
        Task<Tratamento?> ObterTratamentoPorIdAsync(int tratamentoId);
        Task<bool> DeletarDosesEAlarmeAsync(List<Agendamento> doses);
        Task<bool> SalvarNovoAgendamentoAsync(int tratamentoId, Medicamento medicamento, string dosagem, int intervaloHoras, bool ativo, List<Agendamento> dosesGeradas);
        Task<bool> AtualizarAgendamentoExistenteAsync(int medicamentoTratamentoId, string dosagem, int intervaloHoras, bool ativo);
        Task SincronizarFilaDeAlarmesAsync();
    }
}