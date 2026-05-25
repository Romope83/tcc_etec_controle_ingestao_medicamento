using Moq;
using Xunit;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Threading.Tasks;

namespace IngestaoMed.Tests.ViewModels
{
    public class SnoozeViewModelTests
    {
        private readonly Mock<ISnoozeService> _snoozeServiceMock;
        private readonly Mock<ISnoozeScheduler> _snoozeSchedulerMock;
        private readonly Mock<IMonitorFalhaService> _monitorFalhaServiceMock;
        private readonly SnoozeViewModel _viewModel;

        public SnoozeViewModelTests()
        {
            _snoozeServiceMock = new Mock<ISnoozeService>();
            _snoozeSchedulerMock = new Mock<ISnoozeScheduler>();
            _monitorFalhaServiceMock = new Mock<IMonitorFalhaService>();

            _viewModel = new SnoozeViewModel(
                _snoozeServiceMock.Object,
                _snoozeSchedulerMock.Object,
                _monitorFalhaServiceMock.Object
            );
        }

        #region Cenários de Sucesso

        [Fact]
        public void Inicializar_ComDadosValidos_DevePreencherPropriedadesObsevaveis()
        {
            // Arrange
            int agendamentoId = 42;
            string nomeMedicamento = "Paracetamol";

            _snoozeSchedulerMock.Setup(s => s.ObterTentativas(agendamentoId)).Returns(2);
            _snoozeSchedulerMock.Setup(s => s.PodeAdiar(agendamentoId)).Returns(true);

            // Act
            _viewModel.Inicializar(agendamentoId, nomeMedicamento);

            // Assert
            Assert.Equal(nomeMedicamento, _viewModel.NomeMedicamento);
            Assert.Equal(2, _viewModel.QuantidadeSonecas);
            Assert.True(_viewModel.PodeAdiarNovamente);
        }

        [Fact]
        public async Task ConfirmarIngestao_QuandoAcionado_DeveLimparHistoricoNoScheduler()
        {
            // Arrange
            int agendamentoId = 42;
            _viewModel.Inicializar(agendamentoId, "Dipirona");

            // Act
            await _viewModel.ConfirmarIngestaoCommand.ExecuteAsync(null);

            // Assert
            _snoozeSchedulerMock.Verify(s => s.LimparHistorico(agendamentoId), Times.Once);
        }

        [Fact]
        public void MensagemTempoRestante_QuandoRecebida_DeveAtualizarPropriedadeTempoRestante()
        {
            // Arrange
            string tempoEsperado = "08:45";

            // Act
            WeakReferenceMessenger.Default.Send(tempoEsperado);

            // Assert
            Assert.Equal(tempoEsperado, _viewModel.TempoRestante);
        }

        #endregion

        #region Cenários de Falha / Limite Crítico

        [Fact]
        public async Task SnoozeFinished_QuandoPodeAdiar_NaoDeveAcionarMonitorFalha()
        {
            // Arrange
            int agendamentoId = 42;
            _snoozeSchedulerMock.Setup(s => s.PodeAdiar(agendamentoId)).Returns(true);
            _viewModel.Inicializar(agendamentoId, "Ibuprofeno");

            // Act
            WeakReferenceMessenger.Default.Send("DadosAdicionais", "SnoozeFinished");
            await Task.Delay(50); // Pequena pausa para execução da thread asíncrona do mensageiro

            // Assert
            _monitorFalhaServiceMock.Verify(m => m.VerificarELoggerFalhaAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task SnoozeFinished_QuandoLimiteSonecasAtingido_DeveAcionarMonitorFalha()
        {
            // Arrange
            int agendamentoId = 42;
            int totalSonecasAtingido = 3;

            _snoozeSchedulerMock.Setup(s => s.PodeAdiar(agendamentoId)).Returns(false);
            _snoozeSchedulerMock.Setup(s => s.ObterTentativas(agendamentoId)).Returns(totalSonecasAtingido);

            _viewModel.Inicializar(agendamentoId, "Amoxicilina");

            // Act
            WeakReferenceMessenger.Default.Send("DadosAdicionais", "SnoozeFinished");
            await Task.Delay(50); // Aguarda o processamento do manipulador assíncrono

            // Assert
            _monitorFalhaServiceMock.Verify(m =>
                m.VerificarELoggerFalhaAsync(agendamentoId, totalSonecasAtingido),
                Times.Once
            );
        }

        #endregion
    }
}