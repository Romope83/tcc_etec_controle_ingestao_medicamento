using Moq;
using Xunit;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Data;
using IngestaoMed.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IngestaoMed.Tests.Services
{
    public class MedicamentoServiceTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly MedicamentoService _service;

        public MedicamentoServiceTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _service = new MedicamentoService(_dbMock.Object);
        }

        #region AdicionarOuAtualizarMedicamentoAsync Tests

        [Fact]
        public async Task AdicionarOuAtualizarMedicamentoAsync_IdZero_DeveInserirComSucesso()
        {
            // Arrange
            var medicamento = new Medicamento { Id = 0, NomeComercial = "Dipirona" };
            _dbMock.Setup(d => d.InserirAsync(medicamento)).ReturnsAsync(true);

            // Act
            bool resultado = await _service.AdicionarOuAtualizarMedicamentoAsync(medicamento);

            // Assert
            Assert.True(resultado);
            _dbMock.Verify(d => d.InserirAsync(medicamento), Times.Once);
            _dbMock.Verify(d => d.AtualizarAsync(It.IsAny<Medicamento>()), Times.Never);
        }

        [Fact]
        public async Task AdicionarOuAtualizarMedicamentoAsync_IdZero_FalhaNaInsercao_DeveRetornarFalse()
        {
            // Arrange
            var medicamento = new Medicamento { Id = 0, NomeComercial = "Dipirona" };
            _dbMock.Setup(d => d.InserirAsync(medicamento)).ReturnsAsync(false);

            // Act
            bool resultado = await _service.AdicionarOuAtualizarMedicamentoAsync(medicamento);

            // Assert
            Assert.False(resultado);
            _dbMock.Verify(d => d.InserirAsync(medicamento), Times.Once);
        }

        [Fact]
        public async Task AdicionarOuAtualizarMedicamentoAsync_IdMaiorQueZero_DeveAtualizarComSucesso()
        {
            // Arrange
            var medicamento = new Medicamento { Id = 1, NomeComercial = "Dipirona" };
            _dbMock.Setup(d => d.AtualizarAsync(medicamento)).ReturnsAsync(1);

            // Act
            bool resultado = await _service.AdicionarOuAtualizarMedicamentoAsync(medicamento);

            // Assert
            Assert.True(resultado);
            _dbMock.Verify(d => d.AtualizarAsync(medicamento), Times.Once);
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<Medicamento>()), Times.Never);
        }

        [Fact]
        public async Task AdicionarOuAtualizarMedicamentoAsync_IdMaiorQueZero_NenhumaLinhaAfetada_DeveRetornarFalse()
        {
            // Arrange
            var medicamento = new Medicamento { Id = 1, NomeComercial = "Dipirona" };
            _dbMock.Setup(d => d.AtualizarAsync(medicamento)).ReturnsAsync(0);

            // Act
            bool resultado = await _service.AdicionarOuAtualizarMedicamentoAsync(medicamento);

            // Assert
            Assert.False(resultado);
            _dbMock.Verify(d => d.AtualizarAsync(medicamento), Times.Once);
        }

        #endregion

        #region ExisteMedicamentoAsync Tests

        [Fact]
        public async Task ExisteMedicamentoAsync_MedicamentoEncontrado_DeveRetornarTrue()
        {
            // Arrange
            string nome = "Paracetamol";
            string forma = "Comprimido";
            var medicamentoExistente = new Medicamento { NomeComercial = nome, FormaIngestao = forma };

            _dbMock.Setup(d => d.BuscarPrimeiroAsync(It.IsAny<Expression<Func<Medicamento, bool>>>()))
                   .ReturnsAsync(medicamentoExistente);

            // Act
            bool resultado = await _service.ExisteMedicamentoAsync(nome, forma);

            // Assert
            Assert.True(resultado);
            _dbMock.Verify(d => d.BuscarPrimeiroAsync<Medicamento>(It.IsAny<Expression<Func<Medicamento, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task ExisteMedicamentoAsync_MedicamentoNaoEncontrado_DeveRetornarFalse()
        {
            // Arrange
            _dbMock.Setup(d => d.BuscarPrimeiroAsync(It.IsAny<Expression<Func<Medicamento, bool>>>()))
                   .ReturnsAsync((Medicamento?)null);

            // Act
            bool resultado = await _service.ExisteMedicamentoAsync("Inexistente", "Gota");

            // Assert
            Assert.False(resultado);
            _dbMock.Verify(d => d.BuscarPrimeiroAsync<Medicamento>(It.IsAny<Expression<Func<Medicamento, bool>>>()), Times.Once);
        }

        #endregion

        #region ObterTodosAsync Tests

        [Fact]
        public async Task ObterTodosAsync_ListaComElementos_DeveRetornarListaCompleta()
        {
            // Arrange
            var listaEsperada = new List<Medicamento>
            {
                new() { Id = 1, NomeComercial = "Med A" },
                new() { Id = 2, NomeComercial = "Med B" }
            };

            _dbMock.Setup(d => d.BuscarTodosAsync<Medicamento>()).ReturnsAsync(listaEsperada);

            // Act
            var resultado = await _service.ObterTodosAsync();

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count);
            Assert.Equal("Med A", resultado[0].NomeComercial);
        }

        [Fact]
        public async Task ObterTodosAsync_BancoVazio_DeveRetornarListaVazia()
        {
            // Arrange
            _dbMock.Setup(d => d.BuscarTodosAsync<Medicamento>()).ReturnsAsync(new List<Medicamento>());

            // Act
            var resultado = await _service.ObterTodosAsync();

            // Assert
            Assert.NotNull(resultado);
            Assert.Empty(resultado);
        }

        #endregion

        #region RemoverMedicamentoAsync Tests

        [Fact]
        public async Task RemoverMedicamentoAsync_ComSucesso_DeveRetornarTrue()
        {
            // Arrange
            var medicamento = new Medicamento { Id = 5 };
            _dbMock.Setup(d => d.ExcluirAsync(medicamento)).ReturnsAsync(1);

            // Act
            bool resultado = await _service.RemoverMedicamentoAsync(medicamento);

            // Assert
            Assert.True(resultado);
            _dbMock.Verify(d => d.ExcluirAsync(medicamento), Times.Once);
        }

        [Fact]
        public async Task RemoverMedicamentoAsync_FalhaOuNaoEncontrado_DeveRetornarFalse()
        {
            // Arrange
            var medicamento = new Medicamento { Id = 99 };
            _dbMock.Setup(d => d.ExcluirAsync(medicamento)).ReturnsAsync(0);

            // Act
            bool resultado = await _service.RemoverMedicamentoAsync(medicamento);

            // Assert
            Assert.False(resultado);
            _dbMock.Verify(d => d.ExcluirAsync(medicamento), Times.Once);
        }

        #endregion

        #region BuscarPrimeiroMedicamentoAsync Tests

        [Fact]
        public async Task BuscarPrimeiroMedicamentoAsync_EncontraRegistro_DeveRetornarMedicamento()
        {
            // Arrange
            var medicamentoEsperado = new Medicamento { Id = 10, NomeComercial = "Ibuprofeno" };
            _dbMock.Setup(d => d.BuscarPrimeiroAsync(It.IsAny<Expression<Func<Medicamento, bool>>>()))
                   .ReturnsAsync(medicamentoEsperado);

            // Act
            var resultado = await _service.BuscarPrimeiroMedicamentoAsync(m => m.Id == 10);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(10, resultado!.Id);
            Assert.Equal("Ibuprofeno", resultado.NomeComercial);
        }

        [Fact]
        public async Task BuscarPrimeiroMedicamentoAsync_NaoEncontraRegistro_DeveRetornarNulo()
        {
            // Arrange
            _dbMock.Setup(d => d.BuscarPrimeiroAsync(It.IsAny<Expression<Func<Medicamento, bool>>>()))
                   .ReturnsAsync((Medicamento?)null);

            // Act
            var resultado = await _service.BuscarPrimeiroMedicamentoAsync(m => m.Id == -1);

            // Assert
            Assert.Null(resultado);
        }

        #endregion
    }
}