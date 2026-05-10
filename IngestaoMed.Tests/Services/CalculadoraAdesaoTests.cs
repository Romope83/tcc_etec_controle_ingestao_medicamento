using IngestaoMed.Core.Enums;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Services;
using Xunit;

namespace IngestaoMed.Tests.Services
{
    public class CalculadoraAdesaoTests
    {
        private readonly CalculadoraAdesaoService _calculadora;

        public CalculadoraAdesaoTests()
        {
            _calculadora = new CalculadoraAdesaoService();
        }

        [Fact]
        public void Calcular_ListaVazia_DeveRetornarZero()
        {
            // Arrange
            var logs = new List<LogEvento>();

            // Act
            var resultado = _calculadora.Calcular(logs);

            // Assert
            Assert.Equal(0, resultado);
        }

        [Fact]
        public void Calcular_IngestaoPontual_DeveRetornarCemPorCento()
        {
            // Arrange
            var logs = new List<LogEvento>
            {
                new LogEvento { Tipo = TipoEventoLog.ConfirmacaoDireta, MinutosAtraso = 5 },
                new LogEvento { Tipo = TipoEventoLog.ConfirmacaoDireta, MinutosAtraso = 10 }
            };

            // Act
            var resultado = _calculadora.Calcular(logs);

            // Assert
            Assert.Equal(100.0, resultado);
        }

        [Fact]
        public void Calcular_IngestaoComSoneca_DeveReduzirIndiceParaSetentaPorCento()
        {
            // Arrange
            // 100% de 1 evento com soneca (peso 0.7) deve resultar em 70%
            var logs = new List<LogEvento>
            {
                new LogEvento { Tipo = TipoEventoLog.ConfirmacaoComSoneca, MinutosAtraso = 20 }
            };

            // Act
            var resultado = _calculadora.Calcular(logs);

            // Assert
            Assert.Equal(70.0, resultado);
        }

        [Fact]
        public void Calcular_MistoDeEventos_DeveCalcularMediaPonderada()
        {
            // Arrange
            // 1 Pontual (1.0) + 1 Soneca (0.7) = 1.7 / 2 = 0.85 (85%)
            var logs = new List<LogEvento>
            {
                new LogEvento { Tipo = TipoEventoLog.ConfirmacaoDireta, MinutosAtraso = 0 },
                new LogEvento { Tipo = TipoEventoLog.ConfirmacaoComSoneca, MinutosAtraso = 30 }
            };

            // Act
            var resultado = _calculadora.Calcular(logs);

            // Assert
            Assert.Equal(85.0, resultado);
        }

        [Fact]
        public void Calcular_AtrasoCritico_DevePenalizarFortemente()
        {
            // Arrange
            // Atraso Crítico tem peso 0.2 (20%)
            var logs = new List<LogEvento>
            {
                new LogEvento { Tipo = TipoEventoLog.AtrasoCritico, MinutosAtraso = 300 }
            };

            // Act
            var resultado = _calculadora.Calcular(logs);

            // Assert
            Assert.Equal(20.0, resultado);
        }
    }
}