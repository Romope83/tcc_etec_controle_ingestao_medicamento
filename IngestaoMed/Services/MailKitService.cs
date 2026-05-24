using IngestaoMed.Core.Interfaces;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace IngestaoMed.Services
{
    public class MailKitService : IEmailService
    {
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderEmail = "seu-email@gmail.com";
        private const string SenderPassword = "sua-senha-de-app"; 

        public async Task<bool> EnviarAlertaFalhaAsync(string destinatario, string assunto, string corpo)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Sistema IngestaoMed", SenderEmail));
            message.To.Add(new MailboxAddress("Cuidador", destinatario));
            message.Subject = assunto;

            message.Body = new TextPart("plain")
            {
                Text = corpo
            };

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(SmtpServer, SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);

                await client.AuthenticateAsync(SenderEmail, SenderPassword);

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