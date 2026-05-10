using IngestaoMed.Core.Models;
using IngestaoMed.Core.Enums;
using IngestaoMed.Core.Interfaces;

namespace IngestaoMed.Core.Services
{
    public class CalculadoraAdesaoService : ICalculadoraAdesao
    {
        public double Calcular(IEnumerable<LogEvento> logs)
        {
            if (logs == null || !logs.Any())
                return 0;

            double pontuacaoTotal = 0;
            int totalLogsValidos = 0;

            foreach (var log in logs)
            {
                // Apenas logs de confirmação ou atraso crítico entram no cálculo de adesão
                if (log.Tipo == TipoEventoLog.ConfirmacaoDireta ||
                    log.Tipo == TipoEventoLog.ConfirmacaoComSoneca ||
                    log.Tipo == TipoEventoLog.AtrasoCritico)
                {
                    totalLogsValidos++;
                    pontuacaoTotal += CalcularPontosPorEvento(log);
                }
            }

            if (totalLogsValidos == 0) return 0;

            return (pontuacaoTotal / totalLogsValidos) * 100;
        }

        private double CalcularPontosPorEvento(LogEvento log)
        {
            return log.Tipo switch
            {
                // Tomou no horário (margem de 15 min): 100%
                TipoEventoLog.ConfirmacaoDireta when log.MinutosAtraso <= 15 => 1.0,

                // Tomou com atraso leve ou via soneca: 70%
                TipoEventoLog.ConfirmacaoDireta => 0.7,
                TipoEventoLog.ConfirmacaoComSoneca => 0.7,

                // Atraso crítico ou ignorado: 0% a 30%
                TipoEventoLog.AtrasoCritico => 0.2,

                _ => 0
            };
        }
    }
}