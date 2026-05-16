using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Linq.Expressions;

namespace IngestaoMed.Core.Services
{
    public class MedicamentoService : IMedicamentoService
    {
        private readonly IDatabaseContext _db;

        public MedicamentoService(IDatabaseContext db)
        {
            _db = db;
        }

        public async Task<bool> AdicionarOuAtualizarMedicamentoAsync(Medicamento medicamento)
        {
            if (medicamento.Id == 0)
            {
                return await _db.InserirAsync(medicamento);
            }
            else
            {
                return await _db.AtualizarAsync(medicamento) > 0;
            }
        }

        public async Task<bool> ExisteMedicamentoAsync(string nome, string forma)
        {
            var existente = await _db.BuscarPrimeiroAsync<Medicamento>(m =>
                m.NomeComercial.ToLower() == nome.ToLower() &&
                m.FormaIngestao == forma);

            return existente != null;
        }

        public async Task<List<Medicamento>> ObterTodosAsync()
        {
            return await _db.BuscarTodosAsync<Medicamento>();
        }

        public async Task<bool> RemoverMedicamentoAsync(Medicamento medicamento)
        {
            var resultado = await _db.ExcluirAsync(medicamento);
            return resultado > 0;
        }

        public async Task<Medicamento?> BuscarPrimeiroMedicamentoAsync(Expression<Func<Medicamento, bool>> predicado)
        {
            return await _db.BuscarPrimeiroAsync(predicado);
        }
    }
}