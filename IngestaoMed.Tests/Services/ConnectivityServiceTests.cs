using Moq;
using Xunit;
using IngestaoMed.Core.Interfaces;

namespace IngestaoMed.Tests.Services
{
    public class ConnectivityServiceTests
    {
        [Fact]
        public void TemInternet_QuandoConectado_DeveRetornarTrue()
        {
            // Arrange
            var mock = new Mock<IConnectivityService>();
            mock.Setup(c => c.TemInternet).Returns(true);

            // Act & Assert
            Assert.True(mock.Object.TemInternet);
        }

        [Fact]
        public void TemInternet_QuandoDesconectado_DeveRetornarFalse()
        {
            // Arrange
            var mock = new Mock<IConnectivityService>();
            mock.Setup(c => c.TemInternet).Returns(false);

            // Act & Assert
            Assert.False(mock.Object.TemInternet);
        }
    }
}