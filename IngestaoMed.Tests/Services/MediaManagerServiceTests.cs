using Moq;
using Xunit;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Data;
using IngestaoMed.Core.Models;

namespace IngestaoMed.Tests.Services
{
    public class MediaManagerServiceTests
    {
        private readonly Mock<IFileStorageService> _storageMock;
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly MediaManagerService _service;

        public MediaManagerServiceTests()
        {
            _storageMock = new Mock<IFileStorageService>();
            _dbMock = new Mock<IDatabaseContext>();
            _service = new MediaManagerService(_storageMock.Object, _dbMock.Object);
        }

        [Fact]
        public async Task RegistrarFoto_ComDadosValidos_DeveSalvarNoDiscoENoBanco()
        {
            // Arrange
            var bytes = new byte[] { 0x20, 0x20, 0x20 };
            _storageMock.Setup(s => s.SalvarArquivoAsync(It.IsAny<byte[]>(), It.IsAny<string>()))
                        .ReturnsAsync("/local/path/foto.jpg");

            // Act
            var resultado = await _service.RegistrarFotoMedicamentoAsync(1, bytes);

            // Assert
            // Verifica se tentou salvar no disco
            _storageMock.Verify(s => s.SalvarArquivoAsync(bytes, It.Is<string>(n => n.StartsWith("med_1"))), Times.Once);

            // Verifica se inseriu o registro do caminho no SQLite
            _dbMock.Verify(d => d.InserirAsync(It.Is<AnexoMedia>(a => a.CaminhoLocal == "/local/path/foto.jpg")), Times.Once);

            Assert.Equal("/local/path/foto.jpg", resultado);
        }

        [Fact]
        public async Task RegistrarFoto_ArrayVazio_DeveRetornarNulo()
        {
            // Act
            var resultado = await _service.RegistrarFotoMedicamentoAsync(1, new byte[0]);

            // Assert
            Assert.Null(resultado);
            _storageMock.Verify(s => s.SalvarArquivoAsync(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never);
        }
    }
}