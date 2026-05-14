using IngestaoMed.Core.Data;
using IngestaoMed.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IngestaoMed.Core.Services
{
    public class PacienteService : IPacienteService
    {
        private readonly IDatabaseContext _db;

        public PacienteService(IDatabaseContext db) => _db = db;

        public async Task<List<Paciente>> ObterTodosAsync()
        {
            var pacientes = await _db.BuscarTodosAsync<Paciente>();

            foreach (var p in pacientes)
            {
                var ativos = (await _db.BuscarOndeAsync<Tratamento>(t => t.PacienteId == p.Id && t.Ativo)).Count;
                var totais = (await _db.BuscarOndeAsync<Tratamento>(t => t.PacienteId == p.Id)).Count;
                p.ResumoTratamentos = $"Tratamentos {ativos:D2}/{totais:D2}";

                var proximo = await _db.BuscarPrimeiroAsync<Agendamento>(a =>
                    a.TratamentoId > 0 && 
                    a.Status == "Pendente" &&
                    a.HorarioProgramado >= DateTime.Now);

                if (proximo != null)
                {
                    p.ProximaData = proximo.HorarioProgramado.ToString("dd.MM.yyyy");
                    p.ProximoHorario = proximo.HorarioProgramado.ToString("HH:mm");
                }
            }
            return pacientes;
        }

        public async Task<bool> SalvarPacienteAsync(Paciente paciente)
        {
            return await _db.InserirAsync(paciente);
        }

        public async Task<bool> RemoverPacienteAsync(Paciente paciente) =>
            await _db.ExcluirAsync(paciente) > 0;

        public async Task<List<Paciente>> BuscarOndeAsync(Expression<Func<Paciente, bool>> predicado)
        {
            return await _db.BuscarOndeAsync<Paciente>(predicado);
        }
        public async Task<List<Paciente>> BuscarPacientesIdososAsync()
        {
            var dataLimite = DateTime.Today.AddYears(-70);

            return await _db.BuscarOndeAsync<Paciente>(p => p.DataNascimento <= dataLimite);
        }
        public async Task<List<Paciente>> BuscarComTratamentoAtivoAsync()
        {
            var pacientesComTratamento = await _db.BuscarOndeAsync<Tratamento>(t => t.Ativo);

            var idsPacientes = pacientesComTratamento
                .Select(t => t.PacienteId)
                .Distinct()
                .ToList();

            return await _db.BuscarOndeAsync<Paciente>(p => idsPacientes.Contains(p.Id));
        }

        public async Task<Paciente?> ObterDetalhesCompletosAsync(int pacienteId)
        {
            var paciente = await _db.BuscarPrimeiroAsync<Paciente>(p => p.Id == pacienteId);

            if (paciente == null) return null;

            var tratamentos = await _db.BuscarOndeAsync<Tratamento>(t => t.PacienteId == pacienteId);

            foreach (var t in tratamentos)
            {
                t.Remedios = await _db.BuscarOndeAsync<MedicamentoTratamento>(m => m.TratamentoId == t.Id);

                foreach (var r in t.Remedios)
                {
                    var med = await _db.BuscarPrimeiroAsync<Medicamento>(x => x.Id == r.MedicamentoId);
                    r.NomeMedicamento = med?.NomeComercial;
                }
            }

            paciente.Tratamentos = tratamentos;

            return paciente;
        }
    }
}
