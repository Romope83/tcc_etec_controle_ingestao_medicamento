using Moq;
using Xunit;
using IngestaoMed.Core.ViewModels;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace IngestaoMed.Tests.ViewModels
{
    public class MedicamentoViewModelTests
    {
        private readonly Mock<IMedicamentoService> _medicamentoServiceMock;
        private readonly Mock<ITratamentoService> _tratamentoServiceMock;
        private readonly Mock<IDialogService> _dialogServiceMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly Mock<IMediaPickerService> _mediaPickerServiceMock;
        private readonly Mock<IMediaManagerService> _mediaManagerServiceMock;
        private readonly MedicamentoViewModel _viewModel;

        public MedicamentoViewModelTests()
        {
            _medicamentoServiceMock = new Mock<IMedicamentoService>();
            _tratamentoServiceMock = new Mock<ITratamentoService>();
            _dialogServiceMock = new Mock<IDialogService>();
            _navigationServiceMock = new Mock<INavigationService>();
            _mediaPickerServiceMock = new Mock<IMediaPickerService>();
            _mediaManagerServiceMock = new Mock<IMediaManagerService>();

            _viewModel = new MedicamentoViewModel(
                _medicamentoServiceMock.Object,
                _tratamentoServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object,
                _mediaPickerServiceMock.Object,
                _mediaManagerServiceMock.Object);
        }

        #region Testes: InicializarAsync

        [Fact]
        public async Task InicializarAsync_ModoCadastro_DeveLimparPropriedades()
        {
            // Act
            await _viewModel.InicializarAsync(0);

            // Assert
            Assert.Equal(0, _viewModel.Id);
            Assert.Empty(_viewModel.NomeComercial);
            Assert.Empty(_viewModel.Dosagem);
            Assert.Null(_viewModel.UnidadeDosagem);
            Assert.Null(_viewModel.FormaIngestao);
            Assert.Null(_viewModel.FotoPath);
        }

        [Fact]
        public async Task InicializarAsync_ModoEdicaoMedicamentoEncontrado_DevePopularPropriedades()
        {
            // Arrange
            int medicamentoId = 1;
            var medicamento = new Medicamento
            {
                Id = medicamentoId,
                NomeComercial = "Dipirona",
                UnidadeDosagem = "Gotas",
                FormaIngestao = "Oral",
                FotoPath = "caminho/foto.png"
            };

            _medicamentoServiceMock
                .Setup(s => s.BuscarPrimeiroMedicamentoAsync(It.IsAny<Expression<Func<Medicamento, bool>>>()))
                .ReturnsAsync(medicamento);

            // Act
            await _viewModel.InicializarAsync(medicamentoId);

            // Assert
            Assert.Equal(medicamentoId, _viewModel.Id);
            Assert.Equal("Dipirona", _viewModel.NomeComercial);
            Assert.Equal("Gotas", _viewModel.UnidadeDosagem);
            Assert.Equal("Oral", _viewModel.FormaIngestao);
            Assert.Equal("caminho/foto.png", _viewModel.FotoPath);
        }

        #endregion

        #region Testes: SelecionarFotoAsync

        [Fact]
        public async Task SelecionarFotoAsync_AcaoCancelar_NaoDeveAlterarFotoPath()
        {
            // Arrange
            _dialogServiceMock
                .Setup(d => d.DisplayActionSheet(It.IsAny<string>(), "Cancelar", null, It.IsAny<string[]>()))
                .ReturnsAsync("Cancelar");

            // Act
            await _viewModel.SelecionarFotoCommand.ExecuteAsync(null);

            // Assert
            Assert.Null(_viewModel.FotoPath);
            _mediaPickerServiceMock.Verify(m => m.CapturarFotoAsync(), Times.Never);
        }

        [Fact]
        public async Task SelecionarFotoAsync_ModoCadastroEscolherGaleria_DeveDefinirCaminhoTemporario()
        {
            // Arrange
            _viewModel.Id = 0;
            _dialogServiceMock
                .Setup(d => d.DisplayActionSheet(It.IsAny<string>(), "Cancelar", null, It.IsAny<string[]>()))
                .ReturnsAsync("Escolher da Galeria");

            _mediaPickerServiceMock
                .Setup(m => m.SelecionarFotoGaleriaAsync())
                .ReturnsAsync("temp/galeria.png");

            // Act
            await _viewModel.SelecionarFotoCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("temp/galeria.png", _viewModel.FotoPath);
        }

        [Fact]
        public async Task SelecionarFotoAsync_ModoEdicaoTirarFoto_DeveRegistrarImagemDefinitiva()
        {
            // Arrange
            _viewModel.Id = 10;
            _dialogServiceMock
                .Setup(d => d.DisplayActionSheet(It.IsAny<string>(), "Cancelar", null, It.IsAny<string[]>()))
                .ReturnsAsync("Tirar Foto");

            _mediaPickerServiceMock
                .Setup(m => m.CapturarFotoAsync())
                .ReturnsAsync("temp/camera.png");

            _mediaManagerServiceMock
                .Setup(m => m.RegistrarFotoPacienteAsync(10, "temp/camera.png"))
                .ReturnsAsync("definitivo/foto_10.png");

            // Act
            await _viewModel.SelecionarFotoCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("definitivo/foto_10.png", _viewModel.FotoPath);
        }

        #endregion

        #region Testes: SalvarAsync (Validações de Campos)

        [Theory]
        [InlineData("", "Gotas", "Oral")]
        [InlineData("Dipirona", "", "Oral")]
        [InlineData("Dipirona", "Gotas", "")]
        public async Task SalvarAsync_CamposObrigatoriosVazios_DeveExibirAvisoEInterromper(string nome, string unidade, string via)
        {
            // Arrange
            _viewModel.NomeComercial = nome;
            _viewModel.UnidadeDosagem = unidade;
            _viewModel.FormaIngestao = via;

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Aviso", It.IsAny<string>(), "OK"), Times.Once);
            _medicamentoServiceMock.Verify(s => s.AdicionarOuAtualizarMedicamentoAsync(It.IsAny<Medicamento>()), Times.Never);
        }

        #endregion

        #region Testes: SalvarAsync (Modo Cadastro)

        [Fact]
        public async Task SalvarAsync_CadastroMedicamentoDuplicado_DeveExibirErroEInterromper()
        {
            // Arrange
            _viewModel.Id = 0;
            _viewModel.NomeComercial = "Dipirona";
            _viewModel.UnidadeDosagem = "Gotas";
            _viewModel.FormaIngestao = "Oral";

            _medicamentoServiceMock
                .Setup(s => s.ExisteMedicamentoAsync("Dipirona", "Oral"))
                .ReturnsAsync(true);

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Erro", "Este medicamento já está cadastrado com esta forma de ingestão.", "OK"), Times.Once);
            _medicamentoServiceMock.Verify(s => s.AdicionarOuAtualizarMedicamentoAsync(It.IsAny<Medicamento>()), Times.Never);
        }

        [Fact]
        public async Task SalvarAsync_CadastroSucesso_DeveInserirENavegarParaTras()
        {
            // Arrange
            _viewModel.Id = 0;
            _viewModel.NomeComercial = "Dipirona";
            _viewModel.UnidadeDosagem = "Gotas";
            _viewModel.FormaIngestao = "Oral";

            _medicamentoServiceMock.Setup(s => s.ExisteMedicamentoAsync("Dipirona", "Oral")).ReturnsAsync(false);
            _medicamentoServiceMock.Setup(s => s.AdicionarOuAtualizarMedicamentoAsync(It.IsAny<Medicamento>())).ReturnsAsync(true);

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Sucesso", "Medicamento cadastrado com sucesso!", "OK"), Times.Once);
            _navigationServiceMock.Verify(n => n.GoToAsync(".."), Times.Once);
        }

        #endregion

        #region Testes: SalvarAsync (Modo Edição)

        [Fact]
        public async Task SalvarAsync_EdicaoSucesso_DeveAtualizarOriginalENavegarParaTras()
        {
            // Arrange
            int medId = 1;
            var medOriginal = new Medicamento { Id = medId, NomeComercial = "Nome Antigo", UnidadeDosagem = "Gotas", FormaIngestao = "Oral" };

            _medicamentoServiceMock
                .Setup(s => s.BuscarPrimeiroMedicamentoAsync(It.IsAny<Expression<Func<Medicamento, bool>>>()))
                .ReturnsAsync(medOriginal);

            await _viewModel.InicializarAsync(medId);

            // Altera dados na tela/ViewModel
            _viewModel.NomeComercial = "Nome Atualizado";
            _medicamentoServiceMock.Setup(s => s.AdicionarOuAtualizarMedicamentoAsync(medOriginal)).ReturnsAsync(true);

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Nome Atualizado", medOriginal.NomeComercial); // Garante que alterou a referência original
            _dialogServiceMock.Verify(d => d.DisplayAlert("Sucesso", "Medicamento updated com sucesso!", "OK"), Times.Never); // Valida mensagem correta de edição
            _dialogServiceMock.Verify(d => d.DisplayAlert("Sucesso", "Medicamento atualizado com sucesso!", "OK"), Times.Once);
            _navigationServiceMock.Verify(n => n.GoToAsync(".."), Times.Once);
        }

        #endregion

        #region Testes: ExcluirAsync

        [Fact]
        public async Task ExcluirAsync_MedicamentoComTratamentosVinculados_DeveBloquearExclusao()
        {
            // Arrange
            int medId = 1;
            var medOriginal = new Medicamento { Id = medId, NomeComercial = "Dipirona" };

            // Configura o mock para permitir a inicialização correta do componente original
            _medicamentoServiceMock
                .Setup(s => s.BuscarPrimeiroMedicamentoAsync(It.IsAny<Expression<Func<Medicamento, bool>>>()))
                .ReturnsAsync(medOriginal);

            await _viewModel.InicializarAsync(medId);

            // Configura o vínculo que disparará o bloqueio
            var vinculosExistentes = new List<MedicamentoTratamento> { new MedicamentoTratamento { Id = 10, MedicamentoId = medId } };
            _tratamentoServiceMock
                .Setup(t => t.ObterOndeMedicamentoVinculadoAsync(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                .ReturnsAsync(vinculosExistentes);

            // Act
            await _viewModel.ExcluirCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Não Permitido", It.IsAny<string>(), "OK"), Times.Once);
            _medicamentoServiceMock.Verify(s => s.RemoverMedicamentoAsync(It.IsAny<Medicamento>()), Times.Never);
        }

        [Fact]
        public async Task ExcluirAsync_MedicamentoOriginalNulo_DeveEncerrarSilenciosamenteSemVerificarVinculos()
        {
            _viewModel.Id = 1;

            // Act
            await _viewModel.ExcluirCommand.ExecuteAsync(null);

            _tratamentoServiceMock.Verify(t => t.ObterOndeMedicamentoVinculadoAsync(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()), Times.Never);

            _dialogServiceMock.Verify(d => d.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _dialogServiceMock.Verify(d => d.DisplayConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

            _medicamentoServiceMock.Verify(s => s.RemoverMedicamentoAsync(It.IsAny<Medicamento>()), Times.Never);
        }

        [Fact]
        public async Task ExcluirAsync_LivreDeVinculosRecusadoPeloUsuario_DeveInterromper()
        {
            // Arrange
            int medId = 1;
            var medOriginal = new Medicamento { Id = medId, NomeComercial = "Dipirona" };
            _medicamentoServiceMock.Setup(s => s.BuscarPrimeiroMedicamentoAsync(It.IsAny<Expression<Func<Medicamento, bool>>>())).ReturnsAsync(medOriginal);
            await _viewModel.InicializarAsync(medId);

            _tratamentoServiceMock.Setup(t => t.ObterOndeMedicamentoVinculadoAsync(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>())).ReturnsAsync(new List<MedicamentoTratamento>());
            _dialogServiceMock.Setup(d => d.DisplayConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), "Sim", "Não")).ReturnsAsync(false); // Usuário cancelou

            // Act
            await _viewModel.ExcluirCommand.ExecuteAsync(null);

            // Assert
            _medicamentoServiceMock.Verify(s => s.RemoverMedicamentoAsync(It.IsAny<Medicamento>()), Times.Never);
        }

        [Fact]
        public async Task ExcluirAsync_LivreDeVinculosConfirmado_DeveDeletarENavegarParaTras()
        {
            // Arrange
            int medId = 1;
            var medOriginal = new Medicamento { Id = medId, NomeComercial = "Dipirona" };
            _medicamentoServiceMock.Setup(s => s.BuscarPrimeiroMedicamentoAsync(It.IsAny<Expression<Func<Medicamento, bool>>>())).ReturnsAsync(medOriginal);
            await _viewModel.InicializarAsync(medId);

            _tratamentoServiceMock.Setup(t => t.ObterOndeMedicamentoVinculadoAsync(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>())).ReturnsAsync(new List<MedicamentoTratamento>());
            _dialogServiceMock.Setup(d => d.DisplayConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), "Sim", "Não")).ReturnsAsync(true);
            _medicamentoServiceMock.Setup(s => s.RemoverMedicamentoAsync(medOriginal)).ReturnsAsync(true);

            // Act
            await _viewModel.ExcluirCommand.ExecuteAsync(null);

            // Assert
            _medicamentoServiceMock.Verify(s => s.RemoverMedicamentoAsync(medOriginal), Times.Once);
            _dialogServiceMock.Verify(d => d.DisplayAlert("Sucesso", "Medicamento removido com sucesso!", "OK"), Times.Once);
            _navigationServiceMock.Verify(n => n.GoToAsync(".."), Times.Once);
        }

        #endregion
    }
}