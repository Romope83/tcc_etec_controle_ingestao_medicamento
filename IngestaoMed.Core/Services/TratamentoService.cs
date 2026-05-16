using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IngestaoMed.Core.Services
{
    public class TratamentoService : ITratamentoService
    {
        private readonly IDatabaseContext _db;

        public TratamentoService(IDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Tratamento?> ObterPorIdAsync(int id)
        {
            return await _db.BuscarPrimeiroAsync<Tratamento>(x => x.Id == id);
        }

        public async Task<List<MedicamentoTratamento>> ObterMedicamentosVinculadosAsync(int tratamentoId)
        {
            var listaRemedios = await _db.BuscarOndeAsync<MedicamentoTratamento>(m => m.TratamentoId == tratamentoId)
                ?? new List<MedicamentoTratamento>();

            foreach (var r in listaRemedios)
            {
                var med = await _db.BuscarPrimeiroAsync<Medicamento>(x => x.Id == r.MedicamentoId);
                r.NomeMedicamento = med?.NomeComercial;
            }

            return listaRemedios;
        }

        public async Task<(Tratamento? Tratamento, List<MedicamentoTratamento> Medicamentos)> ObterTratamentoComMedicamentosAsync(int id)
        {
            var t = await ObterPorIdAsync(id);
            var meds = new List<MedicamentoTratamento>();
            if (t != null)
            {
                meds = await ObterMedicamentosVinculadosAsync(id);
            }
            return (t, meds);
        }

        public async Task<int> InserirTratamentoAsync(Tratamento tratamento)
        {
            await _db.InserirAsync(tratamento);
            return tratamento.Id;
        }

        public async Task<int> AtualizarTratamentoAsync(Tratamento tratamento)
        {
            return await _db.AtualizarAsync(tratamento);
        }

        public async Task<int> ExcluirTratamentoAsync(Tratamento tratamento)
        {
            return await _db.ExcluirAsync(tratamento);
        }

        public async Task<int> ExcluirMedicamentoTratamentoAsync(MedicamentoTratamento vinculo)
        {
            return await _db.ExcluirAsync(vinculo);
        }
    }
}