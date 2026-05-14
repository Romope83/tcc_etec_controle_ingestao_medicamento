using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.ViewModels;
using Moq;
using Xunit;

namespace IngestaoMed.Tests.ViewModels
{
    public class TratamentoRegrasTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly Mock<IDialogService> _dialogMock;
        private readonly Mock<INavigationService> _navigationMock;
        private readonly Mock<IAgendamentoConflitoService> _conflitoMock;
        private readonly TratamentoViewModel _viewModel;

        public TratamentoRegrasTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _dialogMock = new Mock<IDialogService>();
            _navigationMock = new Mock<INavigationService>();
            _conflitoMock = new Mock<IAgendamentoConflitoService>();

            _viewModel = new TratamentoViewModel(
                _dbMock.Object,
                _dialogMock.Object,
                _navigationMock.Object,
                _conflitoMock.Object);
        }

        [Fact]
        public async Task Salvar_DeveExibirErro_QuandoPacienteOuMedicamentoNaoSelecionados()
        {
            // Arrange
            _viewModel.Nome = "Tratamento Teste";
            _viewModel.PacienteSelecionado = null; // Falha aqui

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _dialogMock.Verify(d => d.DisplayAlert("Erro", It.Is<string>(s => s.Contains("Selecione")), "OK"), Times.Once);
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<Tratamento>()), Times.Never);
        }

        [Fact]
        public async Task Salvar_DeveExibirErro_QuandoIntervaloForZeroOuNegativo()
        {
            // Arrange
            _viewModel.PacienteSelecionado = new Paciente { Id = 1 };
            _viewModel.MedicamentoSelecionado = new Medicamento { Id = 1 };
            _viewModel.Nome = "Dipirona";
            _viewModel.IntervaloHoras = 0; // Regra de negócio: deve ser > 0

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _dialogMock.Verify(d => d.DisplayAlert("Erro", It.Is<string>(s => s.Contains("intervalo")), "OK"), Times.Once);
        }

        [Fact]
        public async Task Salvar_DeveExibirErro_QuandoDataFimForAnteriorAoInicio()
        {
            // Arrange
            _viewModel.PacienteSelecionado = new Paciente { Id = 1 };
            _viewModel.MedicamentoSelecionado = new Medicamento { Id = 1 };
            _viewModel.Nome = "Antibiótico";
            _viewModel.IntervaloHoras = 8;
            _viewModel.DataInicio = DateTime.Now.AddDays(5);
            _viewModel.DataFim = DateTime.Now; // Data fim antes do início

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _dialogMock.Verify(d => d.DisplayAlert("Erro", It.Is<string>(s => s.Contains("término")), "OK"), Times.Once);
        }

        [Fact]
        public async Task Salvar_DeveInserirTratamentoENavegar_QuandoDadosForemValidos()
        {
            // Arrange
            _viewModel.PacienteSelecionado = new Paciente { Id = 1 };
            _viewModel.MedicamentoSelecionado = new Medicamento { Id = 1 };
            _viewModel.Nome = "Tratamento OK";
            _viewModel.IntervaloHoras = 12;

            _dbMock.Setup(d => d.InserirAsync(It.IsAny<Tratamento>())).ReturnsAsync(true);


            _conflitoMock.Setup(c => c.VerificarConflitosAsync(
                    It.IsAny<Tratamento>(),
                    It.IsAny<List<DateTime>>()))
                    .ReturnsAsync(new List<Agendamento>());

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<Tratamento>()), Times.AtLeastOnce);
            _navigationMock.Verify(n => n.GoToAsync(".."), Times.Once);
        }
    }
}