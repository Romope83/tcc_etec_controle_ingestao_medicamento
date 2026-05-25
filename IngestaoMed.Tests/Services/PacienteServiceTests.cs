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
    public class PacienteServiceTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly PacienteService _service;

        public PacienteServiceTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _service = new PacienteService(_dbMock.Object);
        }

        #region ObterTodosAsync Tests

        [Fact]
        public async Task ObterTodosAsync_BancoVazio_DeveRetornarListaVaziaSemProcessarTratamentos()
        {
            // Arrange
            _dbMock.Setup(d => d.BuscarTodosAsync<Paciente>())
                   .ReturnsAsync(new List<Paciente>());

            // Act
            var resultado = await _service.ObterTodosAsync();

            // Assert
            Assert.Empty(resultado);
            _dbMock.Verify(d => d.BuscarOndeAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task ObterTodosAsync_PacienteSemTratamentos_DeveFormatasResumoComZerosESemAgendamento()
        {
            // Arrange
            var paciente = new Paciente { Id = 1 };
            _dbMock.Setup(d => d.BuscarTodosAsync<Paciente>())
                   .ReturnsAsync(new List<Paciente> { paciente });

            _dbMock.Setup(d => d.BuscarOndeAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(new List<Tratamento>());

            // Act
            var resultado = await _service.ObterTodosAsync();

            // Assert
            var pResultado = resultado.First();
            Assert.Equal("Tratamentos 00/00", pResultado.ResumoTratamentos);
            Assert.Equal("--", pResultado.ProximaData);
            Assert.Equal("--", pResultado.ProximoHorario);
        }

        [Fact]
        public async Task ObterTodosAsync_PacienteComProximoAlarmeValido_DevePreencherCamposDeDataEHorarioFormatados()
        {
            // Arrange
            var paciente = new Paciente { Id = 1 };
            var tratamento = new Tratamento { Id = 10, PacienteId = 1, Ativo = true };
            var vinculo = new MedicamentoTratamento { Id = 100, TratamentoId = 10 };
            var agendamento = new Agendamento
            {
                MedicamentoTratamentoId = 100,
                Status = "Pendente",
                ProximoAlarme = DateTime.Now.AddDays(1),
                HorarioOriginal = new DateTime(2026, 05, 25, 14, 30, 0)
            };

            _dbMock.Setup(d => d.BuscarTodosAsync<Paciente>())
                   .ReturnsAsync(new List<Paciente> { paciente });

            _dbMock.Setup(d => d.BuscarOndeAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(new List<Tratamento> { tratamento });

            _dbMock.Setup(d => d.BuscarOndeAsync<MedicamentoTratamento>(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(new List<MedicamentoTratamento> { vinculo });

            _dbMock.Setup(d => d.BuscarPrimeiroAsync(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(agendamento);

            // Act
            var resultado = await _service.ObterTodosAsync();

            // Assert
            var pResultado = resultado.First();
            Assert.Equal("Tratamentos 01/01", pResultado.ResumoTratamentos);
            Assert.Equal("25.05.2026", pResultado.ProximaData);
            Assert.Equal("14:30", pResultado.ProximoHorario);
        }

        #endregion

        #region SalvarOuAtualizarPacienteAsync Tests

        [Fact]
        public async Task SalvarOuAtualizarPacienteAsync_IdZero_DeveExecutarInsercaoComSucesso()
        {
            // Arrange
            var paciente = new Paciente { Id = 0 };
            _dbMock.Setup(d => d.InserirAsync(paciente)).ReturnsAsync(true);

            // Act
            bool resultado = await _service.SalvarOuAtualizarPacienteAsync(paciente);

            // Assert
            Assert.True(resultado);
            _dbMock.Verify(d => d.InserirAsync(paciente), Times.Once);
            _dbMock.Verify(d => d.AtualizarAsync(It.IsAny<Paciente>()), Times.Never);
        }

        [Fact]
        public async Task SalvarOuAtualizarPacienteAsync_IdMaiorQueZero_DeveAtualizarERetornarTrueSeModificarLinhas()
        {
            // Arrange
            var paciente = new Paciente { Id = 5 };
            _dbMock.Setup(d => d.AtualizarAsync(paciente)).ReturnsAsync(1);

            // Act
            bool resultado = await _service.SalvarOuAtualizarPacienteAsync(paciente);

            // Assert
            Assert.True(resultado);
            _dbMock.Verify(d => d.AtualizarAsync(paciente), Times.Once);
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<Paciente>()), Times.Never);
        }

        [Fact]
        public async Task SalvarOuAtualizarPacienteAsync_IdMaiorQueZero_DeveRetornarFalseSeNenhumaLinhaForAfetada()
        {
            // Arrange
            var paciente = new Paciente { Id = 5 };
            _dbMock.Setup(d => d.AtualizarAsync(paciente)).ReturnsAsync(0);

            // Act
            bool resultado = await _service.SalvarOuAtualizarPacienteAsync(paciente);

            // Assert
            Assert.False(resultado);
        }

        #endregion

        #region RemoverPacienteAsync Tests

        [Fact]
        public async Task RemoverPacienteAsync_PacienteExistente_DeveRetornarTrueSeExclusaoForConfirmada()
        {
            // Arrange
            var paciente = new Paciente { Id = 1 };
            _dbMock.Setup(d => d.ExcluirAsync(paciente)).ReturnsAsync(1);

            // Act
            bool resultado = await _service.RemoverPacienteAsync(paciente);

            // Assert
            Assert.True(resultado);
        }

        [Fact]
        public async Task RemoverPacienteAsync_FalhaOuInexistente_DeveRetornarFalseSeNenhumRegistroForExcluido()
        {
            // Arrange
            var paciente = new Paciente { Id = 99 };
            _dbMock.Setup(d => d.ExcluirAsync(paciente)).ReturnsAsync(0);

            // Act
            bool resultado = await _service.RemoverPacienteAsync(paciente);

            // Assert
            Assert.False(resultado);
        }

        #endregion

        #region BuscarOndeAsync Tests

        [Fact]
        public async Task BuscarOndeAsync_PredicadoValido_DeveRetornarFiltradosDoBanco()
        {
            // Arrange
            var listaFiltro = new List<Paciente> { new() { Id = 1, Nome = "Ronaldo" } };
            _dbMock.Setup(d => d.BuscarOndeAsync(It.IsAny<Expression<Func<Paciente, bool>>>()))
                   .ReturnsAsync(listaFiltro);

            // Act
            var resultado = await _service.BuscarOndeAsync(p => p.Nome == "Ronaldo");

            // Assert
            Assert.Single(resultado);
            Assert.Equal("Ronaldo", resultado.First().Nome);
        }

        #endregion

        #region BuscarPacientesIdososAsync Tests

        [Fact]
        public async Task BuscarPacientesIdososAsync_FiltroDeIdade_DeveConsultarBancoComDataLimiteDeSetentaAnosAtras()
        {
            // Arrange
            var idosos = new List<Paciente> { new() { Id = 2, DataNascimento = DateTime.Today.AddYears(-75) } };
            _dbMock.Setup(d => d.BuscarOndeAsync(It.IsAny<Expression<Func<Paciente, bool>>>()))
                   .ReturnsAsync(idosos);

            // Act
            var resultado = await _service.BuscarPacientesIdososAsync();

            // Assert
            Assert.Single(resultado);
        }

        #endregion

        #region BuscarComTratamentoAtivoAsync Tests

        [Fact]
        public async Task BuscarComTratamentoAtivoAsync_TratamentosAtivosExistentes_DeveRetornarPacientesVinculadosSemDuplicidade()
        {
            // Arrange
            var tratamentosAtivos = new List<Tratamento>
            {
                new() { Id = 1, PacienteId = 10, Ativo = true },
                new() { Id = 2, PacienteId = 10, Ativo = true } // Mesmo paciente com outro tratamento ativo
            };
            var pacientes = new List<Paciente> { new() { Id = 10 } };

            _dbMock.Setup(d => d.BuscarOndeAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(tratamentosAtivos);

            _dbMock.Setup(d => d.BuscarOndeAsync<Paciente>(It.IsAny<Expression<Func<Paciente, bool>>>()))
                   .ReturnsAsync(pacientes);

            // Act
            var resultado = await _service.BuscarComTratamentoAtivoAsync();

            // Assert
            Assert.Single(resultado);
            _dbMock.Verify(d => d.BuscarOndeAsync<Paciente>(It.Is<Expression<Func<Paciente, bool>>>(exp => exp.Compile().Invoke(new Paciente { Id = 10 }))), Times.Once);
        }

        #endregion

        #region ObterDetalhesCompletosAsync Tests

        [Fact]
        public async Task ObterDetalhesCompletosAsync_PacienteNaoEncontrado_DeveRetornarNuloImediatamente()
        {
            // Arrange
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Paciente>(It.IsAny<Expression<Func<Paciente, bool>>>()))
                   .ReturnsAsync((Paciente?)null);

            // Act
            var resultado = await _service.ObterDetalhesCompletosAsync(1);

            // Assert
            Assert.Null(resultado);
            _dbMock.Verify(d => d.BuscarOndeAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task ObterDetalhesCompletosAsync_PacienteEncontrado_DeveMontarEstruturaEContabilizarDosesEAgendamentos()
        {
            // Arrange
            var paciente = new Paciente { Id = 1 };
            var tratamentos = new List<Tratamento> { new() { Id = 5, PacienteId = 1 } };
            var remedios = new List<MedicamentoTratamento> { new() { Id = 50, TratamentoId = 5 } };
            var agendamentos = new List<Agendamento>
            {
                new() { Id = 501, MedicamentoTratamentoId = 50, Status = "Tomado" },
                new() { Id = 502, MedicamentoTratamentoId = 50, Status = "Pendente" }
            };

            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Paciente>(It.IsAny<Expression<Func<Paciente, bool>>>()))
                   .ReturnsAsync(paciente);

            _dbMock.Setup(d => d.BuscarOndeAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(tratamentos);

            _dbMock.Setup(d => d.BuscarOndeAsync<MedicamentoTratamento>(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(remedios);

            _dbMock.Setup(d => d.BuscarOndeAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(agendamentos);

            // Act
            var resultado = await _service.ObterDetalhesCompletosAsync(1);

            // Assert
            Assert.NotNull(resultado);
            Assert.Single(resultado!.Tratamentos);

            var tResultado = resultado.Tratamentos.First();
            Assert.Equal(1, tResultado.QtdRemedios);
            Assert.Equal(2, tResultado.TotalDosesTratamento);
            Assert.Equal(1, tResultado.DosesTomadasTratamento);
        }

        #endregion

        #region BuscarPacientePorIdAsync Tests

        [Fact]
        public async Task BuscarPacientePorIdAsync_PacienteEncontrado_DeveRetornarInstanciaDoPacienteCorrespondente()
        {
            // Arrange
            var pacienteEsperado = new Paciente { Id = 7 };
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Paciente>(It.IsAny<Expression<Func<Paciente, bool>>>()))
                   .ReturnsAsync(pacienteEsperado);

            // Act
            var resultado = await _service.BuscarPacientePorIdAsync(7);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(7, resultado!.Id);
        }

        [Fact]
        public async Task BuscarPacientePorIdAsync_PacienteInexistente_DeveRetornarNulo()
        {
            // Arrange
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Paciente>(It.IsAny<Expression<Func<Paciente, bool>>>()))
                   .ReturnsAsync((Paciente?)null);

            // Act
            var resultado = await _service.BuscarPacientePorIdAsync(999);

            // Assert
            Assert.Null(resultado);
        }

        #endregion
    }
}