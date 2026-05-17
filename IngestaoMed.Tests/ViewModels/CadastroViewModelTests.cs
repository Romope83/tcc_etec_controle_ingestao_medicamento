using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.ViewModels;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IngestaoMed.Tests.ViewModels
{
    public class CadastroViewModelTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IDialogService> _dialogServiceMock;
        private readonly Mock<IConfigService> _configServiceMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly CadastroViewModel _viewModel;

        public CadastroViewModelTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _dialogServiceMock = new Mock<IDialogService>();
            _configServiceMock = new Mock<IConfigService>();
            _navigationServiceMock = new Mock<INavigationService>();

            _viewModel = new CadastroViewModel(
                _authServiceMock.Object,
                _dialogServiceMock.Object,
                _configServiceMock.Object,
                _navigationServiceMock.Object);
        }

        [Theory]
        [InlineData("", "cuidador@email.com", "123456")]
        [InlineData("Nome Cuidador", "", "123456")]
        [InlineData("Nome Cuidador", "cuidador@email.com", "")]
        public async Task SalvarCadastro_CamposObrigatoriosVazios_DeveExibirAlertaEInterromper(string nome, string email, string senha)
        {
            // Arrange
            _viewModel.Nome = nome;
            _viewModel.Email = email;
            _viewModel.Senha = senha;

            // Act
            await _viewModel.SalvarCadastroCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Campos Obrigatórios", "Por favor, preencha nome, e-mail e senha.", "OK"), Times.Once);
            _authServiceMock.Verify(a => a.RegistrarCuidador(It.IsAny<Cuidador>(), It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("emailinvalido")]
        [InlineData("email@")]
        [InlineData("email@dominio")]
        public async Task SalvarCadastro_FormatoEmailInvalido_DeveExibirAlertaEInterromper(string emailInvalido)
        {
            // Arrange
            _viewModel.Nome = "Ronaldo";
            _viewModel.Email = emailInvalido;
            _viewModel.Senha = "123456";

            // Act
            await _viewModel.SalvarCadastroCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("E-mail Inválido", "Por favor, insira um endereço de e-mail válido.", "OK"), Times.Once);
            _authServiceMock.Verify(a => a.RegistrarCuidador(It.IsAny<Cuidador>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SalvarCadastro_SenhaCurta_DeveExibirAlertaEInterromper()
        {
            // Arrange
            _viewModel.Nome = "Ronaldo";
            _viewModel.Email = "ronaldo@email.com";
            _viewModel.Senha = "12345";

            // Act
            await _viewModel.SalvarCadastroCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Senha Curta", "A senha deve conter no mínimo 6 caracteres para sua segurança.", "OK"), Times.Once);
            _authServiceMock.Verify(a => a.RegistrarCuidador(It.IsAny<Cuidador>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SalvarCadastro_EmailJaExisteNoBanco_DeveExibirAlertaEInterromper()
        {
            // Arrange
            _viewModel.Nome = "Ronaldo";
            _viewModel.Email = "ronaldo@email.com";
            _viewModel.Senha = "123456";

            _authServiceMock.Setup(a => a.ValidarEmail(_viewModel.Email)).ReturnsAsync(true);

            // Act
            await _viewModel.SalvarCadastroCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("E-mail em uso", "Este e-mail já está cadastrado. Tente recuperar sua senha ou use outro e-mail.", "OK"), Times.Once);
            _authServiceMock.Verify(a => a.RegistrarCuidador(It.IsAny<Cuidador>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SalvarCadastro_FalhaNoServicoDeRegistro_DeveExibirMensagemDeErro()
        {
            // Arrange
            _viewModel.Nome = "Ronaldo";
            _viewModel.Email = "ronaldo@email.com";
            _viewModel.Senha = "123456";
            _viewModel.Telefone = "11999999999";

            _authServiceMock.Setup(a => a.ValidarEmail(_viewModel.Email)).ReturnsAsync(false);
            _authServiceMock.Setup(a => a.RegistrarCuidador(It.IsAny<Cuidador>(), _viewModel.Senha)).ReturnsAsync(false);

            // Act
            await _viewModel.SalvarCadastroCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Erro", "Não foi possível realizar o cadastro. Tente novamente mais tarde.", "OK"), Times.Once);
            _navigationServiceMock.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SalvarCadastro_Sucesso_DeveAtualizarConfiguracoesExibirAlertaENavegar()
        {
            // Arrange
            _viewModel.Nome = "Ronaldo";
            _viewModel.Email = "ronaldo@email.com";
            _viewModel.Senha = "123456";
            _viewModel.Telefone = "11999999999";

            _authServiceMock.Setup(a => a.ValidarEmail(_viewModel.Email)).ReturnsAsync(false);
            _authServiceMock.Setup(a => a.RegistrarCuidador(It.IsAny<Cuidador>(), _viewModel.Senha)).ReturnsAsync(true);

            // Act
            await _viewModel.SalvarCadastroCommand.ExecuteAsync(null);

            // Assert
            _authServiceMock.Verify(a => a.RegistrarCuidador(It.Is<Cuidador>(c =>
                c.Nome == "Ronaldo" &&
                c.Email == "ronaldo@email.com" &&
                c.Telefone == "11999999999"
            ), _viewModel.Senha), Times.Once);

            _configServiceMock.VerifySet(c => c.EhPrimeiroAcesso = false, Times.Once);
            _configServiceMock.VerifySet(c => c.EmailCuidadorConfigurado = "ronaldo@email.com", Times.Once);

            _dialogServiceMock.Verify(d => d.DisplayAlert("Sucesso", "Perfil configurado com sucesso!", "OK"), Times.Once);
            _navigationServiceMock.Verify(n => n.GoToAsync("//ListaPacientePage"), Times.Once);
        }
    }
}