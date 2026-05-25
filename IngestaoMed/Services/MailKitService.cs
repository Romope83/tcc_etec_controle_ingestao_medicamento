using IngestaoMed.Core.Interfaces;
using IngestaoMed.Interfaces;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace IngestaoMed.Services
{
    public class MailKitService : IEmailService
    {
        private readonly IEmailSettings _settings;


        public MailKitService(IEmailSettings settings)
        {
            _settings = settings;
        }


        public async Task<bool> EnviarAlertaFalhaAsync(string destinatario, string assunto, string corpo)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Sistema IngestaoMed",_settings.SenderEmail));
            message.To.Add(new MailboxAddress("Cuidador", destinatario));
            message.Subject = assunto;

            message.Body = new TextPart("plain")
            {
                Text = corpo
            };

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_settings.Host, _settings.Port, MailKit.Security.SecureSocketOptions.None);

                //await client.AuthenticateAsync(_settings.SenderEmail, _settings.SenderPassword);

                await client.SendAsync(message);

                await client.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Falha no envio de e-mail: {ex.Message}");
                return false;
            }
        }
    }
}