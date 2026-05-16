using Moq;
using Xunit;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Data;
using System.Linq.Expressions;

namespace IngestaoMed.Tests.Services
{
    public class SnoozeServiceTests
    {
        private readonly Mock<ISnoozeScheduler> _schedulerMock;
        private readonly Mock<IAlarmService> _alarmMock;
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly SnoozeService _service;

        public SnoozeServiceTests()
        {
            _schedulerMock = new Mock<ISnoozeScheduler>();
            _alarmMock = new Mock<IAlarmService>();
            _dbMock = new Mock<IDatabaseContext>();

            _service = new SnoozeService(
                _schedulerMock.Object,
                _alarmMock.Object,
                _dbMock.Object);
        }

        [Fact]
        public async Task AgendarSoneca_CaminhoFeliz_DeveAgendarCorretamente()
        {
            // Arrange
            int id = 1;
            var agendamento = new Agendamento { Id = id, ProximoAlarme = DateTime.Now };

            _schedulerMock.Setup(s => s.PodeAdiar(id)).Returns(true);
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(agendamento);

            // Act
            var novoHorario = await _service.AgendarSonecaAsync(id, 10);

            // Assert
            Assert.True(novoHorario > DateTime.Now);
            _schedulerMock.Verify(s => s.RegistrarSoneca(id), Times.Once);
            _dbMock.Verify(d => d.AtualizarAsync(agendamento), Times.Once);
            _alarmMock.Verify(a => a.AgendarNotificacaoAsync(agendamento), Times.Once);
        }

        [Fact]
        public async Task AgendarSoneca_CaminhoTriste_DeveLancarExcecao_QuandoLimiteAtingido()
        {
            // Arrange
            int id = 1;
            _schedulerMock.Setup(s => s.PodeAdiar(id)).Returns(false);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AgendarSonecaAsync(id, 10));

            Assert.Equal("Limite de sonecas atingido.", ex.Message);
            _alarmMock.Verify(a => a.AgendarNotificacaoAsync(It.IsAny<Agendamento>()), Times.Never);
        }

        [Fact]
        public async Task AgendarSoneca_CaminhoTriste_DeveLancarExcecao_QuandoAgendamentoNaoExiste()
        {
            // Arrange
            int id = 99;
            _schedulerMock.Setup(s => s.PodeAdiar(id)).Returns(true);
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync((Agendamento)null!);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.AgendarSonecaAsync(id, 10));
        }
    }
}