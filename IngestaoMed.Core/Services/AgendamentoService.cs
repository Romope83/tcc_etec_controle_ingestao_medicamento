using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IngestaoMed.Core.Services
{
    public class AgendamentoService : IAgendamentoService
    {
        private readonly IDatabaseContext _db;
        private readonly IAlarmService _alarmService;

        public AgendamentoService(IDatabaseContext db, IAlarmService alarmService)
        {
            _db = db;
            _alarmService = alarmService;
        }

        public async Task<MedicamentoTratamento?> ObterVinculoPorIdAsync(int medicamentoTratamentoId)
        {
            return await _db.BuscarPrimeiroAsync<MedicamentoTratamento>(mt => mt.Id == medicamentoTratamentoId);
        }

        public async Task<Medicamento?> ObterMedicamentoPorIdAsync(int medicamentoId)
        {
            return await _db.BuscarPrimeiroAsync<Medicamento>(m => m.Id == medicamentoId);
        }

        public async Task<List<Medicamento>> ObterTodosMedicamentosAsync()
        {
            return await _db.BuscarTodosAsync<Medicamento>();
        }

        public async Task<List<Agendamento>> ObterDosesPorVinculoIdAsync(int medicamentoTratamentoId)
        {
            var doses = await _db.BuscarOndeAsync<Agendamento>(a => a.MedicamentoTratamentoId == medicamentoTratamentoId);
            return doses ?? new List<Agendamento>();
        }

        public async Task<Tratamento?> ObterTratamentoPorIdAsync(int tratamentoId)
        {
            return await _db.BuscarPrimeiroAsync<Tratamento>(t => t.Id == tratamentoId);
        }

        public async Task<bool> DeletarDosesEAlarmeAsync(List<Agendamento> doses)
        {
            try
            {
                foreach (var dose in doses)
                {
                    if (dose.Id > 0) await _db.ExcluirAsync(dose);
                }

                await SincronizarFilaDeAlarmesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> SalvarNovoAgendamentoAsync(int tratamentoId, Medicamento medicamento, string dosagem, int intervaloHoras, bool ativo, List<Agendamento> dosesGeradas)
        {
            try
            {
                var mt = new MedicamentoTratamento
                {
                    TratamentoId = tratamentoId,
                    MedicamentoId = medicamento.Id,
                    Dosagem = dosagem,
                    IntervaloHoras = intervaloHoras,
                    Ativo = ativo
                };

                await _db.InserirAsync(mt);

                foreach (var dose in dosesGeradas)
                {
                    dose.MedicamentoTratamentoId = mt.Id;
                    await _db.InserirAsync(dose);
                }

                await SincronizarFilaDeAlarmesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> AtualizarAgendamentoExistenteAsync(int medicamentoTratamentoId, string dosagem, int intervaloHoras, bool ativo)
        {
            try
            {
                var mt = await _db.BuscarPrimeiroAsync<MedicamentoTratamento>(x => x.Id == medicamentoTratamentoId);
                if (mt != null)
                {
                    mt.Dosagem = dosagem;
                    mt.IntervaloHoras = intervaloHoras;
                    mt.Ativo = ativo;
                    await _db.AtualizarAsync(mt);

                    await SincronizarFilaDeAlarmesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task SincronizarFilaDeAlarmesAsync()
        {
            var agora = DateTime.Now;
            var dosesPendentes = await _db.BuscarOndeAsync<Agendamento>(a =>
                a.Status == "Pendente" &&
                a.ProximoAlarme >= agora);

            if (dosesPendentes == null) return;

            var as40Primeiras = dosesPendentes
                .OrderBy(a => a.ProximoAlarme)
                .Take(40)
                .ToList();

            await _alarmService.SincronizarJanelaAlarmesAsync(as40Primeiras);
        }
    }
}