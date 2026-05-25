using Moq;
using Xunit;
using IngestaoMed.Core.ViewModels;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace IngestaoMed.Tests.ViewModels
{
    public class PacienteDetalhesViewModelTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly Mock<IPacienteService> _pacienteServiceMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly PacienteDetalhesViewModel _viewModel;

        public PacienteDetalhesViewModelTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _pacienteServiceMock = new Mock<IPacienteService>();
            _navigationServiceMock = new Mock<INavigationService>();

            _viewModel = new PacienteDetalhesViewModel(
                _dbMock.Object,
                _pacienteServiceMock.Object,
                _navigationServiceMock.Object);
        }

        #region InicializarAsync e CarregarDadosAsync

        [Fact]
        public async Task InicializarAsync_Sucesso_DevePopularPacienteETratamentos()
        {
            // Arrange
            int pacienteId = 1;
            var pacienteMock = new Paciente
            {
                Id = pacienteId,
                Nome = "Ronaldo",
                Tratamentos = new List<Tratamento>
                {
                    new Tratamento { Id = 10, Nome = "Tratamento A" },
                    new Tratamento { Id = 11, Nome = "Tratamento B" }
                }
            };
            _pacienteServiceMock.Setup(s => s.ObterDetalhesCompletosAsync(pacienteId)).ReturnsAsync(pacienteMock);

            // Act
            await _viewModel.InicializarAsync(pacienteId);

            // Assert
            Assert.Equal(pacienteId, _viewModel.Id);
            Assert.NotNull(_viewModel.PacienteSelecionado);
            Assert.Equal("Ronaldo", _viewModel.PacienteSelecionado.Nome);
            Assert.Equal(2, _viewModel.Tratamentos.Count);
        }

        [Fact]
        public async Task InicializarAsync_PacienteSemTratamentos_DeveDeixarListaVazia()
        {
            // Arrange
            int pacienteId = 2;
            var pacienteMock = new Paciente { Id = pacienteId, Nome = "Moreira", Tratamentos = null! };
            _pacienteServiceMock.Setup(s => s.ObterDetalhesCompletosAsync(pacienteId)).ReturnsAsync(pacienteMock);

            // Act
            await _viewModel.InicializarAsync(pacienteId);

            // Assert
            Assert.Empty(_viewModel.Tratamentos);
        }

        [Fact]
        public async Task InicializarAsync_ExcecaoNoServico_DeveTratarSilenciosamente()
        {
            // Arrange
            int pacienteId = 3;
            _pacienteServiceMock.Setup(s => s.ObterDetalhesCompletosAsync(pacienteId)).ThrowsAsync(new Exception("Erro no banco"));

            // Act
            await _viewModel.InicializarAsync(pacienteId);

            // Assert
            Assert.Null(_viewModel.PacienteSelecionado);
            Assert.Empty(_viewModel.Tratamentos);
        }

        #endregion

        #region Comandos de Navegação

        [Fact]
        public async Task NovoTratamentoAsync_Sempre_DeveNavegarComPacienteId()
        {
            // Arrange
            _viewModel.Id = 4;

            // Act
            await _viewModel.NovoTratamentoCommand.ExecuteAsync(null);

            // Assert
            _navigationServiceMock.Verify(n => n.GoToAsync("TratamentoPage?pacienteId=4"), Times.Once);
        }

        [Fact]
        public async Task EditarTratamentoAsync_Sempre_DeveNavegarComTratamentoId()
        {
            // Arrange
            int tratamentoId = 25;

            // Act
            await _viewModel.EditarTratamentoCommand.ExecuteAsync(tratamentoId);

            // Assert
            _navigationServiceMock.Verify(n => n.GoToAsync("TratamentoPage?tratamentoId=25"), Times.Once);
        }

        [Fact]
        public async Task EditarPacienteAsync_Sempre_DeveNavegarComIdDoPaciente()
        {
            // Arrange
            int pacienteId = 8;

            // Act
            await _viewModel.EditarPacienteCommand.ExecuteAsync(pacienteId);

            // Assert
            _navigationServiceMock.Verify(n => n.GoToAsync("PacientePage?id=8"), Times.Once);
        }

        #endregion

        #region SelecionarMedicamentoVinculadoAsync

        [Fact]
        public async Task SelecionarMedicamentoVinculadoAsync_MedicamentoNulo_DeveRetornarSemAcao()
        {
            // Arrange & Act
            await _viewModel.SelecionarMedicamentoVinculadoCommand.ExecuteAsync(null);

            // Assert
            _navigationServiceMock.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SelecionarMedicamentoVinculadoAsync_MedicamentoValido_DeveNavegarComIds()
        {
            // Arrange
            var medicamentoMock = new MedicamentoTratamento { Id = 5, TratamentoId = 12 };

            // Act
            await _viewModel.SelecionarMedicamentoVinculadoCommand.ExecuteAsync(medicamentoMock);

            // Assert
            _navigationServiceMock.Verify(n => n.GoToAsync("AgendamentoPage?tratamentoId=12&medicamentoTratamentoId=5"), Times.Once);
        }

        #endregion
    }
}