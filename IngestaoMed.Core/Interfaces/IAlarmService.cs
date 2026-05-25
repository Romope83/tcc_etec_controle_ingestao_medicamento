using IngestaoMed.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IngestaoMed.Core.Interfaces
{
    public interface IAlarmService
    {
        Task AgendarNotificacaoAsync(Agendamento agendamento);
        Task CancelarAlarmeAsync(int agendamentoId);
        Task SincronizarJanelaAlarmesAsync(List<Agendamento> proximosAlarmes); // Recebe os dados prontos
    }
}