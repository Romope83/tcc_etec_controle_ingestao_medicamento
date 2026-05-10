using IngestaoMed.Core.Models;

namespace IngestaoMed.Core.Interfaces
{
    public interface IEmailOutboxService
    {
        Task<List<EmailFila>> ObterPendentesAsync();

        Task AtualizarStatusEnvioAsync(int emailId, bool sucesso);


        Task LimparFilaAntigaAsync(int diasRetencao = 7);
    }
}