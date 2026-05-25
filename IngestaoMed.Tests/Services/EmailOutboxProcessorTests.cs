using Moq;
using Xunit;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IngestaoMed.Tests.Services
{
    public class EmailOutboxProcessorTests
    {
        private readonly Mock<IEmailOutboxService> _outboxServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IConnectivityService> _connectivityMock;
        private readonly EmailOutboxProcessor _processor;

        public EmailOutboxProcessorTests()
        {
            _outboxServiceMock = new Mock<IEmailOutboxService>();
            _emailServiceMock = new Mock<IEmailService>();
            _connectivityMock = new Mock<IConnectivityService>();
            _processor = new EmailOutboxProcessor(
                _outboxServiceMock.Object,
                _emailServiceMock.Object,
                _connectivityMock.Object);
        }

        #region ProcessarFilaAsync

        [Fact]
        public async Task ProcessarFilaAsync_SemInternet_DeveRetornarSemProcessar()
        {
            // Arrange
            _connectivityMock.Setup(c => c.TemInternet).Returns(false);

            // Act
            await _processor.ProcessarFilaAsync();

            // Assert
            _outboxServiceMock.Verify(s => s.ObterPendentesAsync(), Times.Never);
        }

        [Fact]
        public async Task ProcessarFilaAsync_ComInternetEFilaVazia_DeveApenasBuscarPendentes()
        {
            // Arrange
            _connectivityMock.Setup(c => c.TemInternet).Returns(true);
            _outboxServiceMock.Setup(s => s.ObterPendentesAsync())
                .ReturnsAsync(new List<EmailFila>());

            // Act
            await _processor.ProcessarFilaAsync();

            // Assert
            _outboxServiceMock.Verify(s => s.ObterPendentesAsync(), Times.Once);
            _emailServiceMock.Verify(s => s.EnviarAlertaFalhaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcessarFilaAsync_EnvioComSucesso_DeveAtualizarStatusParaSucesso()
        {
            // Arrange
            _connectivityMock.Setup(c => c.TemInternet).Returns(true);
            var emails = new List<EmailFila>
            {
                new EmailFila { Id = 1, Destinatario = "teste@teste.com", Assunto = "Aviso", Corpo = "Alerta" }
            };
            _outboxServiceMock.Setup(s => s.ObterPendentesAsync()).ReturnsAsync(emails);
            _emailServiceMock.Setup(s => s.EnviarAlertaFalhaAsync("teste@teste.com", "Aviso", "Alerta"))
                .ReturnsAsync(true);

            // Act
            await _processor.ProcessarFilaAsync();

            // Assert
            _emailServiceMock.Verify(s => s.EnviarAlertaFalhaAsync("teste@teste.com", "Aviso", "Alerta"), Times.Once);
            _outboxServiceMock.Verify(s => s.AtualizarStatusEnvioAsync(1, true), Times.Once);
        }

        [Fact]
        public async Task ProcessarFilaAsync_EnvioComFalha_DeveAtualizarStatusParaFalha()
        {
            // Arrange
            _connectivityMock.Setup(c => c.TemInternet).Returns(true);
            var emails = new List<EmailFila>
            {
                new EmailFila { Id = 2, Destinatario = "falha@teste.com", Assunto = "Erro", Corpo = "Detalhes" }
            };
            _outboxServiceMock.Setup(s => s.ObterPendentesAsync()).ReturnsAsync(emails);
            _emailServiceMock.Setup(s => s.EnviarAlertaFalhaAsync("falha@teste.com", "Erro", "Detalhes"))
                .ReturnsAsync(false);

            // Act
            await _processor.ProcessarFilaAsync();

            // Assert
            _emailServiceMock.Verify(s => s.EnviarAlertaFalhaAsync("falha@teste.com", "Erro", "Detalhes"), Times.Once);
            _outboxServiceMock.Verify(s => s.AtualizarStatusEnvioAsync(2, false), Times.Once);
        }

        [Fact]
        public async Task ProcessarFilaAsync_QuandoObterPendentesLancarExcecao_DevePropagarErro()
        {
            // Arrange
            _connectivityMock.Setup(c => c.TemInternet).Returns(true);
            _outboxServiceMock.Setup(s => s.ObterPendentesAsync())
                .ThrowsAsync(new Exception("Erro de banco de dados"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _processor.ProcessarFilaAsync());
        }

        #endregion

        #region IniciarProcessamentoAsync

        [Fact]
        public async Task IniciarProcessamentoAsync_QuandoCanceladoImediatamente_DeveEncerrarLoop()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            await _processor.IniciarProcessamentoAsync(cts.Token);

            // Assert
            _connectivityMock.Verify(c => c.TemInternet, Times.Never);
        }

        [Fact]
        public async Task IniciarProcessamentoAsync_ComInternetEEnvioComSucesso_DeveAtualizarPropriedadeEnviado()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            _connectivityMock.Setup(c => c.TemInternet).Returns(true);

            var email = new EmailFila { Id = 1, Destinatario = "sucesso@teste.com", Corpo = "Conteudo", Enviado = false };
            var emails = new List<EmailFila> { email };

            _outboxServiceMock.Setup(s => s.ObterPendentesAsync()).ReturnsAsync(emails);
            _emailServiceMock.Setup(s => s.EnviarAlertaFalhaAsync("sucesso@teste.com", "Paciente", "Conteudo"))
                .ReturnsAsync(true)
                .Callback(() => cts.Cancel()); // Cancela para evitar loop infinito de 5 minutos no teste

            // Act
            await _processor.IniciarProcessamentoAsync(cts.Token);

            // Assert
            Assert.True(email.Enviado);
            _emailServiceMock.Verify(s => s.EnviarAlertaFalhaAsync("sucesso@teste.com", "Paciente", "Conteudo"), Times.Once);
        }

        [Fact]
        public async Task IniciarProcessamentoAsync_ComInternetEEnvioComFalha_NãoDeveAlterarPropriedadeEnviado()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            _connectivityMock.Setup(c => c.TemInternet).Returns(true);

            var email = new EmailFila { Id = 2, Destinatario = "erro@teste.com", Corpo = "Conteudo", Enviado = false };
            var emails = new List<EmailFila> { email };

            _outboxServiceMock.Setup(s => s.ObterPendentesAsync()).ReturnsAsync(emails);
            _emailServiceMock.Setup(s => s.EnviarAlertaFalhaAsync("erro@teste.com", "Paciente", "Conteudo"))
                .ReturnsAsync(false)
                .Callback(() => cts.Cancel());

            // Act
            await _processor.IniciarProcessamentoAsync(cts.Token);

            // Assert
            Assert.False(email.Enviado);
        }

        [Fact]
        public async Task IniciarProcessamentoAsync_QuandoOcorrerExcecaoNoLoop_DeveCapturarEContinuar()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            _connectivityMock.Setup(c => c.TemInternet).Returns(true);

            _outboxServiceMock.Setup(s => s.ObterPendentesAsync())
                .ThrowsAsync(new Exception("Erro temporario de rede"));

            // Act
            var processamentoTask = _processor.IniciarProcessamentoAsync(cts.Token);

            await Task.Delay(50);
            cts.Cancel();

            var excecao = await Record.ExceptionAsync(() => processamentoTask);

            // Assert
            Assert.True(excecao is TaskCanceledException);
            _outboxServiceMock.Verify(s => s.ObterPendentesAsync(), Times.AtLeastOnce);
        }

        #endregion
    }
}