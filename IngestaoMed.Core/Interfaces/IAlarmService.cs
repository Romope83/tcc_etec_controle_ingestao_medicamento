using IngestaoMed.Core.Models;

namespace IngestaoMed.Core.Interfaces
{
    public interface IAlarmService
    {
        Task AgendarNotificacaoAsync(Agendamento agendamento);
        Task CancelarAlarmeAsync(int agendamentoId);
    }
}