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
    public class SnoozeServiceTests
    {
        private readonly Mock<ISnoozeScheduler> _snoozeSchedulerMock;
        private readonly Mock<IAlarmService> _alarmServiceMock;
        private readonly Mock<IDatabaseContext> _databaseMock;
        private readonly SnoozeService _service;

        public SnoozeServiceTests()
        {
            _snoozeSchedulerMock = new Mock<ISnoozeScheduler>();
            _alarmServiceMock = new Mock<IAlarmService>();
            _databaseMock = new Mock<IDatabaseContext>();

            _service = new SnoozeService(
                _snoozeSchedulerMock.Object,
                _alarmServiceMock.Object,
                _databaseMock.Object);
        }

        #region AgendarSonecaAsync Tests

        [Fact]
        public async Task AgendarSonecaAsync_LimiteDeSonecasAtingido_DeveLancarInvalidOperationExceptionSemModificarBanco()
        {
            // Arrange
            int agendamentoId = 1;
            int minutos = 10;
            _snoozeSchedulerMock.Setup(s => s.PodeAdiar(agendamentoId)).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AgendarSonecaAsync(agendamentoId, minutos));

            _databaseMock.Verify(d => d.BuscarPrimeiroAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()), Times.Never);
            _databaseMock.Verify(d => d.AtualizarAsync(It.IsAny<Agendamento>()), Times.Never);
            _alarmServiceMock.Verify(a => a.AgendarNotificacaoAsync(It.IsAny<Agendamento>()), Times.Never);
        }

        [Fact]
        public async Task AgendarSonecaAsync_AgendamentoInexistenteNoBanco_DeveLancarExceptionGenericaAposValidarLimite()
        {
            // Arrange
            int agendamentoId = 99;
            int minutos = 10;
            _snoozeSchedulerMock.Setup(s => s.PodeAdiar(agendamentoId)).Returns(true);
            _databaseMock.Setup(d => d.BuscarPrimeiroAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                         .ReturnsAsync((Agendamento?)null);

            // Act & Assert
            var excecao = await Assert.ThrowsAsync<Exception>(() =>
                _service.AgendarSonecaAsync(agendamentoId, minutos));

            Assert.Equal("Agendamento não encontrado.", excecao.Message);
            _snoozeSchedulerMock.Verify(s => s.RegistrarSoneca(It.IsAny<int>()), Times.Never);
            _databaseMock.Verify(d => d.AtualizarAsync(It.IsAny<Agendamento>()), Times.Never);
        }

        [Fact]
        public async Task AgendarSonecaAsync_AgendamentoValidoELimiteDisponivel_DeveAtualizarHorarioEAgendarNotificacaoFisica()
        {
            // Arrange
            int agendamentoId = 1;
            int minutos = 15;
            var agendamento = new Agendamento { Id = agendamentoId, ProximoAlarme = DateTime.MinValue };

            _snoozeSchedulerMock.Setup(s => s.PodeAdiar(agendamentoId)).Returns(true);
            _databaseMock.Setup(d => d.BuscarPrimeiroAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                         .ReturnsAsync(agendamento);

            // Act
            DateTime horarioRetornado = await _service.AgendarSonecaAsync(agendamentoId, minutos);

            // Assert
            Assert.True(horarioRetornado > DateTime.Now.AddSeconds(-5));
            Assert.Equal(horarioRetornado, agendamento.ProximoAlarme);

            _snoozeSchedulerMock.Verify(s => s.RegistrarSoneca(agendamentoId), Times.Once);
            _databaseMock.Verify(d => d.AtualizarAsync(agendamento), Times.Once);
            _alarmServiceMock.Verify(a => a.AgendarNotificacaoAsync(agendamento), Times.Once);
        }

        #endregion

        #region CancelarSoneca Tests

        [Fact]
        public void CancelarSoneca_IdValido_DeveRemoverNotificacaoEResetarHistoricoDoAgendamento()
        {
            // Arrange
            int agendamentoId = 1;

            // Act
            _service.CancelarSoneca(agendamentoId);

            // Assert
            _alarmServiceMock.Verify(a => a.CancelarAlarmeAsync(agendamentoId), Times.Once);
            _snoozeSchedulerMock.Verify(s => s.LimparHistorico(agendamentoId), Times.Once);
        }

        #endregion
    }
}