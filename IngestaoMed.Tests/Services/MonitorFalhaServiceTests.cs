using Moq;
using Xunit;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Data;
using IngestaoMed.Core.Models;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

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

        #region VerificarELoggerFalhaAsync Tests

        [Fact]
        public async Task VerificarELoggerFalhaAsync_ConfiguracaoNaoEncontrada_DeveEncerrarSemCriarAlerta()
        {
            // Arrange
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<ConfiguracaoCuidador>())
                   .ReturnsAsync((ConfiguracaoCuidador?)null);

            // Act
            await _service.VerificarELoggerFalhaAsync(agendamentoId: 1, totalSonecas: 5);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<EmailFila>()), Times.Never);
        }

        [Fact]
        public async Task VerificarELoggerFalhaAsync_AlertaDesativadoNaConfiguracao_DeveEncerrarSemCriarAlerta()
        {
            // Arrange
            var config = new ConfiguracaoCuidador { AlertaAtivado = false, LimiteSonecasParaAlerta = 3 };
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<ConfiguracaoCuidador>())
                   .ReturnsAsync(config);

            // Act
            await _service.VerificarELoggerFalhaAsync(agendamentoId: 1, totalSonecas: 5);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<EmailFila>()), Times.Never);
        }

        [Fact]
        public async Task VerificarELoggerFalhaAsync_TotalSonecasMenorQueOLimiteConfigurado_DeveEncerrarSemCriarAlerta()
        {
            // Arrange
            var config = new ConfiguracaoCuidador { AlertaAtivado = true, LimiteSonecasParaAlerta = 3 };
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<ConfiguracaoCuidador>())
                   .ReturnsAsync(config);

            // Act
            await _service.VerificarELoggerFalhaAsync(agendamentoId: 1, totalSonecas: 2);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<EmailFila>()), Times.Never);
        }

        [Fact]
        public async Task VerificarELoggerFalhaAsync_AgendamentoNaoEncontrado_DeveEncerrarSemCriarAlerta()
        {
            // Arrange
            var config = new ConfiguracaoCuidador { AlertaAtivado = true, LimiteSonecasParaAlerta = 3 };
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<ConfiguracaoCuidador>())
                   .ReturnsAsync(config);

            _dbMock.Setup(d => d.BuscarPrimeiroAsync(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync((Agendamento?)null);

            // Act
            await _service.VerificarELoggerFalhaAsync(agendamentoId: 99, totalSonecas: 3);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<EmailFila>()), Times.Never);
        }

        [Fact]
        public async Task VerificarELoggerFalhaAsync_VinculoMedicamentoTratamentoNaoEncontrado_DeveEncerrarSemCriarAlerta()
        {
            // Arrange
            var config = new ConfiguracaoCuidador { AlertaAtivado = true, LimiteSonecasParaAlerta = 3 };
            var agendamento = new Agendamento { Id = 1, MedicamentoTratamentoId = 10 };

            _dbMock.Setup(d => d.BuscarPrimeiroAsync<ConfiguracaoCuidador>())
                   .ReturnsAsync(config);

            _dbMock.Setup(d => d.BuscarPrimeiroAsync(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(agendamento);

            _dbMock.Setup(d => d.BuscarPrimeiroAsync(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync((MedicamentoTratamento?)null);

            // Act
            await _service.VerificarELoggerFalhaAsync(agendamentoId: 1, totalSonecas: 3);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<EmailFila>()), Times.Never);
        }

        [Fact]
        public async Task VerificarELoggerFalhaAsync_TodosDadosValidosEMedicamentoNaoIdentificado_DeveInserirEmailComNomeAlternativoNaFila()
        {
            // Arrange
            var config = new ConfiguracaoCuidador { AlertaAtivado = true, LimiteSonecasParaAlerta = 3, EmailCuidador = "cuidador@teste.com", NomeCuidador = "Carlos" };
            var agendamento = new Agendamento { Id = 1, MedicamentoTratamentoId = 10 };
            var vinculo = new MedicamentoTratamento { Id = 10, MedicamentoId = 100, Dosagem = "1 comprimido", Instrucoes = "Após o almoço" };

            _dbMock.Setup(d => d.BuscarPrimeiroAsync<ConfiguracaoCuidador>())
                   .ReturnsAsync(config);

            _dbMock.Setup(d => d.BuscarPrimeiroAsync(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(agendamento);

            _dbMock.Setup(d => d.BuscarPrimeiroAsync(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(vinculo);

            _dbMock.Setup(d => d.BuscarPrimeiroAsync(It.IsAny<Expression<Func<Medicamento, bool>>>()))
                   .ReturnsAsync((Medicamento?)null);

            // Act
            await _service.VerificarELoggerFalhaAsync(agendamentoId: 1, totalSonecas: 3);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.Is<EmailFila>(e =>
                e.Destinatario == "cuidador@teste.com" &&
                e.Assunto.Contains("Medicamento não identificado") &&
                e.Enviado == false)), Times.Once);
        }

        [Fact]
        public async Task VerificarELoggerFalhaAsync_TodosDadosValidosEMedicamentoEncontrado_DeveInserirEmailCompletoNaFilaComSucesso()
        {
            // Arrange
            var config = new ConfiguracaoCuidador { AlertaAtivado = true, LimiteSonecasParaAlerta = 3, EmailCuidador = "cuidador@teste.com", NomeCuidador = "Carlos" };
            var agendamento = new Agendamento { Id = 1, MedicamentoTratamentoId = 10 };
            var vinculo = new MedicamentoTratamento { Id = 10, MedicamentoId = 100, Dosagem = "1 comprimido", Instrucoes = "Após o almoço" };
            var medicamento = new Medicamento { Id = 100, NomeComercial = "Amoxicilina" };

            _dbMock.Setup(d => d.BuscarPrimeiroAsync<ConfiguracaoCuidador>())
                   .ReturnsAsync(config);

            _dbMock.Setup(d => d.BuscarPrimeiroAsync(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(agendamento);

            _dbMock.Setup(d => d.BuscarPrimeiroAsync(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(vinculo);

            _dbMock.Setup(d => d.BuscarPrimeiroAsync(It.IsAny<Expression<Func<Medicamento, bool>>>()))
                   .ReturnsAsync(medicamento);

            // Act
            await _service.VerificarELoggerFalhaAsync(agendamentoId: 1, totalSonecas: 4);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.Is<EmailFila>(e =>
                e.Destinatario == "cuidador@teste.com" &&
                e.Assunto.Contains("Amoxicilina") &&
                e.Corpo.Contains("Amoxicilina") &&
                e.Corpo.Contains("1 comprimido") &&
                e.Enviado == false)), Times.Once);
        }

        #endregion
    }
}