using Moq;
using Xunit;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Data;
using IngestaoMed.Core.Models;
using System;
using System.IO;
using System.Threading.Tasks;

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

        #region RegistrarFotoMedicamentoAsync Tests

        [Fact]
        public async Task RegistrarFotoMedicamentoAsync_ComDadosValidos_DeveSalvarArquivoEGravarNoBanco()
        {
            // Arrange
            int medicamentoId = 123;
            byte[] fotosBytes = { 1, 2, 3, 4 };
            string caminhoEsperado = "/storage/med_123_random.jpg";

            _storageMock.Setup(s => s.SalvarArquivoAsync(fotosBytes, It.IsAny<string>()))
                        .ReturnsAsync(caminhoEsperado);

            // Act
            string? resultado = await _service.RegistrarFotoMedicamentoAsync(medicamentoId, fotosBytes);

            // Assert
            Assert.Equal(caminhoEsperado, resultado);

            _storageMock.Verify(s => s.SalvarArquivoAsync(fotosBytes, It.Is<string>(nome => nome.StartsWith($"med_{medicamentoId}_"))), Times.Once);

            _dbMock.Verify(d => d.InserirAsync(It.Is<AnexoMedia>(a =>
                a.ReferenciaId == medicamentoId &&
                a.TipoReferencia == "Medicamento" &&
                a.CaminhoLocal == caminhoEsperado)), Times.Once);
        }

        [Fact]
        public async Task RegistrarFotoMedicamentoAsync_BytesNulosOuVazios_DeveRetornarNuloSemProcessar()
        {
            // Arrange
            int medicamentoId = 123;
            byte[]? fotosBytesVazio = Array.Empty<byte>();

            // Act
            string? resultadoNulo = await _service.RegistrarFotoMedicamentoAsync(medicamentoId, null!);
            string? resultadoVazio = await _service.RegistrarFotoMedicamentoAsync(medicamentoId, fotosBytesVazio);

            // Assert
            Assert.Null(resultadoNulo);
            Assert.Null(resultadoVazio);

            _storageMock.Verify(s => s.SalvarArquivoAsync(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never);
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<AnexoMedia>()), Times.Never);
        }

        #endregion

        #region RegistrarFotoPacienteAsync Tests

        [Fact]
        public async Task RegistrarFotoPacienteAsync_ArquivoInexistente_DeveRetornarNuloSemProcessar()
        {
            // Arrange
            int pacienteId = 456;
            string caminhoInvalido = "caminho_que_nao_existe_no_disco.jpg";

            // Act
            string? resultado = await _service.RegistrarFotoPacienteAsync(pacienteId, caminhoInvalido);

            // Assert
            Assert.Null(resultado);
            _storageMock.Verify(s => s.SalvarArquivoAsync(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never);
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<AnexoMedia>()), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task RegistrarFotoPacienteAsync_CaminhoInvalidoOuEspacoEmBranco_DeveRetornarNulo(string? caminhoInvalido)
        {
            // Act
            string? resultado = await _service.RegistrarFotoPacienteAsync(1, caminhoInvalido!);

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public async Task RegistrarFotoPacienteAsync_ComArquivoValido_DeveSalvarArquivoEGravarNoBanco()
        {
            // Arrange
            int pacienteId = 789;
            string caminhoTemporario = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.jpg");
            byte[] bytesEsperados = { 5, 6, 7, 8 };
            string caminhoFinalEsperado = "/storage/paciente_789_random.jpg";

            // Cria o arquivo físico temporário necessário para passar pela validação File.Exists
            await File.WriteAllBytesAsync(caminhoTemporario, bytesEsperados);

            _storageMock.Setup(s => s.SalvarArquivoAsync(It.IsAny<byte[]>(), It.IsAny<string>()))
                        .ReturnsAsync(caminhoFinalEsperado);

            try
            {
                // Act
                string? resultado = await _service.RegistrarFotoPacienteAsync(pacienteId, caminhoTemporario);

                // Assert
                Assert.Equal(caminhoFinalEsperado, resultado);

                _storageMock.Verify(s => s.SalvarArquivoAsync(
                    It.Is<byte[]>(b => b.Length == bytesEsperados.Length),
                    It.Is<string>(nome => nome.StartsWith($"paciente_{pacienteId}_"))),
                    Times.Once);

                _dbMock.Verify(d => d.InserirAsync(It.Is<AnexoMedia>(a =>
                    a.ReferenciaId == pacienteId &&
                    a.TipoReferencia == "Paciente" &&
                    a.CaminhoLocal == caminhoFinalEsperado)), Times.Once);
            }
            finally
            {
                // Limpa o arquivo temporário criado para o teste
                if (File.Exists(caminhoTemporario))
                {
                    File.Delete(caminhoTemporario);
                }
            }
        }

        #endregion
    }
}