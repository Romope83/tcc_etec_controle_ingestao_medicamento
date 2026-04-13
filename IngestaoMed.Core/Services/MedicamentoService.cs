using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;

namespace IngestaoMed.Core.Services
{
    public class MedicamentoService : IMedicamentoService
    {
        private readonly IDatabaseContext _db;
        public MedicamentoService(IDatabaseContext db)
        {
            _db = db;
        }

        public async Task<bool> AdicionarMedicamentoAsync(Medicamento medicamento)
        {
            return await _db.InserirAsync(medicamento);
        }

        public async Task<bool> ExisteMedicamentoAsync(string nome, string forma)
        {
            // Usamos ToLower() para evitar que "Dipirona" e "dipirona" sejam cadastrados duas vezes
            var existente = await _db.BuscarPrimeiroAsync<Medicamento>(m =>
                m.NomeComercial.ToLower() == nome.ToLower() &&
                m.FormaIngestao == forma); // Forma vem do Picker, então a grafia é exata

            return existente != null;
        }

        public async Task<List<Medicamento>> ObterTodosAsync()
        {
            return await _db.BuscarTodosAsync<Medicamento>();
        }
    }
}