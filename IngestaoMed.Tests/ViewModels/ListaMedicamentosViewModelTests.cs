using Moq;
using Xunit;
using IngestaoMed.Core.ViewModels;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace IngestaoMed.Tests.ViewModels
{
    public class ListaMedicamentosViewModelTests
    {
        private readonly Mock<IMedicamentoService> _medicamentoServiceMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly Mock<IDialogService> _dialogServiceMock;
        private readonly ListaMedicamentosViewModel _viewModel;

        public ListaMedicamentosViewModelTests()
        {
            _medicamentoServiceMock = new Mock<IMedicamentoService>();
            _navigationServiceMock = new Mock<INavigationService>();
            _dialogServiceMock = new Mock<IDialogService>();

            _viewModel = new ListaMedicamentosViewModel(
                _medicamentoServiceMock.Object,
                _navigationServiceMock.Object,
                _dialogServiceMock.Object);
        }

        #region CarregarMedicamentosAsync

        [Fact]
        public async Task CarregarMedicamentosAsync_Sucesso_DevePopularLista()
        {
            // Arrange
            var listaMock = new List<Medicamento>
            {
                new Medicamento { Id = 1, NomeComercial = "Paracetamol" },
                new Medicamento { Id = 2, NomeComercial = "Ibuprofeno" }
            };
            _medicamentoServiceMock.Setup(s => s.ObterTodosAsync()).ReturnsAsync(listaMock);

            // Act
            await _viewModel.CarregarMedicamentosCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(2, _viewModel.Medicamentos.Count);
            Assert.Contains(_viewModel.Medicamentos, m => m.NomeComercial == "Paracetamol");
            Assert.Contains(_viewModel.Medicamentos, m => m.NomeComercial == "Ibuprofeno");
        }

        [Fact]
        public async Task CarregarMedicamentosAsync_BancoVazio_DeveLimparLista()
        {
            // Arrange
            _viewModel.Medicamentos.Add(new Medicamento { Id = 1, NomeComercial = "Antigo" });
            _medicamentoServiceMock.Setup(s => s.ObterTodosAsync()).ReturnsAsync(new List<Medicamento>());

            // Act
            await _viewModel.CarregarMedicamentosCommand.ExecuteAsync(null);

            // Assert
            Assert.Empty(_viewModel.Medicamentos);
        }

        #endregion

        #region FiltrarLista e OnTextoBuscaChanged

        [Fact]
        public async Task OnTextoBuscaChanged_TextoVazio_DeveExibirTodosOsMedicamentos()
        {
            // Arrange
            var listaMock = new List<Medicamento>
            {
                new Medicamento { Id = 1, NomeComercial = "Paracetamol" },
                new Medicamento { Id = 2, NomeComercial = "Ibuprofeno" }
            };
            _medicamentoServiceMock.Setup(s => s.ObterTodosAsync()).ReturnsAsync(listaMock);
            await _viewModel.CarregarMedicamentosCommand.ExecuteAsync(null);

            // Act
            _viewModel.TextoBusca = "";

            // Assert
            Assert.Equal(2, _viewModel.Medicamentos.Count);
        }

        [Fact]
        public async Task OnTextoBuscaChanged_TextoPreenchido_DeveFiltrarPorNomeComercial()
        {
            // Arrange
            var listaMock = new List<Medicamento>
            {
                new Medicamento { Id = 1, NomeComercial = "Paracetamol" },
                new Medicamento { Id = 2, NomeComercial = "Ibuprofeno" }
            };
            _medicamentoServiceMock.Setup(s => s.ObterTodosAsync()).ReturnsAsync(listaMock);
            await _viewModel.CarregarMedicamentosCommand.ExecuteAsync(null);

            // Act
            _viewModel.TextoBusca = "para";

            // Assert
            Assert.Single(_viewModel.Medicamentos);
            Assert.Equal("Paracetamol", _viewModel.Medicamentos[0].NomeComercial);
        }

        [Fact]
        public async Task OnTextoBuscaChanged_SemCorrespondencia_DeveRetornarListaVazia()
        {
            // Arrange
            var listaMock = new List<Medicamento>
            {
                new Medicamento { Id = 1, NomeComercial = "Paracetamol" }
            };
            _medicamentoServiceMock.Setup(s => s.ObterTodosAsync()).ReturnsAsync(listaMock);
            await _viewModel.CarregarMedicamentosCommand.ExecuteAsync(null);

            // Act
            _viewModel.TextoBusca = "Amoxicilina";

            // Assert
            Assert.Empty(_viewModel.Medicamentos);
        }

        #endregion

        #region NavegarParaCadastroAsync

        [Fact]
        public async Task NavegarParaCadastroAsync_ParametroIdValido_DeveNavegarParaEdicao()
        {
            // Arrange
            int idValido = 5;

            // Act
            await _viewModel.NavegarParaCadastroCommand.ExecuteAsync(idValido);

            // Assert
            _navigationServiceMock.Verify(n => n.GoToAsync("MedicamentoPage?id=5"), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData("texto")]
        public async Task NavegarParaCadastroAsync_ParametroInvalidoOuBotaoNovo_DeveNavegarParaFormularioLimpo(object? param)
        {
            // Arrange & Act
            await _viewModel.NavegarParaCadastroCommand.ExecuteAsync(param);

            // Assert
            _navigationServiceMock.Verify(n => n.GoToAsync("MedicamentoPage"), Times.Once);
        }

        #endregion
    }
}