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

                var tratamentosDoPaciente = await _db.BuscarOndeAsync<Tratamento>(t => t.PacienteId == p.Id);
                var idsTratamentos = tratamentosDoPaciente.Select(t => t.Id).ToList();

                if (idsTratamentos.Any())
                {
                    var medicamentosVinculados = await _db.BuscarOndeAsync<MedicamentoTratamento>(mt =>
                        idsTratamentos.Contains(mt.TratamentoId));

                    var idsVinculos = medicamentosVinculados.Select(mt => mt.Id).ToList();

                    if (idsVinculos.Any())
                    {
                        var proximo = await _db.BuscarPrimeiroAsync<Agendamento>(a =>
                            idsVinculos.Contains(a.MedicamentoTratamentoId) &&
                            a.Status == "Pendente" &&
                            a.ProximoAlarme >= DateTime.Now);

                        if (proximo != null)
                        {
                            p.ProximaData = proximo.HorarioOriginal.ToString("dd.MM.yyyy");
                            p.ProximoHorario = proximo.HorarioOriginal.ToString("HH:mm");
                        }
                    }
                }
            }
            return pacientes;
        }

        public async Task<bool> SalvarOuAtualizarPacienteAsync(Paciente paciente)
        {
            if (paciente.Id > 0)
            {
                return await _db.AtualizarAsync(paciente) > 0;
            }
            else
            {
                return await _db.InserirAsync(paciente);
            }
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
                var remedios = await _db.BuscarOndeAsync<MedicamentoTratamento>(m => m.TratamentoId == t.Id);
                t.QtdRemedios = remedios.Count;

                int totalDoses = 0;
                int dosesTomadas = 0;

                foreach (var r in remedios)
                {
                    var agendamentos = await _db.BuscarOndeAsync<Agendamento>(a => a.MedicamentoTratamentoId == r.Id);
                    totalDoses += agendamentos.Count;
                    dosesTomadas += agendamentos.Count(a => a.Status == "Tomado");
                }

                t.TotalDosesTratamento = totalDoses;
                t.DosesTomadasTratamento = dosesTomadas;
            }

            paciente.Tratamentos = tratamentos;
            return paciente;
        }
        public async Task<Paciente?> BuscarPacientePorIdAsync(int pacienteId)
        {
            return await _db.BuscarPrimeiroAsync<Paciente>(x => x.Id == pacienteId);
        }
    }
}
