using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.ViewModels;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace IngestaoMed.Tests.ViewModels
{
    public class LoginViewModelTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IDialogService> _dialogServiceMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly LoginViewModel _viewModel;

        public LoginViewModelTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _dialogServiceMock = new Mock<IDialogService>();
            _navigationServiceMock = new Mock<INavigationService>();

            _viewModel = new LoginViewModel(
                _authServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object);
        }

        #region Construtor

        [Fact]
        public void Construtor_DeveInicializarComEstadoVisualPadrao()
        {
            // Arrange & Act

            // Assert
            Assert.Empty(_viewModel.Email);
            Assert.Empty(_viewModel.Senha);
            Assert.False(_viewModel.ExibirCamposLogin);
            Assert.True(_viewModel.ExibirOpcoesPerfil);
        }

        #endregion

        #region NavegarParaPacienteAsync

        [Fact]
        public async Task NavegarParaPacienteCommand_Sempre_DeveNavegarParaPaginaPacientes()
        {
            // Arrange & Act
            await _viewModel.NavegarParaPacienteCommand.ExecuteAsync(null);

            // Assert
            _navigationServiceMock.Verify(n => n.GoToAsync("PacientesPage"), Times.Once);
        }

        #endregion

        #region MostrarLoginCuidador

        [Fact]
        public void MostrarLoginCuidadorCommand_Sempre_DeveAlternarEstadosVisuais()
        {
            // Arrange
            _viewModel.ExibirOpcoesPerfil = true;
            _viewModel.ExibirCamposLogin = false;

            // Act
            _viewModel.MostrarLoginCuidadorCommand.Execute(null);

            // Assert
            Assert.False(_viewModel.ExibirOpcoesPerfil);
            Assert.True(_viewModel.ExibirCamposLogin);
        }

        #endregion

        #region VoltarParaPerfil

        [Fact]
        public void VoltarParaPerfilCommand_Sempre_DeveLimparCamposERetornarEstadoVisual()
        {
            // Arrange
            _viewModel.Email = "cuidador@teste.com";
            _viewModel.Senha = "123456";
            _viewModel.ExibirOpcoesPerfil = false;
            _viewModel.ExibirCamposLogin = true;

            // Act
            _viewModel.VoltarParaPerfilCommand.Execute(null);

            // Assert
            Assert.Empty(_viewModel.Email);
            Assert.Empty(_viewModel.Senha);
            Assert.True(_viewModel.ExibirOpcoesPerfil);
            Assert.False(_viewModel.ExibirCamposLogin);
        }

        #endregion

        #region EntrarAsync

        [Theory]
        [InlineData("", "123")]
        [InlineData("cuidador@teste.com", "")]
        [InlineData(" ", " ")]
        public async Task EntrarCommand_CamposVaziosOuNulos_DeveExibirAlertaEInterromper(string email, string senha)
        {
            // Arrange
            _viewModel.Email = email;
            _viewModel.Senha = senha;

            // Act
            await _viewModel.EntrarCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Erro", "Preencha todos os campos.", "OK"), Times.Once);
            _authServiceMock.Verify(a => a.ValidarLogin(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task EntrarCommand_LoginComSucesso_DeveNavegarParaListaPacientes()
        {
            // Arrange
            _viewModel.Email = "cuidador@teste.com";
            _viewModel.Senha = "senha123";
            _authServiceMock.Setup(a => a.ValidarLogin("cuidador@teste.com", "senha123")).ReturnsAsync(true);

            // Act
            await _viewModel.EntrarCommand.ExecuteAsync(null);

            // Assert
            _navigationServiceMock.Verify(n => n.GoToAsync("//ListaPacientesPage"), Times.Once);
            _dialogServiceMock.Verify(d => d.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task EntrarCommand_LoginInvalido_DeveExibirAlertaDeErro()
        {
            // Arrange
            _viewModel.Email = "errado@teste.com";
            _viewModel.Senha = "senhaIncorreta";
            _authServiceMock.Setup(a => a.ValidarLogin("errado@teste.com", "senhaIncorreta")).ReturnsAsync(false);

            // Act
            await _viewModel.EntrarCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Erro", "E-mail ou senha inválidos.", "OK"), Times.Once);
            _navigationServiceMock.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion
    }
}