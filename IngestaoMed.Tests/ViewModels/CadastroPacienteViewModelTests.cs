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
    public class CadastroPacienteViewModelTests
    {
        private readonly Mock<IPacienteService> _pacienteServiceMock;
        private readonly Mock<INavigationService> _navigationMock;
        private readonly Mock<IDialogService> _dialogMock;
        private readonly Mock<IAuthService> _authMock;
        private readonly PacienteViewModel _viewModel;



        [Fact]
        public void OnTelefoneChanged_DeveFormatarComMascara_QuandoReceberOnzeDigitos()
        {
            // Act
            _viewModel.Telefone = "11988887777";

            // Assert - Verifica se a lógica de Regex formatou corretamente
            Assert.Equal("(11) 98888-7777", _viewModel.Telefone);
        }

        [Fact]
        public async Task SalvarAsync_DeveFalhar_QuandoNomeEstiverVazio()
        {
            // Arrange
            _viewModel.Nome = string.Empty;

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _dialogMock.Verify(d => d.DisplayAlert("Erro", "O nome é obrigatório.", "OK"), Times.Once);
            _pacienteServiceMock.Verify(s => s.SalvarOuAtualizarPacienteAsync(It.IsAny<Paciente>()), Times.Never);
        }

        [Fact]
        public async Task SalvarAsync_DeveExibirErro_QuandoAuthServiceIndicarQueEmailJaExiste()
        {
            // Arrange
            _viewModel.Nome = "Ronaldo";
            _viewModel.Email = "existente@teste.com";

            _authMock.Setup(a => a.ValidarEmail(_viewModel.Email)).ReturnsAsync(true);

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _dialogMock.Verify(d => d.DisplayAlert("Erro", "Este e-mail já está cadastrado.", "OK"), Times.Once);
            _pacienteServiceMock.Verify(s => s.SalvarOuAtualizarPacienteAsync(It.IsAny<Paciente>()), Times.Never);
        }

        [Fact]
        public async Task SalvarAsync_DeveSalvarENavegar_QuandoDadosForemValidosEServiceRetornarTrue()
        {
            // Arrange
            _viewModel.Nome = "Paciente Valido";
            _viewModel.Email = "novo@teste.com";
            _viewModel.DataNascimento = new DateTime(1990, 1, 1);

            _authMock.Setup(a => a.ValidarEmail(It.IsAny<string>())).ReturnsAsync(false);

            _pacienteServiceMock.Setup(s => s.SalvarOuAtualizarPacienteAsync(It.IsAny<Paciente>()))
                                .ReturnsAsync(true);

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _pacienteServiceMock.Verify(s => s.SalvarOuAtualizarPacienteAsync(It.Is<Paciente>(p =>
                p.Nome == _viewModel.Nome &&
                p.Email == _viewModel.Email &&
                p.DataNascimento == _viewModel.DataNascimento
            )), Times.Once);

            _dialogMock.Verify(d => d.DisplayAlert("Sucesso", "Paciente cadastrado com sucesso!", "OK"), Times.Once);
            _navigationMock.Verify(n => n.GoToAsync(".."), Times.Once);
        }

        [Theory]
        [InlineData("emailinvalido")]
        [InlineData("usuario@")]
        [InlineData("@dominio.com")]
        public async Task SalvarAsync_DeveValidarFormatoDeEmailIncorreto(string emailInvalido)
        {
            // Arrange
            _viewModel.Nome = "Teste";
            _viewModel.Email = emailInvalido;

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _dialogMock.Verify(d => d.DisplayAlert("E-mail Inválido", It.IsAny<string>(), "OK"), Times.Once);
            _pacienteServiceMock.Verify(s => s.SalvarOuAtualizarPacienteAsync(It.IsAny<Paciente>()), Times.Never);
        }
    }
}