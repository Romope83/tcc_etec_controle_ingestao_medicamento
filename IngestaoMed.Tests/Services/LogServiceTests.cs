
using Moq;
using Xunit;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Data;
using IngestaoMed.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IngestaoMed.Tests.Services
{
    public class LogServiceTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly LogService _service;

        public LogServiceTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _service = new LogService(_dbMock.Object);
        }

        #region RegistrarAsync

        [Fact]
        public async Task RegistrarAsync_ConfirmacaoDireta_DeveCalcularAtrasoCorretamente()
        {
            // Arrange
            int id = 1;
            var horarioProgramado = DateTime.Now.AddMinutes(-15);
            var agendamento = new Agendamento { Id = id, ProximoAlarme = horarioProgramado };

            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(agendamento);

            // Act
            await _service.RegistrarAsync(id, TipoEventoLog.ConfirmacaoDireta);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.Is<LogEvento>(l =>
                l.AgendamentoId == id &&
                l.Tipo == TipoEventoLog.ConfirmacaoDireta &&
                l.MinutosAtraso >= 15
            )), Times.Once);
        }

        [Fact]
        public async Task RegistrarAsync_ConfirmacaoComSoneca_DeveCalcularAtrasoCorretamente()
        {
            // Arrange
            int id = 1;
            var horarioProgramado = DateTime.Now.AddMinutes(-20);
            var agendamento = new Agendamento { Id = id, ProximoAlarme = horarioProgramado };

            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(agendamento);

            // Act
            await _service.RegistrarAsync(id, TipoEventoLog.ConfirmacaoComSoneca);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.Is<LogEvento>(l =>
                l.AgendamentoId == id &&
                l.Tipo == TipoEventoLog.ConfirmacaoComSoneca &&
                l.MinutosAtraso >= 20
            )), Times.Once);
        }

        [Fact]
        public async Task RegistrarAsync_QuandoAdiantadoOuNoHorarioExato_DeveGravarAtrasoComoZero()
        {
            // Arrange
            int id = 1;
            var horarioProgramado = DateTime.Now.AddMinutes(10);
            var agendamento = new Agendamento { Id = id, ProximoAlarme = horarioProgramado };

            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(agendamento);

            // Act
            await _service.RegistrarAsync(id, TipoEventoLog.ConfirmacaoDireta);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.Is<LogEvento>(l =>
                l.MinutosAtraso == 0
            )), Times.Once);
        }

        [Fact]
        public async Task RegistrarAsync_Soneca_NaoDeveContabilizarAtrasoComoIngestao()
        {
            // Arrange
            int id = 1;
            var agendamento = new Agendamento { Id = id, ProximoAlarme = DateTime.Now.AddMinutes(-5) };
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(agendamento);

            // Act
            await _service.RegistrarAsync(id, TipoEventoLog.SonecaDisparada);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.Is<LogEvento>(l =>
                l.Tipo == TipoEventoLog.SonecaDisparada &&
                l.MinutosAtraso == 0
            )), Times.Once);
        }

        [Fact]
        public async Task RegistrarAsync_AgendamentoInexistente_DeveEncerrarSilenciosamente()
        {
            // Arrange
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync((Agendamento)null!);

            // Act
            await _service.RegistrarAsync(999, TipoEventoLog.ConfirmacaoDireta);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<LogEvento>()), Times.Never);
        }

        #endregion

        #region ObterLogsPorTratamentoAsync

        [Fact]
        public async Task ObterLogsPorTratamentoAsync_QuandoExistiremLogsDoTratamento_DeveFiltrarERetornarLista()
        {
            // Arrange
            int tratamentoAlvoId = 5;
            var tratamentoAlvo = new Tratamento { Id = tratamentoAlvoId };
            var outroTratamento = new Tratamento { Id = 99 };

            var agendamentoAlvo = new Agendamento { Id = 1, Tratamento = tratamentoAlvo };
            var outroAgendamento = new Agendamento { Id = 2, Tratamento = outroTratamento };

            var listaLogs = new List<LogEvento>
            {
                new LogEvento { Id = 10, Agendamento = agendamentoAlvo },
                new LogEvento { Id = 11, Agendamento = agendamentoAlvo },
                new LogEvento { Id = 12, Agendamento = outroAgendamento },
                new LogEvento { Id = 13, Agendamento = null }
            };

            _dbMock.Setup(d => d.BuscarTodosAsync<LogEvento>())
                   .ReturnsAsync(listaLogs);

            // Act
            var resultado = await _service.ObterLogsPorTratamentoAsync(tratamentoAlvoId);

            // Assert
            Assert.Equal(2, resultado.Count);
            Assert.All(resultado, log => Assert.Equal(tratamentoAlvoId, log.Agendamento?.Tratamento?.Id));
        }

        [Fact]
        public async Task ObterLogsPorTratamentoAsync_QuandoNaoHouverLogsDoTratamento_DeveRetornarListaVazia()
        {
            // Arrange
            int tratamentoId = 1;
            var listaLogs = new List<LogEvento>
            {
                new LogEvento { Id = 1, Agendamento = new Agendamento { Id = 2, Tratamento = new Tratamento { Id = 3 } } }
            };

            _dbMock.Setup(d => d.BuscarTodosAsync<LogEvento>())
                   .ReturnsAsync(listaLogs);

            // Act
            var resultado = await _service.ObterLogsPorTratamentoAsync(tratamentoId);

            // Assert
            Assert.Empty(resultado);
        }

        [Fact]
        public async Task ObterLogsPorTratamentoAsync_QuandoTabelaDeLogsEstiverVazia_DeveRetornarListaVazia()
        {
            // Arrange
            _dbMock.Setup(d => d.BuscarTodosAsync<LogEvento>())
                   .ReturnsAsync(new List<LogEvento>());

            // Act
            var resultado = await _service.ObterLogsPorTratamentoAsync(1);

            // Assert
            Assert.Empty(resultado);
        }

        #endregion
    }
}
