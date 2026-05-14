using IngestaoMed.Core.Services;
using Xunit;

namespace IngestaoMed.Tests.Services
{
    public class SnoozeSchedulerTests
    {
        private readonly SnoozeScheduler _scheduler;

        public SnoozeSchedulerTests()
        {
            _scheduler = new SnoozeScheduler();
        }

        [Fact]
        public void PodeAdiar_DeveRetornarTrue_QuandoForAPrimeiraTentativa()
        {
            // Act
            var resultado = _scheduler.PodeAdiar(1);

            // Assert
            Assert.True(resultado);
        }

        [Fact]
        public void PodeAdiar_DeveRetornarFalse_QuandoExcederLimiteDeTres()
        {
            // Arrange
            int agendamentoId = 1;
            _scheduler.RegistrarSoneca(agendamentoId); // 1
            _scheduler.RegistrarSoneca(agendamentoId); // 2
            _scheduler.RegistrarSoneca(agendamentoId); // 3

            // Act
            var resultado = _scheduler.PodeAdiar(agendamentoId);

            // Assert
            Assert.False(resultado);
        }

        [Fact]
        public void LimparHistorico_DevePermitirSonecaNovamente_AposLimpeza()
        {
            // Arrange
            int agendamentoId = 1;
            _scheduler.RegistrarSoneca(agendamentoId);
            _scheduler.RegistrarSoneca(agendamentoId);
            _scheduler.RegistrarSoneca(agendamentoId);

            // Act
            _scheduler.LimparHistorico(agendamentoId);
            var resultado = _scheduler.PodeAdiar(agendamentoId);

            // Assert
            Assert.True(resultado);
        }
        [Fact]
        public void ObterTentativas_DeveRetornarZero_QuandoNaoHouverRegistros()
        {
            // Act
            var tentativas = _scheduler.ObterTentativas(99);

            // Assert
            Assert.Equal(0, tentativas);
        }

        [Fact]
        public void ObterTentativas_DeveRetornarValorCorreto_AposMultiplosRegistros()
        {
            // Arrange
            int agendamentoId = 1;
            _scheduler.RegistrarSoneca(agendamentoId);
            _scheduler.RegistrarSoneca(agendamentoId);

            // Act
            var tentativas = _scheduler.ObterTentativas(agendamentoId);

            // Assert
            Assert.Equal(2, tentativas);
        }

        [Fact]
        public void EhUltimaTentativa_DeveRetornarTrue_ApenasNaSegundaSoneca()
        {
            // Arrange
            int agendamentoId = 1;

            // Primeira soneca (tentativas = 1)
            _scheduler.RegistrarSoneca(agendamentoId);
            Assert.False(_scheduler.EhUltimaTentativa(agendamentoId));

            // Segunda soneca (tentativas = 2) - Próxima será a 3ª (limite)
            _scheduler.RegistrarSoneca(agendamentoId);

            // Act
            var resultado = _scheduler.EhUltimaTentativa(agendamentoId);

            // Assert
            Assert.True(resultado);
        }

        [Fact]
        public void EhUltimaTentativa_DeveRetornarFalse_AposExcederOuLimpar()
        {
            // Arrange
            int agendamentoId = 1;
            _scheduler.RegistrarSoneca(agendamentoId);
            _scheduler.RegistrarSoneca(agendamentoId); // Eh ultima (2 de 3)
            _scheduler.RegistrarSoneca(agendamentoId); // Já atingiu o limite (3 de 3)

            // Act & Assert
            Assert.False(_scheduler.EhUltimaTentativa(agendamentoId));

            _scheduler.LimparHistorico(agendamentoId);
            Assert.False(_scheduler.EhUltimaTentativa(agendamentoId));
        }
    }
}