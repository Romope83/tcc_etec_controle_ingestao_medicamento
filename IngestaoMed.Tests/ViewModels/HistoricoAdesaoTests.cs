using Xunit;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Enums;
using System.Collections.Generic;

namespace IngestaoMed.Tests.Business
{
    public class HistoricoAdesaoTests
    {
        private readonly CalculadoraAdesaoService _calculadoraService;

        public HistoricoAdesaoTests()
        {
            // Como não é mais estática, instanciamos para testar
            _calculadoraService = new CalculadoraAdesaoService();
        }

        [Fact]
        public void CalcularPercentual_DeveRetornarCem_QuandoTodasDosesRealizadasPontualmente()
        {
            // Arrange - Mudamos de Agendamento para LogEvento
            var logs = new List<LogEvento>
            {
                new LogEvento { Tipo = TipoEventoLog.ConfirmacaoDireta, MinutosAtraso = 0 },
                new LogEvento { Tipo = TipoEventoLog.ConfirmacaoDireta, MinutosAtraso = 10 }
            };

            // Act
            double resultado = _calculadoraService.Calcular(logs);

            // Assert
            Assert.Equal(100, resultado);
        }

        [Fact]
        public void CalcularPercentual_DeveRetornarVinte_QuandoAtrasoCritico()
        {
            // Arrange - Testando a nova regra de negócio do TCC
            var logs = new List<LogEvento>
            {
                new LogEvento { Tipo = TipoEventoLog.AtrasoCritico, MinutosAtraso = 120 }
            };

            // Act
            double resultado = _calculadoraService.Calcular(logs);

            // Assert
            // Segundo a nossa regra: AtrasoCritico = 0.2 (20%)
            Assert.Equal(20, resultado);
        }

        [Fact]
        public void CalcularPercentual_DeveRetornarZero_QuandoListaForVazia()
        {
            // Arrange
            var logs = new List<LogEvento>();

            // Act
            double resultado = _calculadoraService.Calcular(logs);

            // Assert
            Assert.Equal(0, resultado);
        }
    }
}