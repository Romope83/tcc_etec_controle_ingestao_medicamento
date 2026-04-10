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
    }
}