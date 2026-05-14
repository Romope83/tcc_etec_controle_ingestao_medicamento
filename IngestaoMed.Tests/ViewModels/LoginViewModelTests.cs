using Moq;
using Xunit;
using IngestaoMed.Core.ViewModels;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Interfaces;

namespace IngestaoMed.Tests.ViewModels
{
    public class LoginViewModelTests
    {
        private readonly Mock<IAuthService> _authMock;
        private readonly Mock<IDialogService> _dialogMock;
        private readonly Mock<INavigationService> _navigationMock;
        private readonly LoginViewModel _viewModel;

        public LoginViewModelTests()
        {
            _authMock = new Mock<IAuthService>();
            _dialogMock = new Mock<IDialogService>();
            _navigationMock = new Mock<INavigationService>();
            _viewModel = new LoginViewModel(_authMock.Object, _dialogMock.Object, _navigationMock.Object);
        }

        [Fact]
        public async Task Entrar_DeveNavegarParaMainPage_QuandoLoginForSucesso()
        {
            // Arrange
            _viewModel.Email = "admin@teste.com";
            _viewModel.Senha = "123456";

            _authMock.Setup(a => a.ValidarLogin(_viewModel.Email, _viewModel.Senha))
                     .ReturnsAsync(true);

            // Act
            await _viewModel.EntrarCommand.ExecuteAsync(null);

            // Assert
            // Verifica se o método de navegação foi chamado com a rota correta
            _navigationMock.Verify(n => n.GoToAsync("//ListaPacientePage"), Times.Once);

            // Garante que nenhum alerta de erro foi exibido
            _dialogMock.Verify(d => d.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Entrar_NaoDeveNavegar_QuandoLoginFalhar()
        {
            // Arrange
            _viewModel.Email = "usuario@invalido.com";
            _viewModel.Senha = "000";

            _authMock.Setup(a => a.ValidarLogin(It.IsAny<string>(), It.IsAny<string>()))
                     .ReturnsAsync(false);

            // Act
            await _viewModel.EntrarCommand.ExecuteAsync(null);

            // Assert
            _navigationMock.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);

            _dialogMock.Verify(d => d.DisplayAlert("Erro", "E-mail ou senha inválidos.", "OK"), Times.Once);
        }

        [Fact]
        public async Task Entrar_DeveExibirErro_QuandoCamposEstaoVazios()
        {
            // Arrange
            _viewModel.Email = "";
            _viewModel.Senha = "";

            // Act
            await _viewModel.EntrarCommand.ExecuteAsync(null);

            // Assert
            _dialogMock.Verify(d => d.DisplayAlert("Erro", "Preencha todos os campos.", "OK"), Times.Once);
            _authMock.Verify(a => a.ValidarLogin(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Entrar_DeveExibirErro_QuandoCredenciaisSaoInvalidas()
        {
            // Arrange
            _viewModel.Email = "errado@teste.com";
            _viewModel.Senha = "123";

            _authMock.Setup(a => a.ValidarLogin(_viewModel.Email, _viewModel.Senha))
                     .ReturnsAsync(false);

            // Act
            await _viewModel.EntrarCommand.ExecuteAsync(null);

            // Assert
            _dialogMock.Verify(d => d.DisplayAlert("Erro", "E-mail ou senha inválidos.", "OK"), Times.Once);
        }

        [Fact]
        public async Task Entrar_DeveChamarValidarLogin_ComDadosCorretos()
        {
            // Arrange
            string emailTeste = "usuario@teste.com";
            string senhaTeste = "Senha123";

            _viewModel.Email = emailTeste;
            _viewModel.Senha = senhaTeste;

            _authMock.Setup(a => a.ValidarLogin(emailTeste, senhaTeste))
                     .ReturnsAsync(true);

            // Act
            await _viewModel.EntrarCommand.ExecuteAsync(null);

            // Assert
            _authMock.Verify(a => a.ValidarLogin(emailTeste, senhaTeste), Times.Once);
        }
    }
}