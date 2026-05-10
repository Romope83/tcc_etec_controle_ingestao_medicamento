using IngestaoMed.Core.Models;

namespace IngestaoMed.Core.Interfaces
{
    public interface IAgendamentoConflitoService
    {
        Task<List<Agendamento>> VerificarConflitosAsync(Tratamento novoTratamento, List<DateTime> novosHorarios);
    }
}