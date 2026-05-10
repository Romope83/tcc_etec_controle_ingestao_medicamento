namespace IngestaoMed.Core.Interfaces
{
    public interface INotificationActionService
    {
        void RegistrarAcoes();
        Task ProcessarAcaoAsync(int actionId, int agendamentoId);
    }
}