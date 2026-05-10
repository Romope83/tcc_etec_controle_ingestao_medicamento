using Moq;
using Xunit;
using IngestaoMed.Core.Interfaces;

namespace IngestaoMed.Tests.Services
{
    public class EmailServiceTests
    {
        [Fact]
        public async Task EnviarAlerta_ComDadosValidos_DeveRetornarTrue()
        {
            // Arrange
            var mock = new Mock<IEmailService>();
            mock.Setup(e => e.EnviarAlertaFalhaAsync("cuidador@teste.com", "Assunto", "Corpo"))
                .ReturnsAsync(true);

            // Act
            var resultado = await mock.Object.EnviarAlertaFalhaAsync("cuidador@teste.com", "Assunto", "Corpo");

            // Assert
            Assert.True(resultado);
        }

        [Fact]
        public async Task EnviarAlerta_QuandoServidorFalha_DeveRetornarFalse()
        {
            // Arrange
            var mock = new Mock<IEmailService>();
            mock.Setup(e => e.EnviarAlertaFalhaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var resultado = await mock.Object.EnviarAlertaFalhaAsync("erro@teste.com", "Falha", "Corpo");

            // Assert
            Assert.False(resultado);
        }
    }
}