using Moq;
using Xunit;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Data;
using System.Linq.Expressions;

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

        [Fact]
        public async Task ObterTimeline_DeveMesclarLogsEFotosOrdenadosPorData()
        {
            // Arrange
            var dataRecente = DateTime.Now;
            var dataAntiga = DateTime.Now.AddDays(-1);

            var logs = new List<LogMedicamento> {
                new LogMedicamento { Id = 1, DataHora = dataAntiga, Mensagem = "Tomou Dipirona" }
            };
            var fotos = new List<AnexoMedia> {
                new AnexoMedia { Id = 1, DataCriacao = dataRecente, CaminhoLocal = "foto.jpg" }
            };

            _dbMock.Setup(d => d.BuscarTodosAsync<LogMedicamento>()).ReturnsAsync(logs);
            _dbMock.Setup(d => d.BuscarTodosAsync<AnexoMedia>()).ReturnsAsync(fotos);

            // Act
            var resultado = await _service.ObterHistoricoCompletoAsync();

            // Assert
            Assert.Equal(2, resultado.Count);
            Assert.Equal("foto.jpg", resultado[0].CaminhoImagem); // A mais recente (foto) deve vir primeiro
            Assert.Equal("Tomou Dipirona", resultado[1].Titulo);
        }
    }
}