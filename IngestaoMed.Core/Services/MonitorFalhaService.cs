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
            var config = await _db.BuscarPrimeiroAsync<ConfiguracaoCuidador>();
            if (config == null || !config.AlertaAtivado) return;

            if (totalSonecas >= config.LimiteSonecasParaAlerta)
            {
                var agendamento = await _db.BuscarPrimeiroAsync<Agendamento>(a => a.Id == agendamentoId);
                if (agendamento == null) return;

                var vinculo = await _db.BuscarPrimeiroAsync<MedicamentoTratamento>(m => m.Id == agendamento.MedicamentoTratamentoId);
                if (vinculo == null) return;

                var medicamento = await _db.BuscarPrimeiroAsync<Medicamento>(m => m.Id == vinculo.MedicamentoId);

                string nomeRemedio = medicamento?.NomeComercial ?? "Medicamento não identificado";

                var emailParaFila = new EmailFila
                {
                    Destinatario = config.EmailCuidador,
                    Assunto = $"⚠️ ALERTA: Falha no medicamento {nomeRemedio}",
                    Corpo = $@"Olá {config.NomeCuidador},

O paciente não confirmou a ingestão do seguinte remédio:
Medicamento: {nomeRemedio}
Dosagem: {vinculo.Dosagem}
Instruções: {vinculo.Instrucoes}

Este remédio faz parte do tratamento: {agendamento.Tratamento?.Nome ?? "N/A"}.

O limite de {totalSonecas} sonecas foi atingido. Por favor, verifique o paciente.",
                    DataCriacao = DateTime.Now,
                    Enviado = false
                };

                await _db.InserirAsync(emailParaFila);
            }
        }
    }
}