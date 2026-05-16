using Moq;
using Xunit;
using IngestaoMed.Core.ViewModels;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IngestaoMed.Tests.ViewModels
{
    public class PacienteDetalhesViewModelTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly Mock<IPacienteService> _pacienteServiceMock;
        private readonly Mock<INavigationService> _navigationMock;
        private readonly PacienteDetalhesViewModel _viewModel;

        public PacienteDetalhesViewModelTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _pacienteServiceMock = new Mock<IPacienteService>();
            _navigationMock = new Mock<INavigationService>();

            _viewModel = new PacienteDetalhesViewModel(
                _dbMock.Object,
                _pacienteServiceMock.Object,
                _navigationMock.Object);
        }

        // --- MÉTODOS: InicializarAsync / CarregarDadosAsync ---

        [Fact]
        public async Task InicializarAsync_DevePreencherPacienteETratamentos_CaminhoFeliz()
        {
            // Arrange
            var idValido = 1;
            var pacienteFake = new Paciente
            {
                Id = idValido,
                Nome = "Ronaldo",
                Tratamentos = new List<Tratamento> { new Tratamento(), new Tratamento() }
            };

            // Ajustado para o tipo esperado pela ViewModel para resolver o erro CS1503
            _pacienteServiceMock
                .Setup(s => s.ObterDetalhesCompletosAsync(idValido))
                .ReturnsAsync(pacienteFake);

            // Act
            await _viewModel.InicializarAsync(idValido);

            // Assert (Saídas de Estado)
            Assert.Equal(idValido, _viewModel.Id);
            Assert.NotNull(_viewModel.PacienteSelecionado);
            Assert.Equal("Ronaldo", _viewModel.PacienteSelecionado.Nome);
            Assert.Equal(2, _viewModel.Tratamentos.Count);
        }

        [Fact]
        public async Task InicializarAsync_DeveLimparListaDeTratamentosEVisualizarNull_QuandoPacienteNaoTiverTratamentos()
        {
            // Arrange
            var idValido = 2;
            var pacienteSemTratamentos = new Paciente { Id = idValido, Nome = "Maria", Tratamentos = null! };

            // Popula a lista previamente para testar se o Clear() funciona no fluxo
            _viewModel.Tratamentos.Add(new Tratamento());

            _pacienteServiceMock
                .Setup(s => s.ObterDetalhesCompletosAsync(idValido))
                .ReturnsAsync(pacienteSemTratamentos);

            // Act
            await _viewModel.InicializarAsync(idValido);

            // Assert (Casos de Borda de Coleção)
            Assert.Empty(_viewModel.Tratamentos);
            Assert.Null(_viewModel.PacienteSelecionado!.Tratamentos);
        }

        [Fact]
        public async Task InicializarAsync_DeveCapturarExcecaoENaoCrashar_QuandoServicoFalhar()
        {
            // Arrange
            var idValido = 3;
            _pacienteServiceMock
                .Setup(s => s.ObterDetalhesCompletosAsync(idValido))
                .ThrowsAsync(new Exception("Falha de conexão com o banco local SQLite"));

            // Act
            var exception = await Record.ExceptionAsync(() => _viewModel.InicializarAsync(idValido));

            // Assert (Caso de Borda de Falha)
            Assert.Null(exception); // O bloco try/catch interno deve impedir o crash
            Assert.Null(_viewModel.PacienteSelecionado);
            Assert.Empty(_viewModel.Tratamentos);
        }

        // --- MÉTODO: CarregarDetalhesPacienteAsync ---


        // --- MÉTODOS DE COMANDO (Navegação/Saída de Rotas) ---

        [Fact]
        public async Task NovoTratamentoCommand_DeveNavegarParaEdicaoPassandoIdDoPaciente()
        {
            // Arrange
            _viewModel.Id = 10;

            // Act
            await _viewModel.NovoTratamentoCommand.ExecuteAsync(null);

            // Assert
            _navigationMock.Verify(n => n.GoToAsync("EdicaoTratamentoPage?pacienteId=10"), Times.Once);
        }

        [Fact]
        public async Task EditarTratamentoCommand_DeveNavegarParaEdicaoPassandoIdDoTratamento()
        {
            // Arrange
            var tratamentoId = 55;

            // Act
            await _viewModel.EditarTratamentoCommand.ExecuteAsync(tratamentoId);

            // Assert
            _navigationMock.Verify(n => n.GoToAsync("EdicaoTratamentoPage?tratamentoId=55"), Times.Once);
        }

        [Fact]
        public async Task EditarPacienteCommand_DeveNavegarParaCadastroPassandoIdDoPaciente()
        {
            // Arrange
            var pacienteId = 7;

            // Act
            await _viewModel.EditarPacienteCommand.ExecuteAsync(pacienteId);

            // Assert
            _navigationMock.Verify(n => n.GoToAsync("CadastroPacientePage?id=7"), Times.Once);
        }
    }
}