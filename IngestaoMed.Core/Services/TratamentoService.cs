using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
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
            var remedios = await _db.BuscarOndeAsync<MedicamentoTratamento>(m => m.TratamentoId == tratamentoId);

            foreach (var r in remedios)
            {
                var med = await _db.BuscarPrimeiroAsync<Medicamento>(x => x.Id == r.MedicamentoId);
                r.NomeMedicamento = med?.NomeComercial;
                r.FotoPath = med?.FotoPath;
                r.FormaIngestao = med?.FormaIngestao;
                r.UnidadeDosagem = med?.UnidadeDosagem;


                var agendamentos = await _db.BuscarOndeAsync<Agendamento>(a => a.MedicamentoTratamentoId == r.Id);
                if (agendamentos.Any())
                {
                    r.DataPrimeiraDose = agendamentos.Min(a => a.HorarioOriginal);
                    r.DataUltimaDose = agendamentos.Max(a => a.HorarioOriginal);
                }
            }

            return remedios;
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
            var vinculos = await _db.BuscarOndeAsync<MedicamentoTratamento>(v => v.TratamentoId == tratamento.Id);

            foreach (var vinculo in vinculos)
            {
                await ExcluirMedicamentoTratamentoAsync(vinculo);
            }
            return await _db.ExcluirAsync(tratamento);
        }

        public async Task<int> ExcluirMedicamentoTratamentoAsync(MedicamentoTratamento vinculo)
        {
            var agendamentos = await _db.BuscarOndeAsync<Agendamento>(a => a.MedicamentoTratamentoId == vinculo.Id);

            foreach (var agendamento in agendamentos)
            {
                await _db.ExcluirAsync(agendamento);
            }

            return await _db.ExcluirAsync(vinculo);
        }
        public async Task<List<MedicamentoTratamento>> ObterOndeMedicamentoVinculadoAsync(Expression<Func<MedicamentoTratamento, bool>> predicado)
        {
            return await _db.BuscarOndeAsync(predicado);
        }

        public async Task<List<Tratamento>> ObterTodosAsync()
        {
            return await _db.BuscarTodosAsync<Tratamento>();
        }

    }
}