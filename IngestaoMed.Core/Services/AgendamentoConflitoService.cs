using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;

namespace IngestaoMed.Core.Services
{
    public class AgendamentoConflitoService : IAgendamentoConflitoService
    {
        private readonly IDatabaseContext _db;

        public AgendamentoConflitoService(IDatabaseContext db)
        {
            _db = db;
        }

        public async Task<List<Agendamento>> VerificarConflitosAsync(Tratamento novoTratamento, List<DateTime> novosHorarios)
        {
            var agendamentosExistentes = await _db.BuscarTodosAsync<Agendamento>();

            var conflitos = agendamentosExistentes
                .Where(a => a.Status == "Pendente" &&
                            novosHorarios.Any(h => Math.Abs((h - a.HorarioProgramado).TotalMinutes) < 30))
                .ToList();

            return conflitos;
        }
    }
}