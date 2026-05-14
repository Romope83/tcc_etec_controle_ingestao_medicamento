namespace IngestaoMed.Core.Interfaces
{
    public interface IMonitorFalhaService
    {
        Task VerificarELoggerFalhaAsync(int agendamentoId, int totalSonecas);
    }
}