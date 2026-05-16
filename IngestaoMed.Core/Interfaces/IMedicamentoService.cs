using IngestaoMed.Core.Models;
using System.Linq.Expressions;

namespace IngestaoMed.Core.Interfaces
{
    public interface IMedicamentoService
    {
        Task<bool> AdicionarMedicamentoAsync(Medicamento medicamento);

        Task<bool> ExisteMedicamentoAsync(string nome, string forma);
        Task<List<Medicamento>> ObterTodosAsync();

        Task<bool> RemoverMedicamentoAsync(Medicamento medicamento);
        Task<Medicamento?> BuscarPrimeiroMedicamentoAsync(Expression<Func<Medicamento, bool>> predicado);
    }
}