using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Data;

namespace IngestaoMed.Core.Services
{
    public class SnoozeService : ISnoozeService
    {
        private readonly ISnoozeScheduler _snoozeScheduler;
        private readonly IAlarmService _alarmService;
        private readonly IDatabaseContext _database;

        public SnoozeService(
            ISnoozeScheduler snoozeScheduler,
            IAlarmService alarmService,
            IDatabaseContext database)
        {
            _snoozeScheduler = snoozeScheduler;
            _alarmService = alarmService;
            _database = database;
        }

        public async Task<DateTime> AgendarSonecaAsync(int agendamentoId, int minutos)
        {
            if (!_snoozeScheduler.PodeAdiar(agendamentoId))
            {
                throw new InvalidOperationException("Limite de sonecas atingido.");
            }

            // Usando sua interface genérica para buscar o agendamento
            var agendamento = await _database.BuscarPrimeiroAsync<Agendamento>(x => x.Id == agendamentoId);

            if (agendamento == null)
                throw new Exception("Agendamento não encontrado.");

            var novoHorario = DateTime.Now.AddMinutes(minutos);
            agendamento.HorarioProgramado = novoHorario;

            _snoozeScheduler.RegistrarSoneca(agendamentoId);

            // Importante: Atualizar o banco com o novo horário antes de agendar o alarme físico
            await _database.AtualizarAsync(agendamento);

            await _alarmService.AgendarNotificacaoAsync(agendamento);

            return novoHorario;
        }

        public void CancelarSoneca(int agendamentoId)
        {
            // O uso de Fire and Forget aqui depende da sua implementação de extensões
            _alarmService.CancelarAlarmeAsync(agendamentoId);
            _snoozeScheduler.LimparHistorico(agendamentoId);
        }
    }
}