using Moq;
using Xunit;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Data;
using IngestaoMed.Core.Enums;
using System.Linq.Expressions;

namespace IngestaoMed.Tests.Services
{
    public class LogServiceTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly LogService _service;

        public LogServiceTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _service = new LogService(_dbMock.Object);
        }

        [Fact]
        public async Task RegistrarAsync_ConfirmacaoDireta_DeveCalcularAtrasoCorretamente()
        {
            // Arrange
            int id = 1;
            // Definimos que o remédio era para as 08:00 e agora são 08:15 (15 min de atraso)
            var horarioProgramado = DateTime.Now.AddMinutes(-15);
            var agendamento = new Agendamento { Id = id, HorarioProgramado = horarioProgramado };

            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(agendamento);

            // Act
            await _service.RegistrarAsync(id, TipoEventoLog.ConfirmacaoDireta);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.Is<LogEvento>(l =>
                l.AgendamentoId == id &&
                l.Tipo == TipoEventoLog.ConfirmacaoDireta &&
                l.MinutosAtraso >= 15 // Verifica se o cálculo de atraso ocorreu
            )), Times.Once);
        }

        [Fact]
        public async Task RegistrarAsync_Soneca_NaoDeveContabilizarAtrasoComoIngestao()
        {
            // Arrange
            int id = 1;
            var agendamento = new Agendamento { Id = id, HorarioProgramado = DateTime.Now.AddMinutes(-5) };
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(agendamento);

            // Act
            await _service.RegistrarAsync(id, TipoEventoLog.SonecaDisparada);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.Is<LogEvento>(l =>
                l.Tipo == TipoEventoLog.SonecaDisparada &&
                l.MinutosAtraso == 0 // Soneca não é atraso de ingestão, é apenas um adiamento
            )), Times.Once);
        }

        [Fact]
        public async Task RegistrarAsync_AgendamentoInexistente_DeveEncerrarSilenciosamente()
        {
            // Arrange
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync((Agendamento)null!);

            // Act
            await _service.RegistrarAsync(999, TipoEventoLog.ConfirmacaoDireta);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<LogEvento>()), Times.Never);
        }

        [Fact]
        public async Task ObterLogsPorTratamentoAsync_DeveFiltrarCorretamente()
        {
            // Arrange
            int tratamentoId = 10;
            var logs = new List<LogEvento>
            {
                new LogEvento { Id = 1, Agendamento = new Agendamento { TratamentoId = tratamentoId } },
                new LogEvento { Id = 2, Agendamento = new Agendamento { TratamentoId = 99 } } // Outro tratamento
            };

            _dbMock.Setup(d => d.BuscarTodosAsync<LogEvento>()).ReturnsAsync(logs);

            // Act
            var resultado = await _service.ObterLogsPorTratamentoAsync(tratamentoId);

            // Assert
            Assert.Single(resultado);
            Assert.Equal(tratamentoId, resultado[0].Agendamento!.TratamentoId);
        }
    }
}