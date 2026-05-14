namespace IngestaoMed.Core.Interfaces
{
    public interface ISnoozeScheduler
    {
        bool PodeAdiar(int agendamentoId);
        void RegistrarSoneca(int agendamentoId);
        void LimparHistorico(int agendamentoId);
        int ObterTentativas(int agendamentoId);
        bool EhUltimaTentativa(int agendamentoId);
    }
}