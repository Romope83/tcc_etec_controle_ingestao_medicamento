using Moq;
using Xunit;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Data;
using System;
using System.Threading.Tasks;
using System.Linq.Expressions;
using BC = BCrypt.Net.BCrypt;
using IngestaoMed.Core.Interfaces;

namespace IngestaoMed.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly Mock<IConfigService> _configMock;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _configMock = new Mock<IConfigService>();
            _service = new AuthService(_dbMock.Object, _configMock.Object);
        }

        #region RegistrarCuidador

        [Fact]
        public async Task RegistrarCuidador_ComDadosValidos_DeveCriptografarSenhaESalvarComSucesso()
        {
            var cuidador = new Cuidador { Id = 1 };
            string senhaLimpa = "SenhaValida123";
            _dbMock.Setup(x => x.InserirAsync(It.IsAny<Cuidador>()))
                   .ReturnsAsync(true);

            var resultado = await _service.RegistrarCuidador(cuidador, senhaLimpa);

            Assert.True(resultado);
            Assert.False(string.IsNullOrWhiteSpace(cuidador.PasswordHash));
            Assert.True(BC.Verify(senhaLimpa, cuidador.PasswordHash));
            _dbMock.Verify(x => x.InserirAsync(cuidador), Times.Once);
        }

        [Fact]
        public async Task RegistrarCuidador_QuandoBancoFalhar_DeveRetornarFalse()
        {
            var cuidador = new Cuidador { Id = 1 };
            string senhaLimpa = "SenhaValida123";
            _dbMock.Setup(x => x.InserirAsync(It.IsAny<Cuidador>()))
                   .ReturnsAsync(false);

            var resultado = await _service.RegistrarCuidador(cuidador, senhaLimpa);

            Assert.False(resultado);
        }

        [Fact]
        public async Task RegistrarCuidador_QuandoSenhaForNula_DeveLancarArgumentNullException()
        {
            var cuidador = new Cuidador { Id = 1 };
            string senhaLimpa = null!;

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.RegistrarCuidador(cuidador, senhaLimpa));
        }

        #endregion

        #region ValidarLogin

        [Fact]
        public async Task ValidarLogin_ComCredenciaisCorretas_DeveRetornarTrue()
        {
            string email = "cuidador@teste.com";
            string senhaLimpa = "SenhaCorreta123";
            var hashGerado = BC.HashPassword(senhaLimpa);
            var usuarioDoBanco = new Cuidador { Id = 1, Email = email, PasswordHash = hashGerado };
            _dbMock.Setup(x => x.BuscarPrimeiroAsync<Cuidador>(It.IsAny<Expression<Func<Cuidador, bool>>>()))
                   .ReturnsAsync(usuarioDoBanco);

            var resultado = await _service.ValidarLogin(email, senhaLimpa);

            Assert.True(resultado);
        }

        [Fact]
        public async Task ValidarLogin_ComSenhaIncorreta_DeveRetornarFalse()
        {
            string email = "cuidador@teste.com";
            string senhaCorreta = "SenhaCorreta123";
            string senhaErrada = "SenhaErrada123";
            var hashGerado = BC.HashPassword(senhaCorreta);
            var usuarioDoBanco = new Cuidador { Id = 1, Email = email, PasswordHash = hashGerado };
            _dbMock.Setup(x => x.BuscarPrimeiroAsync<Cuidador>(It.IsAny<Expression<Func<Cuidador, bool>>>()))
                   .ReturnsAsync(usuarioDoBanco);

            var resultado = await _service.ValidarLogin(email, senhaErrada);

            Assert.False(resultado);
        }

        [Fact]
        public async Task ValidarLogin_QuandoEmailNaoExistirNoBanco_DeveRetornarFalse()
        {
            string emailInexistente = "nao_existe@teste.com";
            string senha = "QualquerSenha123";
            _dbMock.Setup(x => x.BuscarPrimeiroAsync<Cuidador>(It.IsAny<Expression<Func<Cuidador, bool>>>()))
                   .ReturnsAsync((Cuidador?)null);

            var resultado = await _service.ValidarLogin(emailInexistente, senha);

            Assert.False(resultado);
        }

        #endregion

        #region ExisteCuidadorCadastrado

        [Fact]
        public async Task ExisteCuidadorCadastrado_QuandoExistiremRegistros_DeveRetornarTrue()
        {
            _dbMock.Setup(x => x.ContarAsync<Cuidador>())
                   .ReturnsAsync(1);

            var resultado = await _service.ExisteCuidadorCadastrado();

            Assert.True(resultado);
        }

        [Fact]
        public async Task ExisteCuidadorCadastrado_QuandoTabelaEstiverVazia_DeveRetornarFalse()
        {
            _dbMock.Setup(x => x.ContarAsync<Cuidador>())
                   .ReturnsAsync(0);

            var resultado = await _service.ExisteCuidadorCadastrado();

            Assert.False(resultado);
        }

        #endregion

        #region GetCuidadorAtual

        [Fact]
        public async Task GetCuidadorAtual_QuandoUsuarioExistir_DeveRetornarCuidador()
        {
            // Arrange
            int idEsperado = 1;
            var configFake = new ConfiguracaoCuidador { Id = idEsperado };
            var cuidadorEsperado = new Cuidador { Id = idEsperado, Nome = "Ronaldo" };

            // Configura o mock para simular o usuário logado nas preferências
            _configMock.Setup(c => c.ConfiguracaoCuidador).Returns(configFake);

            // Configura o mock do banco para retornar o cuidador baseado na expressão de ID
            _dbMock.Setup(x => x.BuscarPrimeiroAsync<Cuidador>(It.IsAny<System.Linq.Expressions.Expression<Func<Cuidador, bool>>>()))
                   .ReturnsAsync(cuidadorEsperado);

            // Act
            var resultado = await _service.GetCuidadorAtual();

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(idEsperado, resultado?.Id);
            Assert.Equal("Ronaldo", resultado?.Nome);
            _dbMock.Verify(x => x.BuscarPrimeiroAsync<Cuidador>(It.IsAny<System.Linq.Expressions.Expression<Func<Cuidador, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task GetCuidadorAtual_QuandoNaoHouverUsuario_DeveRetornarNulo()
        {
            _dbMock.Setup(x => x.BuscarPrimeiroAsync<Cuidador>())
                   .ReturnsAsync((Cuidador?)null);

            var resultado = await _service.GetCuidadorAtual();

            Assert.Null(resultado);
        }

        #endregion

        #region ValidarEmail

        [Fact]
        public async Task ValidarEmail_QuandoEmailJaCadastrado_DeveRetornarTrue()
        {
            string emailExistente = "ja_existe@teste.com";
            var usuarioDoBanco = new Cuidador { Id = 1, Email = emailExistente };
            _dbMock.Setup(x => x.BuscarPrimeiroAsync<Cuidador>(It.IsAny<Expression<Func<Cuidador, bool>>>()))
                   .ReturnsAsync(usuarioDoBanco);

            var resultado = await _service.ValidarEmail(emailExistente);

            Assert.True(resultado);
        }

        [Fact]
        public async Task ValidarEmail_QuandoEmailNaoCadastrado_DeveRetornarFalse()
        {
            string emailLivre = "livre@teste.com";
            _dbMock.Setup(x => x.BuscarPrimeiroAsync<Cuidador>(It.IsAny<Expression<Func<Cuidador, bool>>>()))
                   .ReturnsAsync((Cuidador?)null);

            var resultado = await _service.ValidarEmail(emailLivre);

            Assert.False(resultado);
        }

        [Fact]
        public async Task ValidarEmail_ComEmailVazioOuNulo_DeveRetornarFalse()
        {
            string emailInvalido = "";

            var resultado = await _service.ValidarEmail(emailInvalido);

            Assert.False(resultado);
            _dbMock.Verify(x => x.BuscarPrimeiroAsync<Cuidador>(It.IsAny<Expression<Func<Cuidador, bool>>>()), Times.Never);
        }

        #endregion
    }
}