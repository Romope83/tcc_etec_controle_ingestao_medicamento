using Moq;
using Xunit;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace IngestaoMed.Tests.Services
{
    public class CuidadorServiceTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly CuidadorService _service;

        public CuidadorServiceTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _authServiceMock = new Mock<IAuthService>();
            _service = new CuidadorService(_dbMock.Object, _authServiceMock.Object);
        }

        #region SalvarCuidadorAsync

        [Fact]
        public async Task SalvarCuidadorAsync_QuandoCuidadorForNulo_DeveRetornarFalse()
        {
            Cuidador cuidadorNulo = null!;

            var resultado = await _service.SalvarCuidadorAsync(cuidadorNulo);

            Assert.False(resultado);
            _dbMock.Verify(x => x.InserirAsync(It.IsAny<Cuidador>()), Times.Never);
            _authServiceMock.Verify(x => x.RegistrarCuidador(It.IsAny<Cuidador>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SalvarCuidadorAsync_ComIdMaiorQueZero_DeveAtualizarNoBancoERetornarTrue()
        {
            // Arrange
            var cuidador = new Cuidador { Id = 1, Nome = "Ronaldo", PasswordHash = "novaSenha123" };

            // Configura o mock para o método AtualizarAsync retornar 1 (sucesso)
            _dbMock.Setup(x => x.AtualizarAsync(It.IsAny<Cuidador>()))
                   .ReturnsAsync(1);

            // Act
            var resultado = await _service.SalvarCuidadorAsync(cuidador);

            // Assert
            Assert.True(resultado);
            _dbMock.Verify(x => x.AtualizarAsync(It.IsAny<Cuidador>()), Times.Once);
            _authServiceMock.Verify(x => x.RegistrarCuidador(It.IsAny<Cuidador>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SalvarCuidadorAsync_ComIdMaiorQueZeroEAtualizacaoFalhar_DeveRetornarFalse()
        {
            // Arrange
            var cuidadorExistente = new Cuidador { Id = 10, PasswordHash = "HashExistente" };

            // Configura o AtualizarAsync para retornar 0 (nenhuma linha afetada/falha)
            _dbMock.Setup(x => x.AtualizarAsync(It.IsAny<Cuidador>()))
                   .ReturnsAsync(0);

            // Act
            var resultado = await _service.SalvarCuidadorAsync(cuidadorExistente);

            // Assert
            Assert.False(resultado);
            _dbMock.Verify(x => x.AtualizarAsync(It.IsAny<Cuidador>()), Times.Once);
            _dbMock.Verify(x => x.InserirAsync(It.IsAny<Cuidador>()), Times.Never);
        }

        [Fact]
        public async Task SalvarCuidadorAsync_ComIdZero_DeveRegistrarViaAuthServiceERetornarTrue()
        {
            var novoCuidador = new Cuidador { Id = 0, PasswordHash = "MinhaSenha123" };
            _authServiceMock.Setup(x => x.RegistrarCuidador(novoCuidador, "MinhaSenha123"))
                             .ReturnsAsync(true);

            var resultado = await _service.SalvarCuidadorAsync(novoCuidador);

            Assert.True(resultado);
            _authServiceMock.Verify(x => x.RegistrarCuidador(novoCuidador, "MinhaSenha123"), Times.Once);
            _dbMock.Verify(x => x.InserirAsync(It.IsAny<Cuidador>()), Times.Never);
        }

        [Fact]
        public async Task SalvarCuidadorAsync_ComIdZeroERegistroFalhar_DeveRetornarFalse()
        {
            var novoCuidador = new Cuidador { Id = 0, PasswordHash = "MinhaSenha123" };
            _authServiceMock.Setup(x => x.RegistrarCuidador(novoCuidador, "MinhaSenha123"))
                             .ReturnsAsync(false);

            var resultado = await _service.SalvarCuidadorAsync(novoCuidador);

            Assert.False(resultado);
            _authServiceMock.Verify(x => x.RegistrarCuidador(novoCuidador, "MinhaSenha123"), Times.Once);
        }

        [Fact]
        public async Task SalvarCuidadorAsync_QuandoBancoLancarExcecao_DeveCapturarRetornarFalse()
        {
            var cuidadorExistente = new Cuidador { Id = 5 };
            _dbMock.Setup(x => x.InserirAsync(It.IsAny<Cuidador>()))
                   .ThrowsAsync(new Exception("Falha crítica no SQLite"));

            var resultado = await _service.SalvarCuidadorAsync(cuidadorExistente);

            Assert.False(resultado);
        }

        [Fact]
        public async Task SalvarCuidadorAsync_QuandoAuthServiceLancarExcecao_DeveCapturarRetornarFalse()
        {
            var novoCuidador = new Cuidador { Id = 0, PasswordHash = "Senha" };
            _authServiceMock.Setup(x => x.RegistrarCuidador(It.IsAny<Cuidador>(), It.IsAny<string>()))
                             .ThrowsAsync(new Exception("Falha no serviço de criptografia"));

            var resultado = await _service.SalvarCuidadorAsync(novoCuidador);

            Assert.False(resultado);
        }

        #endregion

        #region ObterPorIdAsync

        [Fact]
        public async Task ObterPorIdAsync_QuandoCuidadorExistir_DeveRetornarCuidador()
        {
            // Arrange
            int idAlvo = 1;
            var cuidadorEsperado = new Cuidador { Id = idAlvo, Nome = "Ronaldo" };
            _dbMock.Setup(x => x.BuscarPrimeiroAsync<Cuidador>(It.IsAny<System.Linq.Expressions.Expression<Func<Cuidador, bool>>>()))
                   .ReturnsAsync(cuidadorEsperado);

            // Act
            var resultado = await _service.ObterPorIdAsync(idAlvo);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(idAlvo, resultado?.Id);
            Assert.Equal("Ronaldo", resultado?.Nome);
        }

        [Fact]
        public async Task ObterPorIdAsync_QuandoCuidadorNaoExistir_DeveRetornarNulo()
        {
            // Arrange
            int idInexistente = 999;
            _dbMock.Setup(x => x.BuscarPrimeiroAsync<Cuidador>(It.IsAny<System.Linq.Expressions.Expression<Func<Cuidador, bool>>>()))
                   .ReturnsAsync((Cuidador?)null);

            // Act
            var resultado = await _service.ObterPorIdAsync(idInexistente);

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObterPorIdAsync_QuandoBancoLancarExcecao_DevePropagarErro()
        {
            // Arrange
            int id = 1;
            _dbMock.Setup(x => x.BuscarPrimeiroAsync<Cuidador>(It.IsAny<System.Linq.Expressions.Expression<Func<Cuidador, bool>>>()))
                   .ThrowsAsync(new Exception("Erro de leitura do banco"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.ObterPorIdAsync(id));
        }

        #endregion
    }
}