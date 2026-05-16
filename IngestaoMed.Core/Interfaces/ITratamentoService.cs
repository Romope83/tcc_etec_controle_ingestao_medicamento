using IngestaoMed.Core.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IngestaoMed.Core.Interfaces
{
    public interface ITratamentoService
    {
        Task<Tratamento?> ObterPorIdAsync(int id);
        Task<List<MedicamentoTratamento>> ObterMedicamentosVinculadosAsync(int tratamentoId);
        Task<(Tratamento? Tratamento, List<MedicamentoTratamento> Medicamentos)> ObterTratamentoComMedicamentosAsync(int id);

        Task<int> InserirTratamentoAsync(Tratamento tratamento);
        Task<int> AtualizarTratamentoAsync(Tratamento tratamento);
        Task<int> ExcluirTratamentoAsync(Tratamento tratamento);

        Task<int> ExcluirMedicamentoTratamentoAsync(MedicamentoTratamento vinculo);
        Task<List<MedicamentoTratamento>> ObterOndeMedicamentoVinculadoAsync(Expression<Func<MedicamentoTratamento, bool>> predicado);
    }
}