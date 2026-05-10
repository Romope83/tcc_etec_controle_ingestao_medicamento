using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.ViewModels;
using Moq;
using Xunit;

namespace IngestaoMed.Tests.ViewModels
{
    public class TratamentoViewModelTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly Mock<IDialogService> _dialogMock;
        private readonly Mock<INavigationService> _navMock;
        private readonly Mock<IAgendamentoConflitoService> _conflitoMock;
        private readonly TratamentoViewModel _viewModel;

        public TratamentoViewModelTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _dialogMock = new Mock<IDialogService>();
            _navMock = new Mock<INavigationService>();
            _conflitoMock = new Mock<IAgendamentoConflitoService>();

            _viewModel = new TratamentoViewModel(
                _dbMock.Object,
                _dialogMock.Object,
                _navMock.Object,
                _conflitoMock.Object);
        }

        [Fact]
        public async Task Salvar_DeveGerarAgendamentosCorretos_QuandoDadosSaoValidos()
        {
            // Arrange
            _viewModel.PacienteSelecionado = new Paciente { Id = 1 };
            _viewModel.MedicamentoSelecionado = new Medicamento { Id = 1 };
            _viewModel.Nome = "Tratamento de 24h";
            _viewModel.IntervaloHoras = 8; // De 8 em 8 horas
            _viewModel.DataInicio = new DateTime(2026, 05, 01, 08, 0, 0);
            _viewModel.DataFim = new DateTime(2026, 05, 02, 00, 0, 0); // Termina às 00h do dia seguinte

            _dbMock.Setup(d => d.InserirAsync(It.IsAny<Tratamento>())).ReturnsAsync(true);

            _conflitoMock.Setup(c => c.VerificarConflitosAsync(
            It.IsAny<Tratamento>(),
            It.IsAny<List<DateTime>>()))
            .ReturnsAsync(new List<Agendamento>());

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            //Deve gerar 3 agendamentos: 08:00, 16:00 e 00:00
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<Agendamento>()), Times.Exactly(3));

            // Verifica se o primeiro agendamento tem o horário de início correto e status pendente
            _dbMock.Verify(d => d.InserirAsync(It.Is<Agendamento>(a =>
                a.HorarioProgramado == _viewModel.DataInicio &&
                a.Status == "Pendente")), Times.Once);
        }

        [Fact]
        public async Task Salvar_DeveExibirErro_QuandoIntervaloForZeroOuNegativo()
        {
            // Arrange
            _viewModel.PacienteSelecionado = new Paciente { Id = 1 };
            _viewModel.MedicamentoSelecionado = new Medicamento { Id = 1 };
            _viewModel.Nome = "Teste Erro Intervalo";
            _viewModel.IntervaloHoras = 0; // Intervalo inválido

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            _dialogMock.Verify(d => d.DisplayAlert("Erro", "O intervalo entre as doses deve ser maior que zero.", "OK"), Times.Once);
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<Tratamento>()), Times.Never);
        }

        [Fact]
        public async Task Salvar_DeveLimitarAgenda_QuandoTratamentoForContinuo()
        {
            // Arrange
            _viewModel.PacienteSelecionado = new Paciente { Id = 1 };
            _viewModel.MedicamentoSelecionado = new Medicamento { Id = 1 };
            _viewModel.Nome = "Tratamento Contínuo";
            _viewModel.IntervaloHoras = 24; // 1 vez ao dia
            _viewModel.DataInicio = DateTime.Now;
            _viewModel.DataFim = null; // Sem data de término

            _dbMock.Setup(d => d.InserirAsync(It.IsAny<Tratamento>())).ReturnsAsync(true);

            _conflitoMock.Setup(c => c.VerificarConflitosAsync(
            It.IsAny<Tratamento>(),
            It.IsAny<List<DateTime>>()))
            .ReturnsAsync(new List<Agendamento>());

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            // Como definimos o limite de 30 dias na ViewModel, deve gerar aproximadamente 30/31 registros
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<Agendamento>()), Times.AtLeast(30));
        }

        [Fact]
        public async Task Salvar_NaoDeveGerarAgenda_SeFalharAoInserirTratamento()
        {
            // Arrange
            _viewModel.PacienteSelecionado = new Paciente { Id = 1 };
            _viewModel.MedicamentoSelecionado = new Medicamento { Id = 1 };
            _viewModel.Nome = "Falha no Banco";
            _viewModel.IntervaloHoras = 6;

            _conflitoMock.Setup(c => c.VerificarConflitosAsync(
            It.IsAny<Tratamento>(),
            It.IsAny<List<DateTime>>()))
            .ReturnsAsync(new List<Agendamento>());

            _dbMock.Setup(d => d.InserirAsync(It.IsAny<Tratamento>())).ReturnsAsync(false);

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            // Se o tratamento não foi salvo, a agenda nunca deve ser gerada
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<Agendamento>()), Times.Never);
            _dialogMock.Verify(d => d.DisplayAlert("Erro", "Não foi possível salvar o tratamento.", "OK"), Times.Once);
        }
    }
}