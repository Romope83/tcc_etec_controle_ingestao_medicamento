using Moq;
using Xunit;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Services; // Este ficará no MAUI por causa do Connectivity

namespace IngestaoMed.Tests.Services
{
    public class EmailOutboxProcessorTests
    {
        private readonly Mock<IEmailOutboxService> _outboxMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IConnectivityService> _connectivityMock;
        private readonly EmailOutboxProcessor _processor;

        public EmailOutboxProcessorTests()
        {
            _outboxMock = new Mock<IEmailOutboxService>();
            _emailServiceMock = new Mock<IEmailService>();
            _connectivityMock = new Mock<IConnectivityService>();

            // O Processor recebe os três via DI
            _processor = new EmailOutboxProcessor(
                _outboxMock.Object,
                _emailServiceMock.Object,
                _connectivityMock.Object);
        }

        [Fact]
        public async Task ProcessarFila_SemInternet_DeveAbortarImediatamente()
        {
            // Arrange
            _connectivityMock.Setup(c => c.TemInternet).Returns(false);

            // Act
            await _processor.ProcessarFilaAsync();

            // Assert
            _outboxMock.Verify(o => o.ObterPendentesAsync(), Times.Never);
            _emailServiceMock.Verify(e => e.EnviarAlertaFalhaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcessarFila_ComInternetESucessoNoEnvio_DeveAtualizarStatusParaSucesso()
        {
            // Arrange
            _connectivityMock.Setup(c => c.TemInternet).Returns(true);
            var pendentes = new List<EmailFila>
            {
                new EmailFila { Id = 1, Destinatario = "cuid@teste.com", Assunto = "A", Corpo = "B" }
            };

            _outboxMock.Setup(o => o.ObterPendentesAsync()).ReturnsAsync(pendentes);
            _emailServiceMock.Setup(e => e.EnviarAlertaFalhaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                             .ReturnsAsync(true);

            // Act
            await _processor.ProcessarFilaAsync();

            // Assert
            _outboxMock.Verify(o => o.AtualizarStatusEnvioAsync(1, true), Times.Once);
        }

        [Fact]
        public async Task ProcessarFila_ComInternetMasFalhaNoEnvio_DeveAtualizarStatusParaFalha()
        {
            // Arrange
            _connectivityMock.Setup(c => c.TemInternet).Returns(true);
            var pendentes = new List<EmailFila> { new EmailFila { Id = 1 } };

            _outboxMock.Setup(o => o.ObterPendentesAsync()).ReturnsAsync(pendentes);
            _emailServiceMock.Setup(e => e.EnviarAlertaFalhaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                             .ReturnsAsync(false); // Servidor SMTP recusou

            // Act
            await _processor.ProcessarFilaAsync();

            // Assert
            _outboxMock.Verify(o => o.AtualizarStatusEnvioAsync(1, false), Times.Once);
        }
    }
}