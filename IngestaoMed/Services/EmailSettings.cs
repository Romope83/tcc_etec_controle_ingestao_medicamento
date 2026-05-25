using IngestaoMed.Interfaces;

namespace IngestaoMed.Services
{
    public class EmailSettings : IEmailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string SenderEmail { get; set; }
        public string SenderPassword { get; set; }
    }
}