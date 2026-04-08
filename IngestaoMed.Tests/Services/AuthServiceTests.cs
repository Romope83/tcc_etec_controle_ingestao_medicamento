using Moq;
using Xunit;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Data;
using BCrypt.Net;

namespace IngestaoMed.Tests.Services
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task RegistrarCuidador_DeveGerarHashESalvarComSucesso()
        {
            // Arrange
            var mockDb = new Mock<IDatabaseContext>();
            var authService = new AuthService(mockDb.Object);

            var cuidador = new Cuidador { Nome = "João", Email = "joao@teste.com" };
            string senhaLimpa = "Senha@123";

            mockDb.Setup(d => d.InserirAsync(It.IsAny<Cuidador>())).ReturnsAsync(true);

            var resultado = await authService.RegistrarCuidador(cuidador, senhaLimpa);


            Assert.True(resultado);
            Assert.NotEqual(senhaLimpa, cuidador.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify(senhaLimpa, cuidador.PasswordHash));
        }
    }
}