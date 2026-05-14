using IngestaoMed.Core.Enums;
using IngestaoMed.Core.Models;

namespace IngestaoMed.Core.Interfaces
{
    public interface ILogService
    {
        Task RegistrarAsync(int agendamentoId, TipoEventoLog tipo);
        Task<List<LogEvento>> ObterLogsPorTratamentoAsync(int tratamentoId);
    }
}