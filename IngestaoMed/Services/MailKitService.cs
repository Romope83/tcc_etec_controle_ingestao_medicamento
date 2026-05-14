using IngestaoMed.Core.Interfaces;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace IngestaoMed.Services
{
    public class MailKitService : IEmailService
    {
        // No TCC, você pode carregar isso de um arquivo de configuração ou constantes
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderEmail = "seu-email@gmail.com";
        private const string SenderPassword = "sua-senha-de-app"; // Senha de App, não a senha real

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
                // Conecta ao servidor (SSL/TLS)
                await client.ConnectAsync(SmtpServer, SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);

                // Autentica
                await client.AuthenticateAsync(SenderEmail, SenderPassword);

                // Envia
                await client.SendAsync(message);

                await client.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                // No TCC, logar o erro é fundamental para demonstrar tratamento de exceções
                System.Diagnostics.Debug.WriteLine($"Falha no envio de e-mail: {ex.Message}");
                return false;
            }
        }
    }
}