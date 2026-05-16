using Moq;
using Xunit;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Data;
using System.Linq.Expressions;

namespace IngestaoMed.Tests.Services
{
    public class MonitorFalhaServiceTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly MonitorFalhaService _service;

        public MonitorFalhaServiceTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _service = new MonitorFalhaService(_dbMock.Object);
        }

        // --- CAMINHOS FELIZES ---

        [Fact]
        public async Task VerificarFalha_AbaixoDoLimite_NaoDeveEnfileirarEmail()
        {
            // Arrange
            var config = new ConfiguracaoCuidador { AlertaAtivado = true, LimiteSonecasParaAlerta = 3 };
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<ConfiguracaoCuidador>(It.IsAny<Expression<Func<ConfiguracaoCuidador, bool>>>()))
                   .ReturnsAsync(config);

            // Act: 2 sonecas, mas o limite é 3
            await _service.VerificarELoggerFalhaAsync(1, 2);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<EmailFila>()), Times.Never);
        }

        [Fact]
        public async Task VerificarFalha_AlertaDesativado_NaoDeveEnfileirarEmail()
        {
            // Arrange
            var config = new ConfiguracaoCuidador { AlertaAtivado = false, LimiteSonecasParaAlerta = 1 };
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<ConfiguracaoCuidador>(It.IsAny<Expression<Func<ConfiguracaoCuidador, bool>>>()))
                   .ReturnsAsync(config);

            // Act
            await _service.VerificarELoggerFalhaAsync(1, 1);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<EmailFila>()), Times.Never);
        }

        [Fact]
        public async Task VerificarFalha_AgendamentoInexistente_DeveEncerrarSemErro()
        {
            // Arrange
            var config = new ConfiguracaoCuidador { AlertaAtivado = true, LimiteSonecasParaAlerta = 1 };
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<ConfiguracaoCuidador>(It.IsAny<Expression<Func<ConfiguracaoCuidador, bool>>>()))
                   .ReturnsAsync(config);

            // Simula agendamento não encontrado no banco
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync((Agendamento)null!);

            // Act
            await _service.VerificarELoggerFalhaAsync(99, 1);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<EmailFila>()), Times.Never);
        }

        [Fact]
        public async Task VerificarFalha_SemConfiguracao_DeveEncerrarSilenciosamente()
        {
            // Arrange
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<ConfiguracaoCuidador>(It.IsAny<Expression<Func<ConfiguracaoCuidador, bool>>>()))
                   .ReturnsAsync((ConfiguracaoCuidador)null!);

            // Act
            await _service.VerificarELoggerFalhaAsync(1, 5);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<EmailFila>()), Times.Never);
        }
    }
}