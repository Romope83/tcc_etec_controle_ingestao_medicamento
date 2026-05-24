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
            if (!_connectivity.TemInternet)
                return;

            var pendentes = await _outboxService.ObterPendentesAsync();

            foreach (var email in pendentes)
            {
                bool enviadoComSucesso = await _emailService.EnviarAlertaFalhaAsync(
                    email.Destinatario,
                    email.Assunto,
                    email.Corpo);

                await _outboxService.AtualizarStatusEnvioAsync(email.Id, enviadoComSucesso);
            }
        }

        public async Task IniciarProcessamentoAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_connectivity.TemInternet)
                    {
                        var emailsPendentes = await _outboxService.ObterPendentesAsync();

                        foreach (var email in emailsPendentes)
                        {
                            // 3. Tenta enviar via MailKit/SMTP
                            bool sucesso = await _emailService.EnviarAlertaFalhaAsync(
                                email.Destinatario,
                                "Paciente",
                                email.Corpo
                            );

                            if (sucesso)
                            {
                                // Atualize o status no banco para Enviado = true através do seu service/context
                                email.Enviado = true;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Evita que uma falha de rede ou banco quebre o loop definitivo do background
                }

                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
            }
        }
    }
}