using Moq;
using Xunit;
using IngestaoMed.Core.ViewModels;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Threading.Tasks;

namespace IngestaoMed.Tests.ViewModels
{
    public class CuidadorViewModelTests
    {
        private readonly Mock<INavigationService> _navegacaoMock;
        private readonly Mock<IConfigService> _configuracaoMock;
        private readonly Mock<ICuidadorService> _cuidadorServiceMock;
        private readonly Mock<IDialogService> _dialogServiceMock;
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly CuidadorViewModel _viewModel;

        public CuidadorViewModelTests()
        {
            _navegacaoMock = new Mock<INavigationService>();
            _configuracaoMock = new Mock<IConfigService>();
            _cuidadorServiceMock = new Mock<ICuidadorService>();
            _dialogServiceMock = new Mock<IDialogService>();

            _viewModel = new CuidadorViewModel(
                _navegacaoMock.Object,
                _configuracaoMock.Object,
                _cuidadorServiceMock.Object,
                _dialogServiceMock.Object);
        }

        #region InicializarAsync

        #region InicializarAsync

        [Fact]
        public async Task InicializarAsync_ModoCadastro_DeveConfigurarEstadoInicial()
        {
            // Arrange
            _configuracaoMock.Setup(c => c.ConfiguracaoCuidador).Returns((ConfiguracaoCuidador?)null);

            // Act
            await _viewModel.InicializarAsync();

            // Assert
            Assert.False(_viewModel.EhEdicao);
            Assert.Equal("CONCLUIR CADASTRO", _viewModel.TextoBotaoAcao);
            Assert.Empty(_viewModel.NomeCuidador);
        }

        [Fact]
        public async Task InicializarAsync_ModoEdicao_DeveConfigurarEstadoParaEdicao()
        {
            // Arrange
            var configFake = new ConfiguracaoCuidador { Id = 1 };
            var cuidadorBanco = new Cuidador { Id = 1, Nome = "Ronaldo", Email = "ronaldo@teste.com", Telefone = "11999999999" };

            _configuracaoMock.Setup(c => c.ConfiguracaoCuidador).Returns(configFake);
            _cuidadorServiceMock.Setup(s => s.ObterPorIdAsync(1)).ReturnsAsync(cuidadorBanco);

            // Act
            await _viewModel.InicializarAsync();

            // Assert
            Assert.True(_viewModel.EhEdicao);
            Assert.Equal("SALVAR ALTERAÇÕES", _viewModel.TextoBotaoAcao);
            Assert.Equal("Ronaldo", _viewModel.NomeCuidador);
            Assert.Equal("ronaldo@teste.com", _viewModel.EmailCuidador);
        }

        #endregion

        #endregion

        #region Validacoes do Formulario

        [Fact]
        public async Task SalvarCuidadorCommand_NomeVazio_DeveExibirAlertaEInterromper()
        {
            // Arrange
            _viewModel.NomeCuidador = "";

            // Act
            await _viewModel.SalvarCuidadorCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Campo Obrigatório", "Por favor, insira o nome do cuidador.", "OK"), Times.Once);
            _cuidadorServiceMock.Verify(s => s.SalvarCuidadorAsync(It.IsAny<Cuidador>()), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SalvarCuidadorCommand_TelefoneVazio_DeveExibirAlertaEInterromper(string telefone)
        {
            // Arrange
            _viewModel.NomeCuidador = "Ronaldo Moreira";
            _viewModel.TelefoneCuidador = telefone;

            // Act
            await _viewModel.SalvarCuidadorCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Campo Obrigatório", "Por favor, insira o telefone do cuidador.", "OK"), Times.Once);
            _cuidadorServiceMock.Verify(s => s.SalvarCuidadorAsync(It.IsAny<Cuidador>()), Times.Never);
        }

        [Theory]
        [InlineData("119999")]
        [InlineData("(11) 9999-999")]
        public async Task SalvarCuidadorCommand_TelefoneMenorQue11Digitos_DeveExibirAlertaEInterromper(string telefone)
        {
            // Arrange
            _viewModel.NomeCuidador = "Ronaldo Moreira";
            _viewModel.TelefoneCuidador = telefone;

            // Act
            await _viewModel.SalvarCuidadorCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Telefone Inválido", "O número de telefone deve conter no mínimo 11 dígitos (DDD + Número).", "OK"), Times.Once);
            _cuidadorServiceMock.Verify(s => s.SalvarCuidadorAsync(It.IsAny<Cuidador>()), Times.Never);
        }

        [Fact]
        public async Task SalvarCuidadorCommand_EmailVazio_DeveExibirAlertaEInterromper()
        {
            // Arrange
            _viewModel.NomeCuidador = "Ronaldo Moreira";
            _viewModel.TelefoneCuidador = "11999999999";
            _viewModel.EmailCuidador = "";

            // Act
            await _viewModel.SalvarCuidadorCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Campo Obrigatório", "Por favor, insira o e-mail do cuidador.", "OK"), Times.Once);
            _cuidadorServiceMock.Verify(s => s.SalvarCuidadorAsync(It.IsAny<Cuidador>()), Times.Never);
        }

        [Theory]
        [InlineData("emailInvalido")]
        [InlineData("cuidador@")]
        [InlineData("cuidador@com")]
        public async Task SalvarCuidadorCommand_EmailFormatoInvalido_DeveExibirAlertaEInterromper(string email)
        {
            // Arrange
            _viewModel.NomeCuidador = "Ronaldo Moreira";
            _viewModel.TelefoneCuidador = "11999999999";
            _viewModel.EmailCuidador = email;

            // Act
            await _viewModel.SalvarCuidadorCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("E-mail Inválido", "Por favor, insira um endereço de e-mail válido.", "OK"), Times.Once);
            _cuidadorServiceMock.Verify(s => s.SalvarCuidadorAsync(It.IsAny<Cuidador>()), Times.Never);
        }

        [Theory]
        [InlineData("", "123")]
        [InlineData("123", "")]
        public async Task SalvarCuidadorCommand_SenhaOuConfirmacaoVazios_DeveExibirAlertaEInterromper(string senha, string confirmacao)
        {
            // Arrange
            _viewModel.NomeCuidador = "Ronaldo Moreira";
            _viewModel.TelefoneCuidador = "11999999999";
            _viewModel.EmailCuidador = "cuidador@teste.com";
            _viewModel.SenhaCuidador = senha
            ;
            _viewModel.ConfirmacaoSenhaCuidador =confirmacao
            ;

            // Act
            await _viewModel.SalvarCuidadorCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Campo Obrigatório", "Por favor, preencha a senha e a confirmação de senha.", "OK"), Times.Once);
            _cuidadorServiceMock.Verify(s => s.SalvarCuidadorAsync(It.IsAny<Cuidador>()), Times.Never);
        }

        [Fact]
        public async Task SalvarCuidadorCommand_SenhasDiferentes_DeveExibirAlertaEInterromper()
        {
            // Arrange
            _viewModel.NomeCuidador = "Ronaldo Moreira";
            _viewModel.TelefoneCuidador = "11999999999";
            _viewModel.EmailCuidador = "cuidador@teste.com";
            _viewModel.SenhaCuidador = "senha123";
            _viewModel.ConfirmacaoSenhaCuidador = "senha321";

            // Act
            await _viewModel.SalvarCuidadorCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Senhas Diferem", "A senha informada e a confirmação não coincidem. Verifique e tente novamente.", "OK"), Times.Once);
            _cuidadorServiceMock.Verify(s => s.SalvarCuidadorAsync(It.IsAny<Cuidador>()), Times.Never);
        }

        #endregion

        #region Fluxos de Persistencia (Sucesso e Falha)

        [Fact]
        public async Task SalvarCuidadorCommand_SucessoCadastro_DeveSalvarConfiguracoesENavegar()
        {
            // Arrange
            await _viewModel.InicializarAsync();
            _viewModel.NomeCuidador = "Ronaldo Moreira";
            _viewModel.TelefoneCuidador = "(11) 98888-8888";
            _viewModel.EmailCuidador = "cuidador@teste.com";
            _viewModel.SenhaCuidador = "senha123";
            _viewModel.ConfirmacaoSenhaCuidador = "senha123";

            _cuidadorServiceMock.Setup(s => s.SalvarCuidadorAsync(It.IsAny<Cuidador>())).ReturnsAsync(true);

            // Act
            await _viewModel.SalvarCuidadorCommand.ExecuteAsync(null);

            // Assert
            _cuidadorServiceMock.Verify(s => s.SalvarCuidadorAsync(It.Is<Cuidador>(c =>
                c.Nome == "Ronaldo Moreira" &&
                c.Telefone == "11988888888" &&
                c.Email == "cuidador@teste.com" &&
                c.Id == 0
            )), Times.Once);

            _configuracaoMock.VerifySet(c => c.EmailCuidadorConfigurado = "cuidador@teste.com", Times.Once);
            _configuracaoMock.VerifySet(c => c.EhPrimeiroAcesso = false, Times.Once);
            _configuracaoMock.VerifySet(c => c.ConfiguracaoCuidador = It.IsAny<ConfiguracaoCuidador>(), Times.Once);

            _dialogServiceMock.Verify(d => d.DisplayAlert("Sucesso", "Cadastro concluído com sucesso!", "OK"), Times.Once);
            _navegacaoMock.Verify(n => n.GoToAsync("//ListaPacientesPage"), Times.Once);
        }

        [Fact]
        public async Task SalvarCuidadorCommand_SucessoEdicao_DeveExibirMensagemAlertaEspecifica()
        {
            // Arrange
            var configFake = new ConfiguracaoCuidador { Id = 1 };
            var cuidadorBanco = new Cuidador { Id = 1, Nome = "Ronaldo", Email = "cuidador@edicao.com", Telefone = "11988888888" };

            _configuracaoMock.Setup(c => c.ConfiguracaoCuidador).Returns(configFake);
            _cuidadorServiceMock.Setup(s => s.ObterPorIdAsync(1)).ReturnsAsync(cuidadorBanco);
            _cuidadorServiceMock.Setup(s => s.SalvarCuidadorAsync(It.IsAny<Cuidador>())).ReturnsAsync(true);

            // Garante que a ViewModel carregue o estado de edição antes de salvar
            await _viewModel.InicializarAsync();

            _viewModel.SenhaCuidador = "senha123";
            _viewModel.ConfirmacaoSenhaCuidador = "senha123";

            // Act
            await _viewModel.SalvarCuidadorCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Sucesso", "Cadastro atualizado com sucesso!", "OK"), Times.Once);
            _navegacaoMock.Verify(n => n.GoToAsync("//ListaPacientesPage"), Times.Once);
        }

        [Fact]
        public async Task SalvarCuidadorCommand_FalhaNoServico_DeveExibirMensagemErroENaoNavegar()
        {
            // Arrange
            _viewModel.NomeCuidador = "Ronaldo Moreira";
            _viewModel.TelefoneCuidador = "11988888888";
            _viewModel.EmailCuidador = "cuidador@falha.com";
            _viewModel.SenhaCuidador = "senha123";
            _viewModel.ConfirmacaoSenhaCuidador = "senha123";

            _cuidadorServiceMock.Setup(s => s.SalvarCuidadorAsync(It.IsAny<Cuidador>())).ReturnsAsync(false);

            // Act
            await _viewModel.SalvarCuidadorCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Erro", "Não foi possível salvar os dados do cuidador.", "OK"), Times.Once);
            _configuracaoMock.VerifySet(c => c.EhPrimeiroAcesso = It.IsAny<bool>(), Times.Never);
            _navegacaoMock.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion
    }
}