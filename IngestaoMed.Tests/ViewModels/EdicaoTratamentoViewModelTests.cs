/*using Moq;
using Xunit;
using IngestaoMed.Core.ViewModels;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IngestaoMed.Tests.ViewModels
{
    public class EdicaoTratamentoViewModelTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly Mock<INavigationService> _navigationMock;
        private readonly Mock<IDialogService> _dialogMock;
        private readonly EdicaoTratamentoViewModel _viewModel;

        public EdicaoTratamentoViewModelTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _navigationMock = new Mock<INavigationService>();
            _dialogMock = new Mock<IDialogService>();

            _viewModel = new EdicaoTratamentoViewModel(
                _dbMock.Object,
                _navigationMock.Object,
                _dialogMock.Object);
        }

        // --- MÉTODO: InicializarAsync / CarregarTratamentoAsync / CarregarRemediosVinculadosAsync ---

        [Fact]
        public async Task InicializarAsync_DeveMapearPropriedadesERemedios_QuandoTratamentoIdForValido()
        {
            // Arrange
            int pacienteId = 1;
            int tratamentoId = 10;

            var tratamentoFake = new Tratamento
            {
                Id = tratamentoId,
                Nome = "Tratamento A",
                Descricao = "Descricao A",
                DataInicio = DateTime.Today,
                DataFim = DateTime.Today.AddDays(7),
                Ativo = true,
                PacienteId = pacienteId
            };

            var vinculosFake = new List<MedicamentoTratamento>
            {
                new MedicamentoTratamento { Id = 1, TratamentoId = tratamentoId, MedicamentoId = 100 }
            };

            var medFake = new Medicamento { Id = 100, NomeComercial = "Paracetamol" };

            // Mock das operações do IDatabaseContext
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(tratamentoFake);

            _dbMock.Setup(d => d.BuscarOndeAsync<MedicamentoTratamento>(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(vinculosFake);

            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Medicamento>(It.IsAny<Expression<Func<Medicamento, bool>>>()))
                   .ReturnsAsync(medFake);

            // Act
            await _viewModel.InicializarAsync(pacienteId, tratamentoId);

            // Assert (Verificação de Saída de Estado)
            Assert.Equal(pacienteId, _viewModel.PacienteId);
            Assert.Equal(tratamentoId, _viewModel.TratamentoId);
            Assert.Equal("Tratamento A", _viewModel.Nome);
            Assert.Equal("Descricao A", _viewModel.Descricao);
            Assert.True(_viewModel.Ativado);
            Assert.Single(_viewModel.Remedios);
            Assert.Equal("Paracetamol", _viewModel.Remedios[0].NomeMedicamento);
        }

        [Fact]
        public async Task InicializarAsync_NaoDeveBuscarNoBanco_QuandoTratamentoIdForZero()
        {
            // Act
            await _viewModel.InicializarAsync(1, 0);

            // Assert
            Assert.Equal(1, _viewModel.PacienteId);
            Assert.Equal(0, _viewModel.TratamentoId);
            _dbMock.Verify(d => d.BuscarPrimeiroAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()), Times.Never);
            Assert.Empty(_viewModel.Remedios);
        }

        // --- MÉTODO: AdicionarMedicamentoCommand ---

        [Fact]
        public async Task AdicionarMedicamentoCommand_DeveSalvarAutomaticoENavegar_QuandoTratamentoForNovo()
        {
            // Arrange
            _viewModel.TratamentoId = 0;
            _viewModel.Nome = string.Empty; // Força a condição de Fallback do nome
            _viewModel.PacienteId = 5;

            // Ajustado para ReturnsAsync(true) para satisfazer o tipo Task<bool> esperado pelo compilador
            _dbMock.Setup(d => d.InserirAsync(It.IsAny<Tratamento>()))
                   .Callback<Tratamento>(t => t.Id = 999) 
                   .ReturnsAsync(true);

            // Act
            await _viewModel.AdicionarMedicamentoCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Novo Tratamento", _viewModel.Nome);
            Assert.Equal(999, _viewModel.TratamentoId);
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<Tratamento>()), Times.Once);
            _navigationMock.Verify(n => n.GoToAsync("AgendamentoPage?tratamentoId=999"), Times.Once);
        }

        // --- MÉTODO: SalvarAlteracoesCommand ---

        [Fact]
        public async Task SalvarAlteracoesCommand_DeveInserirENavegarParaSelecao_QuandoTratamentoIdForZero()
        {
            // Arrange
            _viewModel.TratamentoId = 0;
            _viewModel.Nome = "Tratamento Novo";

            _dbMock.Setup(d => d.InserirAsync(It.IsAny<Tratamento>()))
                   .Callback<Tratamento>(t => t.Id = 50)
                   .ReturnsAsync(true);

            // Act
            await _viewModel.SalvarAlteracoesCommand.ExecuteAsync(null);

            // Assert
            _navigationMock.Verify(n => n.GoToAsync("SelecaoMedicamentoPage?tratamentoId=50"), Times.Once);
        }

        [Fact]
        public async Task SalvarAlteracoesCommand_DeveAtualizarENavegarParaTras_QuandoTratamentoIdExistir()
        {
            // Arrange
            _viewModel.TratamentoId = 10;
            _viewModel.Nome = "Nome Alterado";

            var tratamentoExistente = new Tratamento { Id = 10, Nome = "Nome Antigo" };

            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(tratamentoExistente);
            _dbMock.Setup(d => d.AtualizarAsync(It.IsAny<Tratamento>()))
                   .ReturnsAsync(1);

            // Act
            await _viewModel.SalvarAlteracoesCommand.ExecuteAsync(null);

            // Assert
            _dbMock.Verify(d => d.AtualizarAsync(It.Is<Tratamento>(t => t.Nome == "Nome Alterado")), Times.Once);
            _navigationMock.Verify(n => n.GoToAsync(".."), Times.Once);
        }

        // --- MÉTODO: ExcluirTratamentoCommand ---

        [Fact]
        public async Task ExcluirTratamentoCommand_DeveExcluirENavegar_QuandoConfirmadoPeloUsuario()
        {
            // Arrange
            _viewModel.TratamentoId = 10;
            var tratamentoExistente = new Tratamento { Id = 10 };

            _dialogMock.Setup(d => d.DisplayConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), "Sim", "Não"))
                       .ReturnsAsync(true);

            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(tratamentoExistente);
            _dbMock.Setup(d => d.ExcluirAsync(It.IsAny<Tratamento>()))
                   .ReturnsAsync(1);

            // Act
            await _viewModel.ExcluirTratamentoCommand.ExecuteAsync(null);

            // Assert
            _dbMock.Verify(d => d.ExcluirAsync(tratamentoExistente), Times.Once);
            _navigationMock.Verify(n => n.GoToAsync(".."), Times.Once);
        }

        [Fact]
        public async Task ExcluirTratamentoCommand_NaoDeveExcluir_QuandoUsuarioRecusarConfirmacao()
        {
            // Arrange
            _viewModel.TratamentoId = 10;

            _dialogMock.Setup(d => d.DisplayConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), "Sim", "Não"))
                       .ReturnsAsync(false);

            // Act
            await _viewModel.ExcluirTratamentoCommand.ExecuteAsync(null);

            // Assert
            _dbMock.Verify(d => d.ExcluirAsync(It.IsAny<Tratamento>()), Times.Never);
            _navigationMock.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
        }

        // --- MÉTODO: RemoverMedicamentoDoTratamentoCommand ---

        [Fact]
        public async Task RemoverMedicamentoDoTratamentoCommand_DeveRemoverDaColecaoEDeletarDoBanco()
        {
            // Arrange
            var vinculo = new MedicamentoTratamento { Id = 5, TratamentoId = 10, MedicamentoId = 100 };
            _viewModel.Remedios.Add(vinculo);

            _dbMock.Setup(d => d.ExcluirAsync(It.IsAny<MedicamentoTratamento>()))
                   .ReturnsAsync(1);

            // Act
            await _viewModel.RemoverMedicamentoDoTratamentoCommand.ExecuteAsync(vinculo);

            // Assert
            Assert.Empty(_viewModel.Remedios);
            _dbMock.Verify(d => d.ExcluirAsync(vinculo), Times.Once);
        }

        [Fact]
        public async Task RemoverMedicamentoDoTratamentoCommand_NaoDeveExecutar_QuandoParametroForNulo()
        {
            // Act
            await _viewModel.RemoverMedicamentoDoTratamentoCommand.ExecuteAsync(null);

            // Assert
            _dbMock.Verify(d => d.ExcluirAsync(It.IsAny<MedicamentoTratamento>()), Times.Never);
        }

        // --- MÉTODO: EditarTratamentoCommand ---

        [Fact]
        public async Task EditarTratamentoCommand_DeveNavegarParaPaginaDeEdicaoComId()
        {
            // Act
            await _viewModel.EditarTratamentoCommand.ExecuteAsync(25);

            // Assert
            _navigationMock.Verify(n => n.GoToAsync("EdicaoTratamentoPage?tratamentoId=25"), Times.Once);
        }
    }
}*/