using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IngestaoMed.Core.Services
{
    public class AgendamentoConflitoService : IAgendamentoConflitoService
    {
        private readonly IDatabaseContext _db;

        public AgendamentoConflitoService(IDatabaseContext db)
        {
            _db = db;
        }

        public async Task<List<Agendamento>> VerificarConflitosAsync(Tratamento novoTratamento, List<DateTime> novosHorarios)
        {
            if (novoTratamento == null || novosHorarios == null || !novosHorarios.Any())
                return new List<Agendamento>();

            var tratamentosPaciente = await _db.BuscarOndeAsync<Tratamento>(t => t.PacienteId == novoTratamento.PacienteId);
            if (!tratamentosPaciente.Any())
                return new List<Agendamento>();

            var tratamentoIds = tratamentosPaciente.Select(t => t.Id).ToList();

            // 2. Busca todos os vínculos de medicamentos associados a esses tratamentos
            var vinculosExistentes = await _db.BuscarOndeAsync<MedicamentoTratamento>(mt => tratamentoIds.Contains(mt.TratamentoId));
            if (!vinculosExistentes.Any())
                return new List<Agendamento>();

            var vinculoIds = vinculosExistentes.Select(v => v.Id).ToList();

            // 3. Busca apenas os agendamentos pendentes vinculados a este paciente
            var agendamentosExistentes = await _db.BuscarOndeAsync<Agendamento>(a =>
                a.Status == "Pendente" && vinculoIds.Contains(a.MedicamentoTratamentoId));

            // 4. Filtra em memória os horários que entram em conflito com a margem de 30 minutos
            var conflitos = agendamentosExistentes
                .Where(a => novosHorarios.Any(h => Math.Abs((h - a.ProximoAlarme).TotalMinutes) < 30))
                .ToList();

            return conflitos;
        }

        public async Task<bool> VerificarDuplicidadeMedicamentoEmAndamentoAsync(int pacienteId, int medicamentoId, int medicamentoTratamentoIdAtual)
        {
            // 1. Busca todos os tratamentos do paciente
            var tratamentos = await _db.BuscarOndeAsync<Tratamento>(t => t.PacienteId == pacienteId);
            if (!tratamentos.Any()) return false;

            var tratamentoIds = tratamentos.Select(t => t.Id).ToList();

            // 2. Busca vínculos ativos do mesmo medicamento nesses tratamentos (desconsiderando o próprio registro em caso de edição)
            var vinculos = await _db.BuscarOndeAsync<MedicamentoTratamento>(mt =>
                tratamentoIds.Contains(mt.TratamentoId) &&
                mt.MedicamentoId == medicamentoId &&
                mt.Ativo &&
                mt.Id != medicamentoTratamentoIdAtual);

            if (!vinculos.Any()) return false;

            var vinculoIds = vinculos.Select(v => v.Id).ToList();

            // 3. Verifica se esses vínculos possuem alguma dose com status "Pendente"
            var dosesPendentes = await _db.BuscarOndeAsync<Agendamento>(a =>
                vinculoIds.Contains(a.MedicamentoTratamentoId) && a.Status == "Pendente");

            return dosesPendentes.Any();
        }
    }
}
