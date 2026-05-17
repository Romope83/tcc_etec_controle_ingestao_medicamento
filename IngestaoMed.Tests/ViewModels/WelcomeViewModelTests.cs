using Moq;
using Xunit;
using IngestaoMed.Core.ViewModels;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.DTOs;
using System.Threading.Tasks;

namespace IngestaoMed.Tests.ViewModels
{
    public class WelcomeViewModelTests
    {
        private readonly Mock<INavigationService> _navegacaoMock;
        private readonly Mock<IConfigService> _configuracaoMock;
        private readonly WelcomeViewModel _viewModel;

        public WelcomeViewModelTests()
        {
            _navegacaoMock = new Mock<INavigationService>();
            _configuracaoMock = new Mock<IConfigService>();

            _viewModel = new WelcomeViewModel(_navegacaoMock.Object, _configuracaoMock.Object);
        }

        #region Construtor e Inicializacao

        [Fact]
        public void Construtor_DeveCarregarSlidesEConfigurarEstadoInicial()
        {
            // Arrange & Act

            // Assert
            Assert.Equal(3, _viewModel.Slides.Count);
            Assert.Equal(0, _viewModel.PosicaoAtual);
            Assert.Equal("Próximo", _viewModel.TextoBotaoPrincipal);
            Assert.False(_viewModel.EhUltimoSlide);
            Assert.True(_viewModel.MostrarBotaoPular);
        }

        #endregion

        #region OnPosicaoAtualChanged

        [Fact]
        public void OnPosicaoAtualChanged_MudandoParaSlideIntermediario_DeveManterLayoutProximo()
        {
            // Arrange & Act
            _viewModel.PosicaoAtual = 1;

            // Assert
            Assert.False(_viewModel.EhUltimoSlide);
            Assert.Equal("Próximo", _viewModel.TextoBotaoPrincipal);
            Assert.True(_viewModel.MostrarBotaoPular);
        }

        [Fact]
        public void OnPosicaoAtualChanged_MudandoParaUltimoSlide_DeveAtualizarLayoutParaFinalizacao()
        {
            // Arrange & Act
            _viewModel.PosicaoAtual = 2;

            // Assert
            Assert.True(_viewModel.EhUltimoSlide);
            Assert.Equal("Começar Agora", _viewModel.TextoBotaoPrincipal);
            Assert.False(_viewModel.MostrarBotaoPular);
        }

        #endregion

        #region AvancarOuFinalizarCommand

        [Fact]
        public async Task AvancarOuFinalizarCommand_NaoSendoUltimoSlide_DeveIncrementarPosicao()
        {
            // Arrange
            _viewModel.PosicaoAtual = 0;

            // Act
            await _viewModel.AvancarOuFinalizarCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(1, _viewModel.PosicaoAtual);
            _navegacaoMock.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AvancarOuFinalizarCommand_SendoUltimoSlide_DeveFinalizarOnboarding()
        {
            // Arrange
            _viewModel.PosicaoAtual = 2;

            // Act
            await _viewModel.AvancarOuFinalizarCommand.ExecuteAsync(null);

            // Assert
            _configuracaoMock.VerifySet(c => c.EhPrimeiroAcesso = false, Times.Once);
            _navegacaoMock.Verify(n => n.GoToAsync("CuidadorPage"), Times.Once);
        }

        #endregion

        #region FinalizarApresentacaoCommand

        [Fact]
        public async Task FinalizarApresentacaoCommand_Sempre_DeveAtualizarConfiguracaoENavegar()
        {
            // Arrange & Act
            await _viewModel.FinalizarApresentacaoCommand.ExecuteAsync(null);

            // Assert
            _configuracaoMock.VerifySet(c => c.EhPrimeiroAcesso = false, Times.Once);
            _navegacaoMock.Verify(n => n.GoToAsync("CuidadorPage"), Times.Once);
        }

        #endregion
    }
}