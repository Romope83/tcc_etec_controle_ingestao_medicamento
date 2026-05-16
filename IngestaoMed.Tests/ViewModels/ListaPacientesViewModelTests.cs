using Moq;
using Xunit;
using IngestaoMed.Core.ViewModels;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace IngestaoMed.Tests.ViewModels
{
    public class ListaPacientesViewModelTests
    {
        private readonly Mock<IPacienteService> _serviceMock;
        private readonly Mock<INavigationService> _navigationMock;
        private readonly Mock<IDialogService> _dialogMock;
        private readonly ListaPacientesViewModel _viewModel;

        public ListaPacientesViewModelTests()
        {
            _serviceMock = new Mock<IPacienteService>();
            _navigationMock = new Mock<INavigationService>();
            _dialogMock = new Mock<IDialogService>();

            _viewModel = new ListaPacientesViewModel(
                _serviceMock.Object,
                _navigationMock.Object,
                _dialogMock.Object);
        }

        [Fact]
        public async Task CarregarPacientesAsync_DevePreencherLista_QuandoHouverDados()
        {
            // Arrange
            var pacientesFake = new List<Paciente>
            {
                new Paciente { Nome = "João" },
                new Paciente { Nome = "Maria" }
            };
            _serviceMock.Setup(s => s.ObterTodosAsync()).ReturnsAsync(pacientesFake);

            // Act
            await _viewModel.CarregarPacientesCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(2, _viewModel.Pacientes.Count);
            _serviceMock.Verify(s => s.ObterTodosAsync(), Times.Once);
        }

        [Fact]
        public async Task NavegarParaCadastroAsync_DeveChamarNavegacaoComRotaCorreta()
        {
            // Act
            await _viewModel.NavegarParaCadastroCommand.ExecuteAsync(null);

            // Assert
            _navigationMock.Verify(n => n.GoToAsync("CadastroPacientePage"), Times.Once);
        }

        [Fact]
        public async Task SelecionarPacienteAsync_DeveNavegar_QuandoIdForValido()
        {
            // Act
            await _viewModel.SelecionarPacienteCommand.ExecuteAsync(10);

            // Assert
            _navigationMock.Verify(n => n.GoToAsync("PacienteDetalhesPage?id=10"), Times.Once);
        }

        [Fact]
        public async Task SelecionarPacienteAsync_NaoDeveNavegar_QuandoIdForInvalido()
        {
            // Act
            await _viewModel.SelecionarPacienteCommand.ExecuteAsync(0);

            // Assert
            _navigationMock.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RemoverPacienteAsync_DeveRemoverDaLista_QuandoConfirmadoESucessoNoService()
        {
            // Arrange
            var paciente = new Paciente { Nome = "Teste" };
            _viewModel.Pacientes.Add(paciente);

            _dialogMock.Setup(d => d.DisplayConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), "Sim", "Não"))
                       .ReturnsAsync(true);
            _serviceMock.Setup(s => s.RemoverPacienteAsync(paciente)).ReturnsAsync(true);

            // Act
            await _viewModel.RemoverPacienteCommand.ExecuteAsync(paciente);

            // Assert
            Assert.Empty(_viewModel.Pacientes);
            _serviceMock.Verify(s => s.RemoverPacienteAsync(paciente), Times.Once);
        }

        [Fact]
        public async Task RemoverPacienteAsync_NaoDeveRemover_QuandoUsuarioCancelar()
        {
            // Arrange
            var paciente = new Paciente { Nome = "Teste" };
            _viewModel.Pacientes.Add(paciente);

            _dialogMock.Setup(d => d.DisplayConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), "Sim", "Não"))
                       .ReturnsAsync(false);

            // Act
            await _viewModel.RemoverPacienteCommand.ExecuteAsync(paciente);

            // Assert
            Assert.Single(_viewModel.Pacientes);
            _serviceMock.Verify(s => s.RemoverPacienteAsync(It.IsAny<Paciente>()), Times.Never);
        }

        [Theory]
        [InlineData(FiltroPaciente.Ativos)]
        [InlineData(FiltroPaciente.Idosos)]
        public async Task AlterarFiltroAsync_DeveChamarMetodoCorretoDoService(FiltroPaciente filtro)
        {
            // Arrange
            _serviceMock.Setup(s => s.BuscarComTratamentoAtivoAsync()).ReturnsAsync(new List<Paciente>());
            _serviceMock.Setup(s => s.BuscarPacientesIdososAsync()).ReturnsAsync(new List<Paciente>());

            // Act
            await _viewModel.AlterarFiltroCommand.ExecuteAsync(filtro);

            // Assert
            if (filtro == FiltroPaciente.Ativos)
                _serviceMock.Verify(s => s.BuscarComTratamentoAtivoAsync(), Times.Once);
            else
                _serviceMock.Verify(s => s.BuscarPacientesIdososAsync(), Times.Once);
        }

        [Fact]
        public async Task OnTextoBuscaChanged_DeveFiltrarListaPorNome()
        {
            // Arrange
            var listaGeral = new List<Paciente>
            {
                new Paciente { Nome = "Ronaldo" },
                new Paciente { Nome = "Carlos" }
            };
            _serviceMock.Setup(s => s.ObterTodosAsync()).ReturnsAsync(listaGeral);

            // Act
            _viewModel.TextoBusca = "Ron";
            await Task.Delay(100); // Aguarda o processamento da Task assíncrona disparada pelo partial method

            // Assert
            Assert.Single(_viewModel.Pacientes);
            Assert.Contains(_viewModel.Pacientes, p => p.Nome == "Ronaldo");
        }
    }
}