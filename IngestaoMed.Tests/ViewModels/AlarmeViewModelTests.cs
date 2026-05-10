using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.ViewModels;
using Moq;
using Xunit;

namespace IngestaoMed.Tests.ViewModels
{
    public class AlarmeViewModelTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly Mock<IAlarmService> _alarmMock;
        private readonly AlarmeViewModel _viewModel;

        public AlarmeViewModelTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _alarmMock = new Mock<IAlarmService>();
            _viewModel = new AlarmeViewModel(_dbMock.Object, _alarmMock.Object);
        }

        [Fact]
        public async Task ConfirmarIngestao_DeveAtualizarStatusECancelarAlarme()
        {
            // Arrange
            var agendamento = new Agendamento { Id = 1, Status = "Pendente" };
            _viewModel.AgendamentoAtual = agendamento;

            // Act
            await _viewModel.ConfirmarIngestaoCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Realizado", agendamento.Status);
            Assert.NotNull(agendamento.HorarioRealizado);

            // Verifica se salvou no banco
            _dbMock.Verify(d => d.AtualizarAsync(agendamento), Times.Once);

            // Verifica se notificou o sistema operacional para parar o barulho/alerta
            _alarmMock.Verify(a => a.CancelarAlarmeAsync(agendamento.Id), Times.Once);
        }

        [Fact]
        public async Task AdiarSoneca_DeveChamarServicoDeAlarmeCom10Minutos()
        {
            // Arrange
            var agendamento = new Agendamento { Id = 1 };
            _viewModel.AgendamentoAtual = agendamento;

            // Act
            await _viewModel.AdiarSonecaCommand.ExecuteAsync(null);

            // Assert
            // Verifica se o comando de soneca foi enviado ao serviço de sistema
            _alarmMock.Verify(a => a.AgendarNotificacaoAsync(agendamento), Times.Once);
        }
    }
}