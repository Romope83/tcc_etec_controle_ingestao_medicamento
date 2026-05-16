using IngestaoMed.Core.Data;
using IngestaoMed.Core.Enums;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;

namespace IngestaoMed.Core.Services
{
    public class LogService : ILogService
    {
        private readonly IDatabaseContext _db;

        public LogService(IDatabaseContext db)
        {
            _db = db;
        }

        public async Task RegistrarAsync(int agendamentoId, TipoEventoLog tipo)
        {
            var agendamento = await _db.BuscarPrimeiroAsync<Agendamento>(a => a.Id == agendamentoId);

            if (agendamento == null) return;

            int minutosAtraso = 0;
            var agora = DateTime.Now;

            if (tipo == TipoEventoLog.ConfirmacaoDireta || tipo == TipoEventoLog.ConfirmacaoComSoneca)
            {
                var diferenca = agora - agendamento.ProximoAlarme;
                minutosAtraso = diferenca.TotalMinutes > 0 ? (int)diferenca.TotalMinutes : 0;
            }

            var log = new LogEvento
            {
                AgendamentoId = agendamentoId,
                Tipo = tipo,
                DataOcorrencia = agora,
                MinutosAtraso = minutosAtraso
            };

            await _db.InserirAsync(log);
        }

        public async Task<List<LogEvento>> ObterLogsPorTratamentoAsync(int tratamentoId)
        {
            var todosLogs = await _db.BuscarTodosAsync<LogEvento>();

            return todosLogs.Where(l => l.Agendamento?.Tratamento?.Id == tratamentoId).ToList();
        }
    }
}