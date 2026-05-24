using Xunit;
using IngestaoMed.Core.Services;

namespace IngestaoMed.Tests.Services
{
    public class SnoozeSchedulerTests
    {
        private readonly SnoozeScheduler _scheduler;

        public SnoozeSchedulerTests()
        {
            _scheduler = new SnoozeScheduler();
        }

        #region ObterTentativas Tests

        [Fact]
        public void ObterTentativas_AgendamentoInexistenteNoContador_DeveRetornarZeroTentativas()
        {
            // Arrange
            int agendamentoId = 1;

            // Act
            int resultado = _scheduler.ObterTentativas(agendamentoId);

            // Assert
            Assert.Equal(0, resultado);
        }

        [Fact]
        public void ObterTentativas_ApósRegistrarUmaSoneca_DeveRetornarUmaTentativa()
        {
            // Arrange
            int agendamentoId = 1;
            _scheduler.RegistrarSoneca(agendamentoId);

            // Act
            int resultado = _scheduler.ObterTentativas(agendamentoId);

            // Assert
            Assert.Equal(1, resultado);
        }

        #endregion

        #region PodeAdiar Tests

        [Fact]
        public void PodeAdiar_TentativasAbaixoDoLimiteMaximo_DeveRetornarTruePermitindoSoneca()
        {
            // Arrange
            int agendamentoId = 1;
            _scheduler.RegistrarSoneca(agendamentoId);
            _scheduler.RegistrarSoneca(agendamentoId);

            // Act
            bool resultado = _scheduler.PodeAdiar(agendamentoId);

            // Assert
            Assert.True(resultado);
        }

        [Fact]
        public void PodeAdiar_TentativasAtingiramOLimiteMaximoDeTres_DeveRetornarFalseBloqueandoSoneca()
        {
            // Arrange
            int agendamentoId = 1;
            _scheduler.RegistrarSoneca(agendamentoId);
            _scheduler.RegistrarSoneca(agendamentoId);
            _scheduler.RegistrarSoneca(agendamentoId);

            // Act
            bool resultado = _scheduler.PodeAdiar(agendamentoId);

            // Assert
            Assert.False(resultado);
        }

        #endregion

        #region EhUltimaTentativa Tests

        [Fact]
        public void EhUltimaTentativa_ContagemIgualADuasSonecas_DeveRetornarTruePoisProximaAtingiraOLimite()
        {
            // Arrange
            int agendamentoId = 1;
            _scheduler.RegistrarSoneca(agendamentoId);
            _scheduler.RegistrarSoneca(agendamentoId);

            // Act
            bool resultado = _scheduler.EhUltimaTentativa(agendamentoId);

            // Assert
            Assert.True(resultado);
        }

        [Fact]
        public void EhUltimaTentativa_ContagemAbaixoDaPenultimaTentativa_DeveRetornarFalse()
        {
            // Arrange
            int agendamentoId = 1;
            _scheduler.RegistrarSoneca(agendamentoId);

            // Act
            bool resultado = _scheduler.EhUltimaTentativa(agendamentoId);

            // Assert
            Assert.False(resultado);
        }

        #endregion

        #region RegistrarSoneca Tests

        [Fact]
        public void RegistrarSoneca_ChamadasMultiplas_DeveIncrementarContadorProgressivamente()
        {
            // Arrange
            int agendamentoId = 1;

            // Act
            _scheduler.RegistrarSoneca(agendamentoId);
            _scheduler.RegistrarSoneca(agendamentoId);

            // Assert
            Assert.Equal(2, _scheduler.ObterTentativas(agendamentoId));
        }

        #endregion

        #region LimparHistorico Tests

        [Fact]
        public void LimparHistorico_AgendamentoComSonecasRegistradas_DeveRemoverDoDicionarioEResetarParaZero()
        {
            // Arrange
            int agendamentoId = 1;
            _scheduler.RegistrarSoneca(agendamentoId);
            _scheduler.RegistrarSoneca(agendamentoId);

            // Act
            _scheduler.LimparHistorico(agendamentoId);

            // Assert
            Assert.Equal(0, _scheduler.ObterTentativas(agendamentoId));
        }

        [Fact]
        public void LimparHistorico_AgendamentoInexistente_DeveExecutarSemLancamentoDeExcecao()
        {
            // Arrange
            int agendamentoId = 99;

            // Act & Assert
            var excecao = Record.Exception(() => _scheduler.LimparHistorico(agendamentoId));
            Assert.Null(excecao);
        }

        #endregion
    }
}