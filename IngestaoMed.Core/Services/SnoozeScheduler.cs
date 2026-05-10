using IngestaoMed.Core.Interfaces;

namespace IngestaoMed.Core.Services
{
    public class SnoozeScheduler : ISnoozeScheduler
    {
        private readonly Dictionary<int, int> _contadorSoneca = new();
        private const int LimiteMaximo = 3;

        public bool PodeAdiar(int agendamentoId) =>
            ObterTentativas(agendamentoId) < LimiteMaximo;

        public int ObterTentativas(int agendamentoId) =>
            _contadorSoneca.TryGetValue(agendamentoId, out var contagem) ? contagem : 0;

        public bool EhUltimaTentativa(int agendamentoId) =>
            ObterTentativas(agendamentoId) == LimiteMaximo - 1;

        public void RegistrarSoneca(int agendamentoId)
        {
            if (_contadorSoneca.ContainsKey(agendamentoId))
                _contadorSoneca[agendamentoId]++;
            else
                _contadorSoneca[agendamentoId] = 1;
        }

        public void LimparHistorico(int agendamentoId)
        {
            if (_contadorSoneca.ContainsKey(agendamentoId))
                _contadorSoneca.Remove(agendamentoId);
        }
    }
}