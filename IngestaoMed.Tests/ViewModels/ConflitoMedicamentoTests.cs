using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.ViewModels;
using Moq;
using Xunit;

namespace IngestaoMed.Tests.ViewModels
{
    public class ConflitoMedicamentoTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly Mock<IDialogService> _dialogMock;
        private readonly Mock<INavigationService> _navigationMock;
        private readonly Mock<IAgendamentoConflitoService> _conflitoMock;
        private readonly TratamentoViewModel _viewModel;

        public ConflitoMedicamentoTests()
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
        public async Task Salvar_DeveAlertarConflito_E_InterromperSeUsuarioNegar()
        {
            // ... Arrange anterior
            var conflitosFake = new List<Agendamento> { new Agendamento { Id = 99 } };
            _conflitoMock.Setup(c => c.VerificarConflitosAsync(It.IsAny<Tratamento>(), It.IsAny<List<DateTime>>()))
                         .ReturnsAsync(conflitosFake);

            // Simula o usuário clicando em "Não" (false)
            _dialogMock.Setup(d => d.DisplayAlert("Atenção", It.IsAny<string>(), "Sim", "Não"))
                       .ReturnsAsync(false);

            // Act
            await _viewModel.SalvarCommand.ExecuteAsync(null);

            // Assert
            // Verifica que o banco NUNCA foi chamado porque o usuário negou
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<Tratamento>()), Times.Never);
        }
    }
}