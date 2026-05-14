namespace IngestaoMed.Core.Interfaces
{
    public interface IEmailService
    {
        Task<bool> EnviarAlertaFalhaAsync(string destinatario, string nomePaciente, string medicamento);
    }
}