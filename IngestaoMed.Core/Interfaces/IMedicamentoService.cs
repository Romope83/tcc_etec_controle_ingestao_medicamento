using IngestaoMed.Core.Models;

namespace IngestaoMed.Core.Interfaces
{
    public interface IMedicamentoService
    {
        Task<bool> AdicionarMedicamentoAsync(Medicamento medicamento);

        Task<bool> ExisteMedicamentoAsync(string nome, string forma);
        Task<List<Medicamento>> ObterTodosAsync();

        Task<bool> RemoverMedicamentoAsync(Medicamento medicamento);
    }
}