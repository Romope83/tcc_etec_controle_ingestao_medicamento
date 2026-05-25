namespace IngestaoMed.Core.Interfaces
{
    public interface ISnoozeService
    {
        Task<DateTime> AgendarSonecaAsync(int agendamentoId, int minutos);

        void CancelarSoneca(int agendamentoId);
    }
}