using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.ViewModels;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IngestaoMed.Tests.ViewModels
{
    public class PacienteViewModelTests
    {
        private readonly Mock<IPacienteService> _pacienteServiceMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly Mock<IDialogService> _dialogServiceMock;
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IMediaPickerService> _mediaPickerServiceMock;
        private readonly Mock<IMediaManagerService> _mediaManagerServiceMock;
        private readonly PacienteViewModel _viewModel;

        public PacienteViewModelTests()
        {
            _pacienteServiceMock = new Mock<IPacienteService>();
            _navigationServiceMock = new Mock<INavigationService>();
            _dialogServiceMock = new Mock<IDialogService>();
            _authServiceMock = new Mock<IAuthService>();
            _mediaPickerServiceMock = new Mock<IMediaPickerService>();
            _mediaManagerServiceMock = new Mock<IMediaManagerService>();

            _viewModel = new PacienteViewModel(
                _pacienteServiceMock.Object,
                _navigationServiceMock.Object,
                _dialogServiceMock.Object,
                _authServiceMock.Object,
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
            Assert.Empty(_viewModel.Nome);
            Assert.Empty(_viewModel.Telefone);
            Assert.Empty(_viewModel.Email);
            Assert.False(_viewModel.EhEdicao);
            Assert.Null(_viewModel.FotoPerfilPath);
        }

        [Fact]
        public async Task InicializarAsync_ModoEdicaoPacienteEncontrado_DevePopularCampos()
        {
            // Arrange
            int pacienteId = 1;
            var paciente = new Paciente
            {
                Id = pacienteId,
                Nome = "Ronaldo Moreira",
                Telefone = "11999999999",
                Email = "ronaldo@email.com",
                DataNascimento = new DateTime(1995, 5, 10),
                FotoPerfilPath = "path/foto.png"
            };

            _pacienteServiceMock.Setup(s => s.BuscarPacientePorIdAsync(pacienteId)).ReturnsAsync(paciente);

            // Act
            await _viewModel.InicializarAsync(pacienteId);

            // Assert
            Assert.Equal("Ronaldo Moreira", _viewModel.Nome);
            Assert.Equal("11999999999", _viewModel.Telefone);
            Assert.Equal("ronaldo@email.com", _viewModel.Email);
            Assert.Equal(new DateTime(1995, 5, 10), _viewModel.DataNascimento);
            Assert.Equal("path/foto.png", _viewModel.FotoPerfilPath);
            Assert.True(_viewModel.EhEdicao);
        }

        [Fact]
        public async Task InicializarAsync_PacienteRetornaNulo_DeveLimparCampos()
        {
            // Arrange
            _pacienteServiceMock.Setup(s => s.BuscarPacientePorIdAsync(1)).ReturnsAsync((Paciente)null!);

            // Act
            await _viewModel.InicializarAsync(1);

            // Assert
            Assert.Empty(_viewModel.Nome);
            Assert.False(_viewModel.EhEdicao);
        }

        [Fact]
        public async Task InicializarAsync_ServiceDisparaExcecao_DeveLimparCamposSilenciosamente()
        {
            // Arrange
            _pacienteServiceMock.Setup(s => s.BuscarPacientePorIdAsync(1)).ThrowsAsync(new Exception("Falha no Banco"));

            // Act
            await _viewModel.InicializarAsync(1);

            // Assert
            Assert.Empty(_viewModel.Nome);
            Assert.False(_viewModel.EhEdicao);
        }

        #endregion

        #region Testes: SalvarAsync (Validações)

        [Fact]
        public async Task SalvarAsync_NomeVazio_DeveExibirAlertaEInterromper()
        {
            // Arrange
            _viewModel.Nome = "   ";

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Erro", "O nome é obrigatório.", "OK"), Times.Once);
            _pacienteServiceMock.Verify(s => s.SalvarOuAtualizarPacienteAsync(It.IsAny<Paciente>()), Times.Never);
        }

        [Theory]
        [InlineData("emailinvalido")]
        [InlineData("email@")]
        [InlineData("email@dominio")]
        [InlineData("")]
        public async Task SalvarAsync_EmailComFormatoInvalido_DeveExibirAlertaEInterromper(string emailInvalido)
        {
            // Arrange
            _viewModel.Nome = "Ronaldo";
            _viewModel.Email = emailInvalido;

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("E-mail Inválido", "Por favor, insira um e-mail válido.", "OK"), Times.Once);
            _pacienteServiceMock.Verify(s => s.SalvarOuAtualizarPacienteAsync(It.IsAny<Paciente>()), Times.Never);
        }

        [Fact]
        public async Task SalvarAsync_ModoCadastroEmailDuplicado_DeveExibirAlertaEInterromper()
        {
            // Arrange
            _viewModel.Nome = "Ronaldo";
            _viewModel.Email = "ronaldo@email.com";
            _viewModel.EhEdicao = false;

            _authServiceMock.Setup(a => a.ValidarEmail(_viewModel.Email)).ReturnsAsync(true); // Email já existe

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Erro", "Este e-mail já está cadastrado.", "OK"), Times.Once);
            _pacienteServiceMock.Verify(s => s.SalvarOuAtualizarPacienteAsync(It.IsAny<Paciente>()), Times.Never);
        }

        #endregion

        #region Testes: SalvarAsync (Persistência e Formatação)

        [Fact]
        public async Task SalvarAsync_ModoCadastroSucesso_DeveSanitizarTelefoneESalvar()
        {
            // Arrange
            _viewModel.Nome = "Ronaldo Moreira";
            _viewModel.Email = "ronaldo@email.com";
            _viewModel.Telefone = "(11) 99999-9999";
            _viewModel.EhEdicao = false;

            _authServiceMock.Setup(a => a.ValidarEmail(_viewModel.Email)).ReturnsAsync(false);
            _pacienteServiceMock.Setup(s => s.SalvarOuAtualizarPacienteAsync(It.IsAny<Paciente>())).ReturnsAsync(true);

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _pacienteServiceMock.Verify(s => s.SalvarOuAtualizarPacienteAsync(It.Is<Paciente>(p =>
                p.Nome == "Ronaldo Moreira" &&
                p.Telefone == "11999999999" // Garante a remoção de caracteres especiais pelo Regex
            )), Times.Once);

            _dialogServiceMock.Verify(d => d.DisplayAlert("Sucesso", "Paciente cadastrado com sucesso!", "OK"), Times.Once);
            _navigationServiceMock.Verify(n => n.GoToAsync(".."), Times.Once);
        }

        [Fact]
        public async Task SalvarAsync_ModoEdicaoSucesso_DeveAtualizarPacienteExistente()
        {
            // Arrange
            int pacienteId = 44;
            var pacienteOriginal = new Paciente { Id = pacienteId, Nome = "Nome Antigo", Telefone = "1111" };

            _pacienteServiceMock
                .Setup(s => s.BuscarPacientePorIdAsync(pacienteId))
                .ReturnsAsync(() => pacienteOriginal);

            _authServiceMock.Setup(a => a.ValidarEmail(It.IsAny<string>())).ReturnsAsync(false);
            _pacienteServiceMock.Setup(s => s.SalvarOuAtualizarPacienteAsync(pacienteOriginal)).ReturnsAsync(true);

            await _viewModel.InicializarAsync(pacienteId);

            // Modificações feitas na UI (preenche as propriedades da ViewModel)
            _viewModel.Nome = "Nome Editado";
            _viewModel.Telefone = "11-2222-2222";
            _viewModel.Email = "paciente@email.com"; // Adicionado para evitar falha na validação de formato de e-mail

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Nome Editado", pacienteOriginal.Nome);
            Assert.Equal("1122222222", pacienteOriginal.Telefone); // Sanitizado pelo Regex
            _pacienteServiceMock.Verify(s => s.SalvarOuAtualizarPacienteAsync(pacienteOriginal), Times.Once);
            _dialogServiceMock.Verify(d => d.DisplayAlert("Sucesso", "Paciente updated com sucesso!", "OK"), Times.Never); // Apenas um check de segurança do mock anterior
            _dialogServiceMock.Verify(d => d.DisplayAlert("Sucesso", "Paciente atualizado com sucesso!", "OK"), Times.Once);
            _navigationServiceMock.Verify(n => n.GoToAsync(".."), Times.Once);
        }

        [Fact]
        public async Task SalvarAsync_ModoEdicaoPacienteDesapareceuDoBanco_DeveExibirErro()
        {
            // Arrange
            _viewModel.Nome = "Ronaldo";
            _viewModel.Email = "ronaldo@email.com";
            _viewModel.EhEdicao = true;

            // Retorna nulo no momento de rebuscar o registro para atualizar
            _pacienteServiceMock.Setup(s => s.BuscarPacientePorIdAsync(It.IsAny<int>())).ReturnsAsync((Paciente)null!);

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Erro", "Paciente não encontrado para atualização.", "OK"), Times.Once);
            _pacienteServiceMock.Verify(s => s.SalvarOuAtualizarPacienteAsync(It.IsAny<Paciente>()), Times.Never);
        }

        #endregion

        #region Testes: AlterarFotoAsync

        [Fact]
        public async Task AlterarFotoAsync_AcaoCancelar_NaoDeveChamarMediaPicker()
        {
            // Arrange
            _dialogServiceMock.Setup(d => d.DisplayActionSheet(It.IsAny<string>(), "Cancelar", null, It.IsAny<string[]>())).ReturnsAsync("Cancelar");

            // Act
            await _viewModel.AlterarFotoCommand.ExecuteAsync(null);

            // Assert
            _mediaPickerServiceMock.Verify(m => m.CapturarFotoAsync(), Times.Never);
            _mediaPickerServiceMock.Verify(m => m.SelecionarFotoGaleriaAsync(), Times.Never);
        }

        [Fact]
        public async Task AlterarFotoAsync_ModoCadastroTirarFoto_DeveAtribuirCaminhoTemporario()
        {
            // Arrange
            await _viewModel.InicializarAsync(0); // Novo registro (_pacienteIdAtual = 0)
            _dialogServiceMock.Setup(d => d.DisplayActionSheet(It.IsAny<string>(), "Cancelar", null, It.IsAny<string[]>())).ReturnsAsync("Tirar Foto");
            _mediaPickerServiceMock.Setup(m => m.CapturarFotoAsync()).ReturnsAsync("temp/photo.jpg");

            // Act
            await _viewModel.AlterarFotoCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("temp/photo.jpg", _viewModel.FotoPerfilPath);
            _mediaManagerServiceMock.Verify(m => m.RegistrarFotoPacienteAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AlterarFotoAsync_ModoEdicaoEscolherGaleria_DeveSalvarNoDiretorioDefinitivo()
        {
            // Arrange
            int pacienteId = 88;
            var paciente = new Paciente { Id = pacienteId };
            _pacienteServiceMock.Setup(s => s.BuscarPacientePorIdAsync(pacienteId)).ReturnsAsync(paciente);
            await _viewModel.InicializarAsync(pacienteId); // Edição

            _dialogServiceMock.Setup(d => d.DisplayActionSheet(It.IsAny<string>(), "Cancelar", null, It.IsAny<string[]>())).ReturnsAsync("Escolher da Galeria");
            _mediaPickerServiceMock.Setup(m => m.SelecionarFotoGaleriaAsync()).ReturnsAsync("temp/gallery.jpg");
            _mediaManagerServiceMock.Setup(m => m.RegistrarFotoPacienteAsync(pacienteId, "temp/gallery.jpg")).ReturnsAsync("definitive/photo_88.jpg");

            // Act
            await _viewModel.AlterarFotoCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("definitive/photo_88.jpg", _viewModel.FotoPerfilPath);
            _mediaManagerServiceMock.Verify(m => m.RegistrarFotoPacienteAsync(pacienteId, "temp/gallery.jpg"), Times.Once);
        }

        #endregion
    }
}