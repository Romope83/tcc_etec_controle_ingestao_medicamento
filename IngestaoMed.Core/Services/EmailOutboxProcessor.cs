using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;

namespace IngestaoMed.Core.Services
{
    public class EmailOutboxProcessor : IEmailOutboxProcessor
    {
        private readonly IEmailOutboxService _outboxService;
        private readonly IEmailService _emailService;
        private readonly IConnectivityService _connectivity;

        public EmailOutboxProcessor(
            IEmailOutboxService outboxService,
            IEmailService emailService,
            IConnectivityService connectivity)
        {
            _outboxService = outboxService;
            _emailService = emailService;
            _connectivity = connectivity;
        }

        public async Task ProcessarFilaAsync()
        {
            // Se o Mock (ou o Hardware) disser que não tem internet, o Core para aqui.
            if (!_connectivity.TemInternet)
                return;

            var pendentes = await _outboxService.ObterPendentesAsync();

            foreach (var email in pendentes)
            {
                // Tenta o envio físico através do IEmailService
                bool enviadoComSucesso = await _emailService.EnviarAlertaFalhaAsync(
                    email.Destinatario,
                    email.Assunto,
                    email.Corpo);

                // Atualiza o banco via OutboxService (sucesso ou incremento de tentativa)
                await _outboxService.AtualizarStatusEnvioAsync(email.Id, enviadoComSucesso);
            }
        }
    }
}