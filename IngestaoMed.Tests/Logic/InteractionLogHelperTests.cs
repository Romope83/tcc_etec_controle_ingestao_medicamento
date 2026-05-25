using IngestaoMed.Core.Logic;
using Xunit;

namespace IngestaoMed.Tests.Core.Logic
{
    public class InteractionLogHelperTests
    {
        private readonly InteractionLogHelper _helper;

        public InteractionLogHelperTests()
        {
            _helper = new InteractionLogHelper();
        }

        [Fact]
        public void GerarDescricao_DeveRetornarFormatoCorreto()
        {
            // Arrange
            var acao = "Tomei";
            var agendamentoId = 42;
            var horarioEsperado = DateTime.Now.ToString("HH:mm");

            // Act
            var resultado = _helper.GerarDescricao(acao, agendamentoId);

            // Assert
            Assert.Contains($"Ação: {acao}", resultado);
            Assert.Contains($"Agendamento: {agendamentoId}", resultado);
            Assert.Contains($"Horário: {horarioEsperado}", resultado);
        }

        [Theory]
        [InlineData("Soneca")]
        [InlineData("Pulei")]
        public void GerarDescricao_DeveAceitarDiferentesAcoes(string acao)
        {
            // Act
            var resultado = _helper.GerarDescricao(acao, 10);

            // Assert
            Assert.Contains($"Ação: {acao}", resultado);
        }
    }
}