using Moq;
using Xunit;
using IngestaoMed.Core.ViewModels;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace IngestaoMed.Tests.ViewModels
{
    public class TratamentoViewModelTests
    {
        private readonly Mock<ITratamentoService> _tratamentoServiceMock;
        private readonly Mock<IDialogService> _dialogMock;
        private readonly Mock<INavigationService> _navigationMock;
        private readonly TratamentoViewModel _viewModel;

        public TratamentoViewModelTests()
        {
            _tratamentoServiceMock = new Mock<ITratamentoService>();
            _dialogMock = new Mock<IDialogService>();
            _navigationMock = new Mock<INavigationService>();

            _viewModel = new TratamentoViewModel(
                _tratamentoServiceMock.Object,
                _dialogMock.Object,
                _navigationMock.Object);
        }

        #region InicializarAsync

        [Fact]
        public async Task InicializarAsync_ComIdZero_DeveLimparPropriedades()
        {
            // Arrange
            _viewModel.Nome = "Tratamento Limpeza";
            _viewModel.MedicamentosVinculados.Add(new MedicamentoTratamento());

            // Act
            await _viewModel.InicializarAsync(0);

            // Assert
            Assert.Equal(0, _viewModel.TratamentoId);
            Assert.Empty(_viewModel.Nome);
            Assert.Empty(_viewModel.Descricao);
            Assert.False(_viewModel.Ativado);
            Assert.Empty(_viewModel.MedicamentosVinculados);
        }

        [Fact]
        public async Task InicializarAsync_ComIdValidoEncontrado_DevePopularCamposERemedios()
        {
            // Arrange
            int id = 10;
            var tratamento = new Tratamento { Id = id, Nome = "Tratamento A", Descricao = "Descricao A", PacienteId = 2, Ativo = true };
            var remedios = new List<MedicamentoTratamento> { new MedicamentoTratamento { Id = 1, MedicamentoId = 5 } };

            _tratamentoServiceMock.Setup(s => s.ObterPorIdAsync(id)).ReturnsAsync(tratamento);
            _tratamentoServiceMock.Setup(s => s.ObterMedicamentosVinculadosAsync(id)).ReturnsAsync(remedios);

            // Act
            await _viewModel.InicializarAsync(id);

            // Assert
            Assert.Equal(id, _viewModel.TratamentoId);
            Assert.Equal("Tratamento A", _viewModel.Nome);
            Assert.Equal("Descricao A", _viewModel.Descricao);
            Assert.True(_viewModel.Ativado);
            Assert.Equal(2, _viewModel.PacienteId);
            Assert.Single(_viewModel.MedicamentosVinculados);
        }

        [Fact]
        public async Task InicializarAsync_ComIdValidoMasInexistente_NaoDevePreencherPropriedades()
        {
            // Arrange
            int idInexistente = 999;
            _tratamentoServiceMock.Setup(s => s.ObterPorIdAsync(idInexistente)).ReturnsAsync((Tratamento)null!);

            // Act
            await _viewModel.InicializarAsync(idInexistente);

            // Assert
            _tratamentoServiceMock.Verify(s => s.ObterPorIdAsync(idInexistente), Times.Once);
            _tratamentoServiceMock.Verify(s => s.ObterMedicamentosVinculadosAsync(It.IsAny<int>()), Times.Never);
            Assert.Empty(_viewModel.Nome);
        }


        [Fact]
        public async Task InicializarAsync_ComPacienteETratamentoId_DevePopularTratamento()
        {
            // Arrange
            int pacienteId = 3;
            int tratamentoId = 15;
            var tratamento = new Tratamento { Id = tratamentoId, Nome = "Tratamento B", PacienteId = pacienteId };

            _tratamentoServiceMock.Setup(s => s.ObterPorIdAsync(tratamentoId)).ReturnsAsync(tratamento);
            _tratamentoServiceMock.Setup(s => s.ObterMedicamentosVinculadosAsync(tratamentoId)).ReturnsAsync(new List<MedicamentoTratamento>());

            // Act
            await _viewModel.InicializarAsync(pacienteId, tratamentoId);

            // Assert
            Assert.Equal(pacienteId, _viewModel.PacienteId);
            Assert.Equal(tratamentoId, _viewModel.TratamentoId);
            Assert.Equal("Tratamento B", _viewModel.Nome);
        }
        #endregion

        #region SalvarAsync

        [Fact]
        public async Task SalvarAsync_NomeVazio_DeveExibirAlertaEInterromper()
        {
            // Arrange
            _viewModel.Nome = "   ";

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _dialogMock.Verify(d => d.DisplayAlert("Aviso", "O nome do tratamento é obrigatório.", "OK"), Times.Once);
            _tratamentoServiceMock.Verify(s => s.InserirTratamentoAsync(It.IsAny<Tratamento>()), Times.Never);
        }

        [Fact]
        public async Task SalvarAsync_NovoTratamentoSucesso_DeveInserirEDefinirEstado()
        {
            // Arrange
            _viewModel.TratamentoId = 0;
            _viewModel.Nome = "Novo Tratamento";
            _viewModel.Descricao = "Nova Descricao";
            _viewModel.PacienteId = 1;

            _tratamentoServiceMock.Setup(s => s.InserirTratamentoAsync(It.IsAny<Tratamento>())).ReturnsAsync(99);

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _tratamentoServiceMock.Verify(s => s.InserirTratamentoAsync(It.Is<Tratamento>(t =>
                t.Nome == "Novo Tratamento" &&
                t.Descricao == "Nova Descricao" &&
                t.PacienteId == 1
            )), Times.Once);
            Assert.Equal(99, _viewModel.TratamentoId);
            Assert.False(_viewModel.Ativado);
            _dialogMock.Verify(d => d.DisplayAlert("Sucesso", "Tratamento criado com sucesso!", "OK"), Times.Once);
        }

        [Fact]
        public async Task SalvarAsync_EdicaoTratamentoExistente_DeveAtualizarDados()
        {
            // Arrange
            int id = 5;
            var tratamentoOriginal = new Tratamento { Id = id, Nome = "Antigo", Descricao = "Antiga" };

            _tratamentoServiceMock.Setup(s => s.ObterPorIdAsync(id)).ReturnsAsync(() => tratamentoOriginal);
            _tratamentoServiceMock.Setup(s => s.ObterMedicamentosVinculadosAsync(id)).ReturnsAsync(new List<MedicamentoTratamento>());

            await _viewModel.InicializarAsync(id);
            _viewModel.Nome = "Atualizado";
            _viewModel.Descricao = "Atualizada";
            _viewModel.Ativado = true;

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Atualizado", tratamentoOriginal.Nome);
            Assert.Equal("Atualizada", tratamentoOriginal.Descricao);
            Assert.True(tratamentoOriginal.Ativo);
            _tratamentoServiceMock.Verify(s => s.AtualizarTratamentoAsync(tratamentoOriginal), Times.Once);
            _dialogMock.Verify(d => d.DisplayAlert("Sucesso", "Tratamento updated com sucesso!", "OK"), Times.Once);
        }

        [Fact]
        public async Task SalvarAsync_EdicaoTratamentoExistenteMasDeletado_NaoDeveChamarAtualizar()
        {
            // Arrange
            int id = 5;
            var tratamentoExistente = new Tratamento { Id = id, Nome = "Nome Antigo" };

            // Setup para inicializar com sucesso e popular o campo privado _tratamentoAtual
            _tratamentoServiceMock.Setup(s => s.ObterPorIdAsync(id)).ReturnsAsync(tratamentoExistente);
            _tratamentoServiceMock.Setup(s => s.ObterMedicamentosVinculadosAsync(id)).ReturnsAsync(new List<MedicamentoTratamento>());

            await _viewModel.InicializarAsync(id);

            // Modifica o comportamento do mock: agora ele retorna null indicando que sumiu do banco na hora do save
            _tratamentoServiceMock.Setup(s => s.ObterPorIdAsync(id)).ReturnsAsync((Tratamento)null!);

            _viewModel.Nome = "Tratamento Editado";

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _tratamentoServiceMock.Verify(s => s.AtualizarTratamentoAsync(It.IsAny<Tratamento>()), Times.Never);
            _dialogMock.Verify(d => d.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region AdicionarMedicamentoAsync

        [Fact]
        public async Task AdicionarMedicamentoAsync_IdZero_DeveSalvarAutomaticoENavegar()
        {
            // Arrange
            _viewModel.TratamentoId = 0;
            _viewModel.Nome = "Salvar Automatico";
            _viewModel.PacienteId = 7;

            _tratamentoServiceMock.Setup(s => s.InserirTratamentoAsync(It.IsAny<Tratamento>())).ReturnsAsync(77);

            // Act
            await _viewModel.AdicionarMedicamentoCommand.ExecuteAsync(null);

            // Assert
            _tratamentoServiceMock.Verify(s => s.InserirTratamentoAsync(It.Is<Tratamento>(t =>
                t.Nome == "Salvar Automatico" && t.PacienteId == 7
            )), Times.Once);
            Assert.Equal(77, _viewModel.TratamentoId);
            _navigationMock.Verify(n => n.GoToAsync("AgendamentoPage?tratamentoId=77"), Times.Once);
        }

        [Fact]
        public async Task AdicionarMedicamentoAsync_IdExistente_DeveApenasNavegar()
        {
            // Arrange
            _viewModel.TratamentoId = 55;

            // Act
            await _viewModel.AdicionarMedicamentoCommand.ExecuteAsync(null);

            // Assert
            _tratamentoServiceMock.Verify(s => s.InserirTratamentoAsync(It.IsAny<Tratamento>()), Times.Never);
            _navigationMock.Verify(n => n.GoToAsync("AgendamentoPage?tratamentoId=55"), Times.Once);
        }

        #endregion

        #region RemoverMedicamentoAsync

        [Fact]
        public async Task RemoverMedicamentoAsync_MedicamentoNulo_DeveRetornarSemAcao()
        {
            // Act
            await _viewModel.RemoverMedicamentoCommand.ExecuteAsync(null);

            // Assert
            _dialogMock.Verify(d => d.DisplayConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RemoverMedicamentoAsync_RecusadoPeloUsuario_NaoDeveDeletar()
        {
            // Arrange
            var mt = new MedicamentoTratamento { Id = 1 };
            _viewModel.MedicamentosVinculados.Add(mt);
            _dialogMock.Setup(d => d.DisplayConfirmationAsync("Confirmar", It.IsAny<string>(), "Sim", "Não")).ReturnsAsync(false);

            // Act
            await _viewModel.RemoverMedicamentoCommand.ExecuteAsync(mt);

            // Assert
            Assert.Single(_viewModel.MedicamentosVinculados);
            _tratamentoServiceMock.Verify(s => s.ExcluirMedicamentoTratamentoAsync(It.IsAny<MedicamentoTratamento>()), Times.Never);
        }

        [Fact]
        public async Task RemoverMedicamentoAsync_ConfirmadoPeloUsuario_DeveRemoverEDeletar()
        {
            // Arrange
            var mt = new MedicamentoTratamento { Id = 8 };
            _viewModel.MedicamentosVinculados.Add(mt);
            _dialogMock.Setup(d => d.DisplayConfirmationAsync("Confirmar", It.IsAny<string>(), "Sim", "Não")).ReturnsAsync(true);

            // Act
            await _viewModel.RemoverMedicamentoCommand.ExecuteAsync(mt);

            // Assert
            Assert.Empty(_viewModel.MedicamentosVinculados);
            _tratamentoServiceMock.Verify(s => s.ExcluirMedicamentoTratamentoAsync(mt), Times.Once);
        }

        [Fact]
        public async Task RemoverMedicamentoAsync_MedicamentoNovoSemId_DeveRemoverApenasDaTela()
        {
            // Arrange
            var mtSemId = new MedicamentoTratamento { Id = 0, MedicamentoId = 3 };
            _viewModel.MedicamentosVinculados.Add(mtSemId);
            _dialogMock.Setup(d => d.DisplayConfirmationAsync("Confirmar", It.IsAny<string>(), "Sim", "Não")).ReturnsAsync(true);

            // Act
            await _viewModel.RemoverMedicamentoCommand.ExecuteAsync(mtSemId);

            // Assert
            Assert.Empty(_viewModel.MedicamentosVinculados);
            _tratamentoServiceMock.Verify(s => s.ExcluirMedicamentoTratamentoAsync(It.IsAny<MedicamentoTratamento>()), Times.Never);
        }

        #endregion

        #region ExcluirTratamentoAsync

        [Fact]
        public async Task ExcluirTratamentoAsync_IdZero_DeveRetornarSemAcao()
        {
            // Arrange
            _viewModel.TratamentoId = 0;

            // Act
            await _viewModel.ExcluirTratamentoCommand.ExecuteAsync(null);

            // Assert
            _dialogMock.Verify(d => d.DisplayConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExcluirTratamentoAsync_ConfirmadoExistente_DeveDeletarENavegar()
        {
            // Arrange
            int id = 22;
            var t = new Tratamento { Id = id };
            _viewModel.TratamentoId = id;
            _dialogMock.Setup(d => d.DisplayConfirmationAsync("Excluir Tratamento", It.IsAny<string>(), "Sim", "Não")).ReturnsAsync(true);
            _tratamentoServiceMock.Setup(s => s.ObterPorIdAsync(id)).ReturnsAsync(t);

            // Act
            await _viewModel.ExcluirTratamentoCommand.ExecuteAsync(null);

            // Assert
            _tratamentoServiceMock.Verify(s => s.ExcluirTratamentoAsync(t), Times.Once);
            _navigationMock.Verify(n => n.GoToAsync(".."), Times.Once);
        }

        #endregion

        #region SelecionarMedicamentoVinculadoAsync

        [Fact]
        public async Task SelecionarMedicamentoVinculadoAsync_MedicamentoNulo_DeveRetornarSemNavegar()
        {
            // Act
            await _viewModel.SelecionarMedicamentoVinculadoCommand.ExecuteAsync(null);

            // Assert
            _navigationMock.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SelecionarMedicamentoVinculadoAsync_MedicamentoValido_DeveNavegarParaAgendamento()
        {
            // Arrange
            var mt = new MedicamentoTratamento { Id = 45 };

            // Act
            await _viewModel.SelecionarMedicamentoVinculadoCommand.ExecuteAsync(mt);

            // Assert
            _navigationMock.Verify(n => n.GoToAsync("AgendamentoPage?medicamentoTratamentoId=45"), Times.Once);
        }

        #endregion

    }
}