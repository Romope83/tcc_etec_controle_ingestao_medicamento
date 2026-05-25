using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;

namespace IngestaoMed.Core.Services
{
    public class MonitorFalhaService : IMonitorFalhaService
    {
        private readonly IDatabaseContext _db;

        public MonitorFalhaService(IDatabaseContext db)
        {
            _db = db;
        }

        public async Task VerificarELoggerFalhaAsync(int agendamentoId, int totalSonecas)
        {
            var cuidador = await _db.BuscarPrimeiroAsync<Cuidador>();
            if (cuidador == null) return;

            if (totalSonecas >= cuidador.LimiteSonecasParaAlerta)
            {
                var agendamento = await _db.BuscarPrimeiroAsync<Agendamento>(a => a.Id == agendamentoId);
                if (agendamento == null) return;

                var vinculo = await _db.BuscarPrimeiroAsync<MedicamentoTratamento>(m => m.Id == agendamento.MedicamentoTratamentoId);
                if (vinculo == null) return;

                var tratamento = await _db.BuscarPrimeiroAsync<Tratamento>(t => t.Id == vinculo.TratamentoId);
                if (tratamento == null) return;

                var paciente = await _db.BuscarPrimeiroAsync<Paciente>(p=>p.Id == tratamento.PacienteId);
                if (paciente == null) return;

                var medicamento = await _db.BuscarPrimeiroAsync<Medicamento>(m => m.Id == vinculo.MedicamentoId);

                string nomeRemedio = medicamento?.NomeComercial ?? "Medicamento não identificado";

                var emailParaFila = new EmailFila
                {
                    Destinatario = cuidador.Email,
                    Assunto = $"⚠️ ALERTA: Falha no medicamento {nomeRemedio}",
                    Corpo = $@"Olá {cuidador.Nome},

O paciente {paciente.Nome}, nascido em {paciente.DataNascimento:dd/MM/yyyy}, não confirmou a ingestão do medicamento agendado para as {agendamento.HorarioOriginal:HH:mm}.
Medicamento: {nomeRemedio}
Dosagem: {vinculo.Dosagem}
Instruções: {vinculo.Instrucoes}

Este remédio faz parte do tratamento: {tratamento.Nome ?? "N/A"}.

O limite de {totalSonecas} sonecas foi atingido. Por favor, verifique o paciente.",
                    DataCriacao = DateTime.Now,
                    Enviado = false
                };

                await _db.InserirAsync(emailParaFila);
                agendamento.Status = "Perdido";
            }
        }
    }
}