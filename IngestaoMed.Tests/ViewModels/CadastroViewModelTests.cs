using Moq;
using Xunit;
using IngestaoMed.Core.ViewModels;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;

namespace IngestaoMed.Tests.ViewModels
{
    public class CadastroViewModelTests
    {
        private readonly Mock<IAuthService> _authMock;
        private readonly Mock<IDialogService> _dialogMock;
        private readonly CadastroViewModel _viewModel;

        public CadastroViewModelTests()
        {
            _authMock = new Mock<IAuthService>();
            _dialogMock = new Mock<IDialogService>();
            _viewModel = new CadastroViewModel(_authMock.Object, _dialogMock.Object);
        }

        [Fact]
        public async Task SalvarCadastro_DeveExibirErro_QuandoCamposEstaoVazios()
        {
            // Arrange
            _viewModel.Nome = "";
            _viewModel.Email = "";

            // Act
            await _viewModel.SalvarCadastroCommand.ExecuteAsync(null);

            // Assert
            _dialogMock.Verify(d => d.DisplayAlert("Erro", It.IsAny<string>(), "OK"), Times.Once);
        }

        [Theory]
        [InlineData("emailinvalido")]
        [InlineData("usuario@")]
        [InlineData("@dominio.com")]
        public async Task SalvarCadastro_DeveExibirErro_QuandoEmailForInvalido(string emailIncorreto)
        {
            // Arrange
            _viewModel.Nome = "Teste";
            _viewModel.Email = emailIncorreto;
            _viewModel.Senha = "123456";

            // Act
            await _viewModel.SalvarCadastroCommand.ExecuteAsync(null);

            // Assert
            _dialogMock.Verify(d => d.DisplayAlert("E-mail Inválido", It.IsAny<string>(), "OK"), Times.Once);
        }

        [Fact]
        public async Task SalvarCadastro_DeveExibirSucesso_QuandoDadosForemValidos()
        {
            // Arrange
            _viewModel.Nome = "João Silva";
            _viewModel.Email = "joao@email.com";
            _viewModel.Senha = "Senha@123";

            // Simula que o e-mail não existe no banco e o registro funciona
            _authMock.Setup(a => a.ValidarEmail(It.IsAny<string>())).ReturnsAsync(false);
            _authMock.Setup(a => a.RegistrarCuidador(It.IsAny<Cuidador>(), It.IsAny<string>())).ReturnsAsync(true);

            // Act
            await _viewModel.SalvarCadastroCommand.ExecuteAsync(null);

            // Assert
            _dialogMock.Verify(d => d.DisplayAlert("Sucesso", It.IsAny<string>(), "OK"), Times.Once);
        }
    }
}