using Moq;
using Xunit;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Data;
using System.Linq.Expressions;

namespace IngestaoMed.Tests.Services
{
    public class EmailOutboxServiceTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly EmailOutboxService _service;

        public EmailOutboxServiceTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _service = new EmailOutboxService(_dbMock.Object);
        }

        #region ObterPendentes

        [Fact]
        public async Task ObterPendentes_DeveRetornarApenasNaoEnviadosComTentativasBaixas()
        {
            // Arrange
            var lista = new List<EmailFila>
            {
                new EmailFila { Id = 1, Enviado = false, Tentativas = 0 }, // Deve retornar
                new EmailFila { Id = 2, Enviado = true, Tentativas = 1 },  // Já enviado (pula)
                new EmailFila { Id = 3, Enviado = false, Tentativas = 3 }  // Limite atingido (pula)
            };
            _dbMock.Setup(d => d.BuscarTodosAsync<EmailFila>()).ReturnsAsync(lista);

            // Act
            var resultado = await _service.ObterPendentesAsync();

            // Assert
            Assert.Single(resultado);
            Assert.Equal(1, resultado[0].Id);
        }

        #endregion

        #region AtualizarStatusEnvio

        [Fact]
        public async Task AtualizarStatus_Sucesso_DeveMarcarComoEnviado()
        {
            // Arrange
            var email = new EmailFila { Id = 1, Enviado = false };
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<EmailFila>(It.IsAny<Expression<Func<EmailFila, bool>>>()))
                   .ReturnsAsync(email);

            // Act
            await _service.AtualizarStatusEnvioAsync(1, true);

            // Assert
            _dbMock.Verify(d => d.AtualizarAsync(It.Is<EmailFila>(e => e.Enviado == true)), Times.Once);
        }

        [Fact]
        public async Task AtualizarStatus_Falha_DeveIncrementarTentativas()
        {
            // Arrange
            var email = new EmailFila { Id = 1, Enviado = false, Tentativas = 0 };
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<EmailFila>(It.IsAny<Expression<Func<EmailFila, bool>>>()))
                   .ReturnsAsync(email);

            // Act
            await _service.AtualizarStatusEnvioAsync(1, false);

            // Assert
            _dbMock.Verify(d => d.AtualizarAsync(It.Is<EmailFila>(e => e.Tentativas == 1)), Times.Once);
        }

        [Fact]
        public async Task AtualizarStatus_EmailNaoEncontrado_NaoDeveChamarBanco()
        {
            // Arrange
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<EmailFila>(It.IsAny<Expression<Func<EmailFila, bool>>>()))
                   .ReturnsAsync((EmailFila)null!);

            // Act
            await _service.AtualizarStatusEnvioAsync(999, true);

            // Assert
            _dbMock.Verify(d => d.AtualizarAsync(It.IsAny<EmailFila>()), Times.Never);
        }

        #endregion

        #region LimparFilaAntiga

        [Fact]
        public async Task LimparFila_DeveExcluirApenasEmailsEnviadosAntigos()
        {
            // Arrange
            var hoje = DateTime.Now;
            var lista = new List<EmailFila>
            {
                new EmailFila { Id = 1, Enviado = true, DataCriacao = hoje.AddDays(-10) }, // Deve excluir
                new EmailFila { Id = 2, Enviado = true, DataCriacao = hoje.AddDays(-2) },  // Enviado mas recente (fica)
                new EmailFila { Id = 3, Enviado = false, DataCriacao = hoje.AddDays(-10) } // Antigo mas NÃO enviado (fica)
            };
            _dbMock.Setup(d => d.BuscarTodosAsync<EmailFila>()).ReturnsAsync(lista);

            // Act
            await _service.LimparFilaAntigaAsync(7);

            // Assert
            _dbMock.Verify(d => d.ExcluirAsync(It.Is<EmailFila>(e => e.Id == 1)), Times.Once);
            _dbMock.Verify(d => d.ExcluirAsync(It.Is<EmailFila>(e => e.Id == 2)), Times.Never);
            _dbMock.Verify(d => d.ExcluirAsync(It.Is<EmailFila>(e => e.Id == 3)), Times.Never);
        }

        #endregion
    }
}