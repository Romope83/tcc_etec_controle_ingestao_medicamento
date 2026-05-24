using Moq;
using Xunit;
using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IngestaoMed.Tests.Services
{
    public class AgendamentoServiceTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly Mock<IAlarmService> _alarmServiceMock;
        private readonly AgendamentoService _service;

        public AgendamentoServiceTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _alarmServiceMock = new Mock<IAlarmService>();

            _service = new AgendamentoService(
                _dbMock.Object,
                _alarmServiceMock.Object
            );
        }

        #region Método: ObterVinculoPorIdAsync

        [Fact]
        public async Task ObterVinculoPorIdAsync_QuandoRegistroExiste_DeveRetornarMedicamentoTratamento()
        {
            // Arrange
            var vinculoFake = new MedicamentoTratamento { Id = 1, Dosagem = "1 Comprimido" };
            _dbMock.Setup(db => db.BuscarPrimeiroAsync(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(vinculoFake);

            // Act
            var resultado = await _service.ObterVinculoPorIdAsync(1);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(1, resultado.Id);
            Assert.Equal("1 Comprimido", resultado.Dosagem);
        }

        #endregion

        #region Método: ObterMedicamentoPorIdAsync

        [Fact]
        public async Task ObterMedicamentoPorIdAsync_QuandoRegistroExiste_DeveRetornarMedicamento()
        {
            // Arrange
            var medFake = new Medicamento { Id = 10, NomeComercial = "Dipirona" };
            _dbMock.Setup(db => db.BuscarPrimeiroAsync(It.IsAny<Expression<Func<Medicamento, bool>>>()))
                   .ReturnsAsync(medFake);

            // Act
            var resultado = await _service.ObterMedicamentoPorIdAsync(10);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(10, resultado.Id);
            Assert.Equal("Dipirona", resultado.NomeComercial);
        }

        #endregion

        #region Método: ObterTodosMedicamentosAsync

        [Fact]
        public async Task ObterTodosMedicamentosAsync_QuandoChamado_DeveRetornarListaDeMedicamentos()
        {
            // Arrange
            var listaFake = new List<Medicamento>
            {
                new Medicamento { Id = 1, NomeComercial = "Med A" },
                new Medicamento { Id = 2, NomeComercial = "Med B" }
            };
            _dbMock.Setup(db => db.BuscarTodosAsync<Medicamento>())
                   .ReturnsAsync(listaFake);

            // Act
            var resultado = await _service.ObterTodosMedicamentosAsync();

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count);
        }

        #endregion

        #region Método: ObterDosesPorVinculoIdAsync

        [Fact]
        public async Task ObterDosesPorVinculoIdAsync_QuandoDosesExistem_DeveRetornarListaOrdenada()
        {
            // Arrange
            var dosesFake = new List<Agendamento>
            {
                new Agendamento { Id = 1, MedicamentoTratamentoId = 5 },
                new Agendamento { Id = 2, MedicamentoTratamentoId = 5 }
            };
            _dbMock.Setup(db => db.BuscarOndeAsync(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(dosesFake);

            // Act
            var resultado = await _service.ObterDosesPorVinculoIdAsync(5);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count);
        }

        [Fact]
        public async Task ObterDosesPorVinculoIdAsync_QuandoNulo_DeveRetornarListaVazia()
        {
            // Arrange
            _dbMock.Setup(db => db.BuscarOndeAsync(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync((List<Agendamento>)null);

            // Act
            var resultado = await _service.ObterDosesPorVinculoIdAsync(99);

            // Assert
            Assert.NotNull(resultado);
            Assert.Empty(resultado);
        }

        #endregion

        #region Método: ObterTratamentoPorIdAsync

        [Fact]
        public async Task ObterTratamentoPorIdAsync_QuandoRegistroExiste_DeveRetornarTratamento()
        {
            // Arrange
            var tratamentoFake = new Tratamento { Id = 3 };
            _dbMock.Setup(db => db.BuscarPrimeiroAsync(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(tratamentoFake);

            // Act
            var resultado = await _service.ObterTratamentoPorIdAsync(3);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(3, resultado.Id);
        }

        #endregion

        #region Método: DeletarDosesEAlarmeAsync

        [Fact]
        public async Task DeletarDosesEAlarmeAsync_ComDosesValidas_DeveExcluirESincronizarAlarme()
        {
            // Arrange
            var dosesParaDeletar = new List<Agendamento>
            {
                new Agendamento { Id = 1 },
                new Agendamento { Id = 2 }
            };

            // AJUSTADO: Voltando para Task.CompletedTask pois ExcluirAsync é void/Task puro
            _dbMock.Setup(db => db.ExcluirAsync(It.IsAny<Agendamento>())).ReturnsAsync(2);
            _dbMock.Setup(db => db.BuscarOndeAsync(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(new List<Agendamento>());

            // Act
            var resultado = await _service.DeletarDosesEAlarmeAsync(dosesParaDeletar);

            // Assert
            Assert.True(resultado);
            _dbMock.Verify(db => db.ExcluirAsync(It.IsAny<Agendamento>()), Times.Exactly(2));
            _alarmServiceMock.Verify(a => a.SincronizarJanelaAlarmesAsync(It.IsAny<List<Agendamento>>()), Times.Once);
        }

        [Fact]
        public async Task DeletarDosesEAlarmeAsync_QuandoLancarExcecao_DeveRetornarFalso()
        {
            // Arrange
            var dosesParaDeletar = new List<Agendamento> { new Agendamento { Id = 1 } };
            _dbMock.Setup(db => db.ExcluirAsync(It.IsAny<Agendamento>())).ThrowsAsync(new Exception("Falha de Banco"));

            // Act
            var resultado = await _service.DeletarDosesEAlarmeAsync(dosesParaDeletar);

            // Assert
            Assert.False(resultado);
        }

        #endregion

        #region Método: SalvarNovoAgendamentoAsync

        [Fact]
        public async Task SalvarNovoAgendamentoAsync_ComDadosValidos_DeveInserirVinculoEDoses()
        {
            // Arrange
            var med = new Medicamento { Id = 4 };
            var doses = new List<Agendamento> { new Agendamento(), new Agendamento() };

            // AJUSTADO: Configurando os setups com Returns(Task.CompletedTask) condizente com a assinatura do IDatabaseContext
            _dbMock.Setup(db => db.InserirAsync(It.IsAny<MedicamentoTratamento>())).ReturnsAsync(true);
            _dbMock.Setup(db => db.InserirAsync(It.IsAny<Agendamento>())).ReturnsAsync(true);
            _dbMock.Setup(db => db.BuscarOndeAsync(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(new List<Agendamento>());

            // Act
            var resultado = await _service.SalvarNovoAgendamentoAsync(
                tratamentoId: 10,
                medicamento: med,
                dosagem: "2 Comprimidos",
                intervaloHoras: 8,
                ativo: true,
                dosesGeradas: doses
            );

            // Assert
            Assert.True(resultado);
            _dbMock.Verify(db => db.InserirAsync(It.IsAny<MedicamentoTratamento>()), Times.Once);
            _dbMock.Verify(db => db.InserirAsync(It.IsAny<Agendamento>()), Times.Exactly(2));
            _alarmServiceMock.Verify(a => a.SincronizarJanelaAlarmesAsync(It.IsAny<List<Agendamento>>()), Times.Once);
        }

        [Fact]
        public async Task SalvarNovoAgendamentoAsync_QuandoLancarExcecao_DeveRetornarFalso()
        {
            // Arrange
            _dbMock.Setup(db => db.InserirAsync(It.IsAny<MedicamentoTratamento>())).ThrowsAsync(new Exception("Erro"));

            // Act
            var resultado = await _service.SalvarNovoAgendamentoAsync(1, new Medicamento(), "1", 1, true, new List<Agendamento>());

            // Assert
            Assert.False(resultado);
        }

        #endregion

        #region Método: AtualizarAgendamentoExistenteAsync

        [Fact]
        public async Task AtualizarAgendamentoExistenteAsync_QuandoRegistroExiste_DeveAtualizar()
        {
            // Arrange
            var vinculoExistente = new MedicamentoTratamento { Id = 1 };
            _dbMock.Setup(db => db.BuscarPrimeiroAsync(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(vinculoExistente);

            // AJUSTADO: Retornando Task.CompletedTask para o método AtualizarAsync da interface
            _dbMock.Setup(db => db.AtualizarAsync(It.IsAny<MedicamentoTratamento>())).ReturnsAsync(1);
            _dbMock.Setup(db => db.BuscarOndeAsync(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(new List<Agendamento>());

            // Act
            var resultado = await _service.AtualizarAgendamentoExistenteAsync(1, "Nova Dose", 6, false);

            // Assert
            Assert.True(resultado);
            Assert.Equal("Nova Dose", vinculoExistente.Dosagem);
            Assert.Equal(6, vinculoExistente.IntervaloHoras);
            Assert.False(vinculoExistente.Ativo);
            _dbMock.Verify(db => db.AtualizarAsync(vinculoExistente), Times.Once);
            _alarmServiceMock.Verify(a => a.SincronizarJanelaAlarmesAsync(It.IsAny<List<Agendamento>>()), Times.Once);
        }

        [Fact]
        public async Task AtualizarAgendamentoExistenteAsync_QuandoRegistroNaoExiste_DeveRetornarFalso()
        {
            // Arrange
            _dbMock.Setup(db => db.BuscarPrimeiroAsync(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync((MedicamentoTratamento)null);

            // Act
            var resultado = await _service.AtualizarAgendamentoExistenteAsync(999, "Dose", 8, true);

            // Assert
            Assert.False(resultado);
            _dbMock.Verify(db => db.AtualizarAsync(It.IsAny<MedicamentoTratamento>()), Times.Never);
        }

        #endregion

        #region Método: SincronizarFilaDeAlarmesAsync

        [Fact]
        public async Task SincronizarFilaDeAlarmesAsync_QuandoPossuiMaisDeQuarentaDoses_DeveEnviarApenasAsQuarentaPrimeiras()
        {
            // Arrange
            var dosesFake = new List<Agendamento>();
            var dataBase = DateTime.Now.AddHours(1);

            for (int i = 1; i <= 50; i++)
            {
                dosesFake.Add(new Agendamento
                {
                    Id = i,
                    Status = "Pendente",
                    ProximoAlarme = dataBase.AddMinutes(i)
                });
            }

            _dbMock.Setup(db => db.BuscarOndeAsync(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(dosesFake);

            List<Agendamento> dosesSincronizadas = null;
            _alarmServiceMock.Setup(a => a.SincronizarJanelaAlarmesAsync(It.IsAny<List<Agendamento>>()))
                             .Callback<List<Agendamento>>(list => dosesSincronizadas = list)
                             .Returns(Task.CompletedTask);

            // Act
            await _service.SincronizarFilaDeAlarmesAsync();

            // Assert
            Assert.NotNull(dosesSincronizadas);
            Assert.Equal(40, dosesSincronizadas.Count);
            Assert.Equal(1, dosesSincronizadas.First().Id);
            Assert.Equal(40, dosesSincronizadas.Last().Id);
        }

        [Fact]
        public async Task SincronizarFilaDeAlarmesAsync_QuandoFilaVazia_NaoDeveChamarAlarmService()
        {
            // Arrange
            _dbMock.Setup(db => db.BuscarOndeAsync(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync((List<Agendamento>)null);

            // Act
            await _service.SincronizarFilaDeAlarmesAsync();

            // Assert
            _alarmServiceMock.Verify(a => a.SincronizarJanelaAlarmesAsync(It.IsAny<List<Agendamento>>()), Times.Never);
        }

        #endregion
    }
}