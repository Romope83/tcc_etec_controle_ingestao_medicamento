using Moq;
using Xunit;
using IngestaoMed.Core.ViewModels;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace IngestaoMed.Tests.ViewModels
{
    public class ListaPacientesViewModelTests
    {
        private readonly Mock<IPacienteService> _serviceMock;
        private readonly Mock<INavigationService> _navMock;
        private readonly Mock<IDialogService> _dialogMock;
        private readonly ListaPacientesViewModel _viewModel;

        public ListaPacientesViewModelTests()
        {
            _serviceMock = new Mock<IPacienteService>();
            _navMock = new Mock<INavigationService>();
            _dialogMock = new Mock<IDialogService>();

            _viewModel = new ListaPacientesViewModel(
                _serviceMock.Object,
                _navMock.Object,
                _dialogMock.Object);
        }

        #region CarregarPacientesAsync

        [Fact]
        public async Task CarregarPacientesAsync_Sucesso_DevePopularPacientes()
        {
            // Arrange
            var lista = new List<Paciente>
            {
                new Paciente { Id = 1, Nome = "Ronaldo" },
                new Paciente { Id = 2, Nome = "Moreira" }
            };
            _serviceMock.Setup(s => s.ObterTodosAsync()).ReturnsAsync(lista);

            // Act
            await _viewModel.CarregarPacientesCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(2, _viewModel.Pacientes.Count);
            _serviceMock.Verify(s => s.ObterTodosAsync(), Times.Once);
        }

        #endregion

        #region AlterarFiltroAsync e Filtros

        [Fact]
        public async Task AlterarFiltroAsync_Todos_DeveBuscarTodosDoBanco()
        {
            // Arrange
            var lista = new List<Paciente> { new Paciente { Id = 1, Nome = "Paciente Todos" } };
            _serviceMock.Setup(s => s.ObterTodosAsync()).ReturnsAsync(lista);

            // Act
            await _viewModel.AlterarFiltroCommand.ExecuteAsync(FiltroPaciente.Todos);

            // Assert
            Assert.Equal(FiltroPaciente.Todos, _viewModel.FiltroAtual);
            Assert.Single(_viewModel.Pacientes);
            _serviceMock.Verify(s => s.ObterTodosAsync(), Times.Once);
        }

        [Fact]
        public async Task AlterarFiltroAsync_Ativos_DeveBuscarApenasAtivos()
        {
            // Arrange
            var lista = new List<Paciente> { new Paciente { Id = 2, Nome = "Paciente Ativo" } };
            _serviceMock.Setup(s => s.BuscarComTratamentoAtivoAsync()).ReturnsAsync(lista);

            // Act
            await _viewModel.AlterarFiltroCommand.ExecuteAsync(FiltroPaciente.Ativos);

            // Assert
            Assert.Equal(FiltroPaciente.Ativos, _viewModel.FiltroAtual);
            Assert.Single(_viewModel.Pacientes);
            _serviceMock.Verify(s => s.BuscarComTratamentoAtivoAsync(), Times.Once);
        }

        [Fact]
        public async Task AlterarFiltroAsync_Idosos_DeveBuscarApenasIdosos()
        {
            // Arrange
            var lista = new List<Paciente> { new Paciente { Id = 3, Nome = "Paciente Idoso" } };
            _serviceMock.Setup(s => s.BuscarPacientesIdososAsync()).ReturnsAsync(lista);

            // Act
            await _viewModel.AlterarFiltroCommand.ExecuteAsync(FiltroPaciente.Idosos);

            // Assert
            Assert.Equal(FiltroPaciente.Idosos, _viewModel.FiltroAtual);
            Assert.Single(_viewModel.Pacientes);
            _serviceMock.Verify(s => s.BuscarPacientesIdososAsync(), Times.Once);
        }

        #endregion

        #region TextoBusca e Filtragem em Memória

        [Fact]
        public async Task OnTextoBuscaChanged_ComCorrespondencia_DeveFiltrarLista()
        {
            // Arrange
            var lista = new List<Paciente>
            {
                new Paciente { Id = 1, Nome = "Ronaldo Moreira" },
                new Paciente { Id = 2, Nome = "Maria Souza" }
            };
            _serviceMock.Setup(s => s.ObterTodosAsync()).ReturnsAsync(lista);

            // Act
            _viewModel.TextoBusca = "Ronaldo";
            await Task.Delay(50);

            // Assert
            Assert.Single(_viewModel.Pacientes);
            Assert.Equal("Ronaldo Moreira", _viewModel.Pacientes[0].Nome);
        }

        [Fact]
        public async Task OnTextoBuscaChanged_SemCorrespondencia_DeveRetornarVazio()
        {
            // Arrange
            var lista = new List<Paciente>
            {
                new Paciente { Id = 1, Nome = "Ronaldo Moreira" }
            };
            _serviceMock.Setup(s => s.ObterTodosAsync()).ReturnsAsync(lista);

            // Act
            _viewModel.TextoBusca = "Carlos";
            await Task.Delay(50);

            // Assert
            Assert.Empty(_viewModel.Pacientes);
        }

        #endregion

        #region NavegarParaCadastroAsync

        [Fact]
        public async Task NavegarParaCadastroAsync_Sempre_DeveNavegarParaPaginaCadastro()
        {
            // Arrange & Act
            await _viewModel.NavegarParaCadastroCommand.ExecuteAsync(null);

            // Assert
            _navMock.Verify(n => n.GoToAsync("PacientePage"), Times.Once);
        }

        #endregion

        #region SelecionarPacienteAsync

        [Fact]
        public async Task SelecionarPacienteAsync_IdValido_DeveNavegarComQueryString()
        {
            // Arrange
            int idValido = 10;

            // Act
            await _viewModel.SelecionarPacienteCommand.ExecuteAsync(idValido);

            // Assert
            _navMock.Verify(n => n.GoToAsync("PacienteDetalhesPage?id=10"), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task SelecionarPacienteAsync_IdInvalido_DeveInterromperSemNavegar(int idInvalido)
        {
            // Arrange & Act
            await _viewModel.SelecionarPacienteCommand.ExecuteAsync(idInvalido);

            // Assert
            _navMock.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion
    }
}