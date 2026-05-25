using Moq;
using Xunit;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Data;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IngestaoMed.Tests.Services
{
    public class TimelineServiceTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly TimelineService _service;

        public TimelineServiceTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _service = new TimelineService(_dbMock.Object);
        }

        #region ObterHistoricoCompletoAsync Tests

        [Fact]
        public async Task ObterHistoricoCompletoAsync_BancoVazio_DeveRetornarListaVazia()
        {
            // Arrange
            _dbMock.Setup(d => d.BuscarTodosAsync<LogMedicamento>())
                   .ReturnsAsync(new List<LogMedicamento>());

            _dbMock.Setup(d => d.BuscarTodosAsync<AnexoMedia>())
                   .ReturnsAsync(new List<AnexoMedia>());

            // Act
            var resultado = await _service.ObterHistoricoCompletoAsync();

            // Assert
            Assert.NotNull(resultado);
            Assert.Empty(resultado);
        }

        [Fact]
        public async Task ObterHistoricoCompletoAsync_ApenasLogsExistentes_DeveRetornarListaMapeadaComTipoLog()
        {
            // Arrange
            var logs = new List<LogMedicamento>
            {
                new() { Id = 1, DataHora = new DateTime(2026, 05, 20), Mensagem = "Medicamento Tomado" }
            };

            _dbMock.Setup(d => d.BuscarTodosAsync<LogMedicamento>())
                   .ReturnsAsync(logs);

            _dbMock.Setup(d => d.BuscarTodosAsync<AnexoMedia>())
                   .ReturnsAsync(new List<AnexoMedia>());

            // Act
            var resultado = await _service.ObterHistoricoCompletoAsync();

            // Assert
            Assert.Single(resultado);
            var item = resultado.First();
            Assert.Equal(1, item.Id);
            Assert.Equal("LOG", item.Tipo);
            Assert.Equal("Medicamento Tomado", item.Titulo);
            Assert.Null(item.CaminhoImagem);
        }

        [Fact]
        public async Task ObterHistoricoCompletoAsync_ApenasFotosExistentes_DeveRetornarListaMapeadaComTipoFotoECaminhoDaImagem()
        {
            // Arrange
            var fotos = new List<AnexoMedia>
            {
                new() { Id = 10, DataCriacao = new DateTime(2026, 05, 21), CaminhoLocal = "foto.jpg" }
            };

            _dbMock.Setup(d => d.BuscarTodosAsync<LogMedicamento>())
                   .ReturnsAsync(new List<LogMedicamento>());

            _dbMock.Setup(d => d.BuscarTodosAsync<AnexoMedia>())
                   .ReturnsAsync(fotos);

            // Act
            var resultado = await _service.ObterHistoricoCompletoAsync();

            // Assert
            Assert.Single(resultado);
            var item = resultado.First();
            Assert.Equal(10, item.Id);
            Assert.Equal("FOTO", item.Tipo);
            Assert.Equal("Anexo de Mídia", item.Titulo);
            Assert.Equal("foto.jpg", item.CaminhoImagem);
        }

        [Fact]
        public async Task ObterHistoricoCompletoAsync_DadosMistos_DeveRetornarUniaoOrdenadaPorDataDecrescente()
        {
            // Arrange
            var logs = new List<LogMedicamento>
            {
                new() { Id = 1, DataHora = new DateTime(2026, 05, 10), Mensagem = "Log Antigo" },
                new() { Id = 2, DataHora = new DateTime(2026, 05, 30), Mensagem = "Log Recente" }
            };

            var fotos = new List<AnexoMedia>
            {
                new() { Id = 10, DataCriacao = new DateTime(2026, 05, 20), CaminhoLocal = "foto.jpg" }
            };

            _dbMock.Setup(d => d.BuscarTodosAsync<LogMedicamento>())
                   .ReturnsAsync(logs);

            _dbMock.Setup(d => d.BuscarTodosAsync<AnexoMedia>())
                   .ReturnsAsync(fotos);

            // Act
            var resultado = await _service.ObterHistoricoCompletoAsync();

            // Assert
            Assert.Equal(3, resultado.Count);
            Assert.Equal("Log Recente", resultado[0].Titulo); // 30/05
            Assert.Equal("Anexo de Mídia", resultado[1].Titulo); // 20/05
            Assert.Equal("Log Antigo", resultado[2].Titulo); // 10/05
        }

        #endregion
    }
}