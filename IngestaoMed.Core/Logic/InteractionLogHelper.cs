using IngestaoMed.Core.Interfaces;

namespace IngestaoMed.Core.Logic
{
    public class InteractionLogHelper : IInteractionLogHelper
    {
        public string GerarDescricao(string acao, int agendamentoId)
        {
            return $"Ação: {acao} | Agendamento: {agendamentoId} | Horário: {DateTime.Now:HH:mm}";
        }
    }
}