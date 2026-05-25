namespace IngestaoMed.Core.Interfaces
{
    public interface IInteractionLogHelper
    {
        string GerarDescricao(string acao, int agendamentoId);
    }
}