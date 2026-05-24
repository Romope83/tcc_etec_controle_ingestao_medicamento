using Moq;
using Xunit;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Data;
using IngestaoMed.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IngestaoMed.Tests.Services
{
    public class TratamentoServiceTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly TratamentoService _service;

        public TratamentoServiceTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _service = new TratamentoService(_dbMock.Object);
        }

        #region ObterPorIdAsync Tests

        [Fact]
        public async Task ObterPorIdAsync_TratamentoExistenteNoBanco_DeveRetornarOTratamentoCorrespondente()
        {
            // Arrange
            int tratamentoId = 1;
            var tratamentoEsperado = new Tratamento { Id = tratamentoId };
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(tratamentoEsperado);

            // Act
            var resultado = await _service.ObterPorIdAsync(tratamentoId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(tratamentoId, resultado!.Id);
        }

        [Fact]
        public async Task ObterPorIdAsync_TratamentoInexistenteNoBanco_DeveRetornarNulo()
        {
            // Arrange
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync((Tratamento?)null);

            // Act
            var resultado = await _service.ObterPorIdAsync(99);

            // Assert
            Assert.Null(resultado);
        }

        #endregion

        #region ObterMedicamentosVinculadosAsync Tests

        [Fact]
        public async Task ObterMedicamentosVinculadosAsync_NenhumVinculoEncontrado_DeveRetornarListaVaziaSemConsultarMedicamentos()
        {
            // Arrange
            int tratamentoId = 1;
            _dbMock.Setup(d => d.BuscarOndeAsync<MedicamentoTratamento>(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(new List<MedicamentoTratamento>());

            // Act
            var resultado = await _service.ObterMedicamentosVinculadosAsync(tratamentoId);

            // Assert
            Assert.Empty(resultado);
            _dbMock.Verify(d => d.BuscarPrimeiroAsync<Medicamento>(It.IsAny<Expression<Func<Medicamento, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task ObterMedicamentosVinculadosAsync_ComVinculosEMedicamentoNaoIdentificado_DeveRetornarVinculosComDadosDoMedicamentoNulos()
        {
            // Arrange
            int tratamentoId = 1;
            var vinculos = new List<MedicamentoTratamento> { new() { Id = 10, TratamentoId = tratamentoId, MedicamentoId = 100 } };

            _dbMock.Setup(d => d.BuscarOndeAsync<MedicamentoTratamento>(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(vinculos);
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Medicamento>(It.IsAny<Expression<Func<Medicamento, bool>>>()))
                   .ReturnsAsync((Medicamento?)null);
            _dbMock.Setup(d => d.BuscarOndeAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(new List<Agendamento>());

            // Act
            var resultado = await _service.ObterMedicamentosVinculadosAsync(tratamentoId);

            // Assert
            var vinculoResultado = resultado.First();
            Assert.Null(vinculoResultado.NomeMedicamento);
            Assert.Null(vinculoResultado.FotoPath);
        }

        [Fact]
        public async Task ObterMedicamentosVinculadosAsync_ComAgendamentosExistentes_DevePreencherDatasDaPrimeiraEUltimaDose()
        {
            // Arrange
            int tratamentoId = 1;
            var vinculos = new List<MedicamentoTratamento> { new() { Id = 10, TratamentoId = tratamentoId, MedicamentoId = 100 } };
            var medicamento = new Medicamento { Id = 100, NomeComercial = "Amoxicilina" };
            var agendamentos = new List<Agendamento>
            {
                new() { HorarioOriginal = new DateTime(2026, 05, 20, 8, 0, 0) },
                new() { HorarioOriginal = new DateTime(2026, 05, 25, 20, 0, 0) }
            };

            _dbMock.Setup(d => d.BuscarOndeAsync<MedicamentoTratamento>(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(vinculos);
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Medicamento>(It.IsAny<Expression<Func<Medicamento, bool>>>()))
                   .ReturnsAsync(medicamento);
            _dbMock.Setup(d => d.BuscarOndeAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(agendamentos);

            // Act
            var resultado = await _service.ObterMedicamentosVinculadosAsync(tratamentoId);

            // Assert
            var vinculoResultado = resultado.First();
            Assert.Equal("Amoxicilina", vinculoResultado.NomeMedicamento);
            Assert.Equal(new DateTime(2026, 05, 20, 8, 0, 0), vinculoResultado.DataPrimeiraDose);
            Assert.Equal(new DateTime(2026, 05, 25, 20, 0, 0), vinculoResultado.DataUltimaDose);
        }

        #endregion

        #region ObterTratamentoComMedicamentosAsync Tests

        [Fact]
        public async Task ObterTratamentoComMedicamentosAsync_TratamentoInexistente_DeveRetornarTratamentoNuloEListaDeMedicamentosVazia()
        {
            // Arrange
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync((Tratamento?)null);

            // Act
            var (tratamento, medicamentos) = await _service.ObterTratamentoComMedicamentosAsync(99);

            // Assert
            Assert.Null(tratamento);
            Assert.Empty(medicamentos);
            _dbMock.Verify(d => d.BuscarOndeAsync<MedicamentoTratamento>(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task ObterTratamentoComMedicamentosAsync_TratamentoExistente_DeveRetornarTratamentoPreenchidoEListaDeMedicamentosVinculados()
        {
            // Arrange
            int tratamentoId = 1;
            var tratamento = new Tratamento { Id = tratamentoId };
            var vinculos = new List<MedicamentoTratamento> { new() { Id = 10, TratamentoId = tratamentoId } };

            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(tratamento);
            _dbMock.Setup(d => d.BuscarOndeAsync<MedicamentoTratamento>(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(vinculos);
            _dbMock.Setup(d => d.BuscarOndeAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(new List<Agendamento>());

            // Act
            var (resultadoTratamento, resultadoMedicamentos) = await _service.ObterTratamentoComMedicamentosAsync(tratamentoId);

            // Assert
            Assert.NotNull(resultadoTratamento);
            Assert.Equal(tratamentoId, resultadoTratamento!.Id);
            Assert.Single(resultadoMedicamentos);
        }

        #endregion

        #region InserirTratamentoAsync Tests

        [Fact]
        public async Task InserirTratamentoAsync_ExecucaoComSucesso_DeveChamarInsercaoNoBancoERetornarIdDoTratamento()
        {
            // Arrange
            var tratamento = new Tratamento { Id = 45 };
            _dbMock.Setup(d => d.InserirAsync(tratamento)).ReturnsAsync(true);

            // Act
            int resultadoId = await _service.InserirTratamentoAsync(tratamento);

            // Assert
            Assert.Equal(45, resultadoId);
            _dbMock.Verify(d => d.InserirAsync(tratamento), Times.Once);
        }

        #endregion

        #region AtualizarTratamentoAsync Tests

        [Fact]
        public async Task AtualizarTratamentoAsync_ExecucaoComSucesso_DeveRetornarQuantidadeDeLinhasAfetadasNoBanco()
        {
            // Arrange
            var tratamento = new Tratamento { Id = 1 };
            _dbMock.Setup(d => d.AtualizarAsync(tratamento)).ReturnsAsync(1);

            // Act
            int linhasAfetadas = await _service.AtualizarTratamentoAsync(tratamento);

            // Assert
            Assert.Equal(1, linhasAfetadas);
            _dbMock.Verify(d => d.AtualizarAsync(tratamento), Times.Once);
        }

        #endregion

        #region ExcluirTratamentoAsync Tests

        [Fact]
        public async Task ExcluirTratamentoAsync_ExecucaoComSucesso_DeveRetornarQuantidadeDeLinhasRemovidasNoBanco()
        {
            // Arrange
            var tratamento = new Tratamento { Id = 1 };
            _dbMock.Setup(d => d.ExcluirAsync(tratamento)).ReturnsAsync(1);

            // Act
            int linhasAfetadas = await _service.ExcluirTratamentoAsync(tratamento);

            // Assert
            Assert.Equal(1, linhasAfetadas);
            _dbMock.Verify(d => d.ExcluirAsync(tratamento), Times.Once);
        }

        #endregion

        #region ExcluirMedicamentoTratamentoAsync Tests

        [Fact]
        public async Task ExcluirMedicamentoTratamentoAsync_ExecucaoComSucesso_DeveRetornarQuantidadeDeLinhasDeVinculoRemovidas()
        {
            // Arrange
            var vinculo = new MedicamentoTratamento { Id = 10 };
            _dbMock.Setup(d => d.ExcluirAsync(vinculo)).ReturnsAsync(1);

            // Act
            int linhasAfetadas = await _service.ExcluirMedicamentoTratamentoAsync(vinculo);

            // Assert
            Assert.Equal(1, linhasAfetadas);
            _dbMock.Verify(d => d.ExcluirAsync(vinculo), Times.Once);
        }

        #endregion

        #region ObterOndeMedicamentoVinculadoAsync Tests

        [Fact]
        public async Task ObterOndeMedicamentoVinculadoAsync_PredicadoValido_DeveRetornarListaDeVinculosFiltradosDoBanco()
        {
            // Arrange
            var vinculosFiltrados = new List<MedicamentoTratamento> { new() { Id = 10, TratamentoId = 2 } };
            _dbMock.Setup(d => d.BuscarOndeAsync(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(vinculosFiltrados);

            // Act
            var resultado = await _service.ObterOndeMedicamentoVinculadoAsync(x => x.TratamentoId == 2);

            // Assert
            Assert.Single(resultado);
            Assert.Equal(10, resultado.First().Id);
        }

        #endregion

        #region ObterTodosAsync Tests

        [Fact]
        public async Task ObterTodosAsync_TratamentosCadastrados_DeveRetornarListaCompletaDeTratamentosDoBanco()
        {
            // Arrange
            var listaCompleta = new List<Tratamento> { new() { Id = 1 }, new() { Id = 2 } };
            _dbMock.Setup(d => d.BuscarTodosAsync<Tratamento>()).ReturnsAsync(listaCompleta);

            // Act
            var resultado = await _service.ObterTodosAsync();

            // Assert
            Assert.Equal(2, resultado.Count);
        }

        [Fact]
        public async Task ObterTodosAsync_BancoSemTratamentos_DeveRetornarListaVazia()
        {
            // Arrange
            _dbMock.Setup(d => d.BuscarTodosAsync<Tratamento>()).ReturnsAsync(new List<Tratamento>());

            // Act
            var resultado = await _service.ObterTodosAsync();

            // Assert
            Assert.Empty(resultado);
        }

        #endregion
    }
}