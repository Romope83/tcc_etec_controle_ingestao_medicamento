namespace IngestaoMed.Interfaces
{
    public interface IEmailSettings
    {
        string Host { get; }
        int Port { get; }
        string SenderEmail { get; }
        string SenderPassword { get; }
    }
}