namespace IngestaoMed.Core.Interfaces
{

    public interface IEmailOutboxProcessor
    {
        Task ProcessarFilaAsync();
        Task IniciarProcessamentoAsync(CancellationToken cancellationToken);
    }
}