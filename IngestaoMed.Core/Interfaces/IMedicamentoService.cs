using IngestaoMed.Core.Models;

namespace IngestaoMed.Core.Interfaces
{
    public interface IMedicamentoService
    {
        Task<bool> AdicionarMedicamentoAsync(Medicamento medicamento);
        // Futuramente: BuscarTodosAsync, AtualizarAsync, etc.
    }
}