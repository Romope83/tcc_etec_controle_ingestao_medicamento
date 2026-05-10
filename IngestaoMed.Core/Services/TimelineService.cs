using IngestaoMed.Core.DTOs;
using IngestaoMed.Core.Data;
using IngestaoMed.Core.Models;

namespace IngestaoMed.Core.Services
{
    public class TimelineService
    {
        private readonly IDatabaseContext _db;

        public TimelineService(IDatabaseContext db)
        {
            _db = db;
        }

        public async Task<List<TimelineItem>> ObterHistoricoCompletoAsync()
        {
            var logs = await _db.BuscarTodosAsync<LogMedicamento>();
            var fotos = await _db.BuscarTodosAsync<AnexoMedia>();

            // Transforma Logs em TimelineItems
            var itensLog = logs.Select(l => new TimelineItem
            {
                Id = l.Id,
                Data = l.DataHora,
                Titulo = l.Mensagem,
                Tipo = "LOG"
            });

            // Transforma Fotos em TimelineItems
            var itensFoto = fotos.Select(f => new TimelineItem
            {
                Id = f.Id,
                Data = f.DataCriacao,
                Titulo = "Anexo de Mídia",
                CaminhoImagem = f.CaminhoLocal,
                Tipo = "FOTO"
            });

            // Une tudo, ordena pela data decrescente (mais recente primeiro) e converte para lista
            return itensLog.Concat(itensFoto)
                           .OrderByDescending(x => x.Data)
                           .ToList();
        }
    }
}