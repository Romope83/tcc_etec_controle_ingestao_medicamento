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
            // 1. Busca configuração do cuidador
            var config = await _db.BuscarPrimeiroAsync<ConfiguracaoCuidador>();
            if (config == null || !config.AlertaAtivado) return;

            // 2. Verifica se atingiu o limite crítico de sonecas
            if (totalSonecas >= config.LimiteSonecasParaAlerta)
            {
                // 3. Busca o agendamento
                var agendamento = await _db.BuscarPrimeiroAsync<Agendamento>(a => a.Id == agendamentoId);
                if (agendamento == null) return;

                // 4. Busca o tratamento para identificar o que falhou
                var tratamento = await _db.BuscarPrimeiroAsync<Tratamento>(t => t.Id == agendamento.TratamentoId);

                // Prioriza o Nome do Tratamento, se vazio usa a propriedade de apoio ou genérico
                string identificadorMed = !string.IsNullOrEmpty(tratamento?.Nome)
                    ? tratamento.Nome
                    : (tratamento?.NomeMedicamento ?? "Medicamento");

                // 5. Enfileira o e-mail na "Outbox" para envio via internet
                var emailParaFila = new EmailFila
                {
                    Destinatario = config.EmailCuidador,
                    Assunto = "⚠️ ALERTA DE SAÚDE: Falha na Medicação",
                    Corpo = $@"Olá {config.NomeCuidador},

O sistema IngestaoMed detectou uma falha recorrente.
O paciente não confirmou a ingestão do medicamento referente ao tratamento: {identificadorMed}.
O limite de {totalSonecas} sonecas foi atingido e o medicamento ainda consta como pendente.

Por favor, verifique o estado do paciente.",
                    DataCriacao = DateTime.Now,
                    Enviado = false,
                    Tentativas = 0
                };

                await _db.InserirAsync(emailParaFila);
            }
        }
    }
}