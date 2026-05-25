namespace IngestaoMed.Core.Interfaces
{
    public interface ITimerService
    {
        void Iniciar(TimeSpan intervalo, Action callback);
        void Parar();
    }
}