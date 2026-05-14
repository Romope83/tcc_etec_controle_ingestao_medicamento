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
        public async Task VerificarFalha_NoLimite_DeveEnfileirarEmailComDadosCorretos()
        {
            // Arrange
            var config = new ConfiguracaoCuidador
            {
                AlertaAtivado = true,
                LimiteSonecasParaAlerta = 3,
                EmailCuidador = "cuidador@teste.com",
                NomeCuidador = "João"
            };

            var agendamento = new Agendamento { Id = 1, TratamentoId = 10 };
            var tratamento = new Tratamento { Id = 10, Nome = "Dipirona 500mg" };

            // 1. Setup para a Configuração (Geralmente sem predicado)
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<ConfiguracaoCuidador>())
                   .ReturnsAsync(config);

            // 2. Setup para o Agendamento (Use o predicado genérico explicitamente)
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(agendamento);

            // 3. Setup para o Tratamento
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(tratamento);

            // Act
            await _service.VerificarELoggerFalhaAsync(1, 3);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.Is<EmailFila>(e =>
                e.Destinatario == "cuidador@teste.com" &&
                e.Corpo.Contains("Dipirona 500mg") &&
                e.Enviado == false)), Times.Once);
        }

        // --- CAMINHOS TRISTES / REGRAS DE BLOQUEIO ---

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