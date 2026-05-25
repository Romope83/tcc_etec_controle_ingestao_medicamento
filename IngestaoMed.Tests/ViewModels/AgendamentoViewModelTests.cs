using Moq;
using Xunit;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IngestaoMed.Tests.ViewModels
{
    public class AgendamentoViewModelTests
    {
        private readonly Mock<INavigationService> _navigationMock;
        private readonly Mock<IDialogService> _dialogServiceMock;
        private readonly Mock<IAgendamentoConflitoService> _conflitoServiceMock;
        private readonly Mock<IAgendamentoService> _agendamentoServiceMock;
        private readonly AgendamentoViewModel _viewModel;

        public AgendamentoViewModelTests()
        {
            _navigationMock = new Mock<INavigationService>();
            _dialogServiceMock = new Mock<IDialogService>();
            _conflitoServiceMock = new Mock<IAgendamentoConflitoService>();
            _agendamentoServiceMock = new Mock<IAgendamentoService>();

            _viewModel = new AgendamentoViewModel(
                _navigationMock.Object,
                _dialogServiceMock.Object,
                _conflitoServiceMock.Object,
                _agendamentoServiceMock.Object
            );
        }

        #region Método: OnToleranciaChanged

        [Fact]
        public void OnToleranciaChanged_QuandoValorNaoMultiploDeDez_DeveArredondarParaDezMaisProximo()
        {
            // Act
            _viewModel.Tolerancia = 24;

            // Assert
            Assert.Equal(20, _viewModel.Tolerancia);
        }

        [Fact]
        public void OnToleranciaChanged_QuandoValorJaForMultiploDeDez_NaoDeveAlterarValor()
        {
            // Act
            _viewModel.Tolerancia = 30;

            // Assert
            Assert.Equal(30, _viewModel.Tolerancia);
        }

        #endregion

        #region Método: InicializarAsync

        [Fact]
        public async Task InicializarAsync_QuandoNovoAgendamento_DeveResetarPropriedadesECarregarMedicamentos()
        {
            // Arrange
            var listaMedicamentos = new List<Medicamento> { new Medicamento { Id = 1, NomeComercial = "Dipirona" } };
            _agendamentoServiceMock.Setup(s => s.ObterTodosMedicamentosAsync()).ReturnsAsync(listaMedicamentos);

            // Act
            await _viewModel.InicializarAsync(tratamentoId: 10, medicamentoTratamentoId: 0);

            // Assert
            Assert.Equal(10, _viewModel.TratamentoId);
            Assert.Equal(0, _viewModel.MedicamentoTratamentoId);
            Assert.Null(_viewModel.MedicamentoSelecionado);
            Assert.False(_viewModel.TemMedicamentoSelecionado);
            Assert.Empty(_viewModel.DosesGeradas);
            Assert.Single(_viewModel.SugestoesBusca);
            _agendamentoServiceMock.Verify(s => s.ObterTodosMedicamentosAsync(), Times.Once);
        }

        [Fact]
        public async Task InicializarAsync_QuandoEdicaoComDadosValidos_DevePreencherCamposECarregarDoses()
        {
            // Arrange
            var vinculoFake = new MedicamentoTratamento { MedicamentoId = 5, Dosagem = "1 comprimido", IntervaloHoras = 12, Ativo = true };
            var medFake = new Medicamento { Id = 5, NomeComercial = "Aspirina" };
            var dosesFake = new List<Agendamento>
            {
                new Agendamento { HorarioOriginal = DateTime.Today.AddHours(8) },
                new Agendamento { HorarioOriginal = DateTime.Today.AddHours(20) }
            };

            _agendamentoServiceMock.Setup(s => s.ObterVinculoPorIdAsync(1)).ReturnsAsync(vinculoFake);
            _agendamentoServiceMock.Setup(s => s.ObterMedicamentoPorIdAsync(5)).ReturnsAsync(medFake);
            _agendamentoServiceMock.Setup(s => s.ObterDosesPorVinculoIdAsync(1)).ReturnsAsync(dosesFake);

            // Act
            await _viewModel.InicializarAsync(tratamentoId: 10, medicamentoTratamentoId: 1);

            // Assert
            Assert.Equal("1 comprimido", _viewModel.Dose);
            Assert.Equal(12, _viewModel.IntervaloHoras);
            Assert.True(_viewModel.Ativado);
            Assert.Equal(medFake, _viewModel.MedicamentoSelecionado);
            Assert.True(_viewModel.TemMedicamentoSelecionado);
            Assert.Equal(2, _viewModel.DosesGeradas.Count);
            Assert.True(_viewModel.PossuiDosesGeradas);
            Assert.True(_viewModel.EstaEditando);
        }

        #endregion

        #region Método: AlterarMedicamento

        [Fact]
        public void AlterarMedicamento_QuandoJaPossuiDosesGeradas_NaoDeveLimparCampos()
        {
            // Arrange
            _viewModel.PossuiDosesGeradas = true;
            _viewModel.TextoBusca = "Mantido";

            // Act
            _viewModel.AlterarMedicamentoCommand.Execute(null);

            // Assert
            Assert.Equal("Mantido", _viewModel.TextoBusca);
        }

        [Fact]
        public void AlterarMedicamento_QuandoNaoPossuiDosesGeradas_DeveLimparSelecaoERecarregarLista()
        {
            // Arrange
            _viewModel.PossuiDosesGeradas = false;
            _viewModel.TextoBusca = "Removido";
            _agendamentoServiceMock.Setup(s => s.ObterTodosMedicamentosAsync()).ReturnsAsync(new List<Medicamento>());

            // Act
            _viewModel.AlterarMedicamentoCommand.Execute(null);

            // Assert
            Assert.Null(_viewModel.MedicamentoSelecionado);
            Assert.False(_viewModel.TemMedicamentoSelecionado);
            Assert.Empty(_viewModel.TextoBusca);
            _agendamentoServiceMock.Verify(s => s.ObterTodosMedicamentosAsync(), Times.Once);
        }

        #endregion

        #region Método: BuscarMedicamentos

        [Fact]
        public async Task BuscarMedicamentos_QuandoTextoVazio_DeveRetornarListaOriginalCompleta()
        {
            // Arrange
            var listaCompleta = new List<Medicamento> { new Medicamento { NomeComercial = "A" }, new Medicamento { NomeComercial = "B" } };
            _agendamentoServiceMock.Setup(s => s.ObterTodosMedicamentosAsync()).ReturnsAsync(listaCompleta);
            await _viewModel.InicializarAsync(10, 0); // Preenche lista original interna

            _viewModel.TextoBusca = "   ";

            // Act
            _viewModel.BuscarMedicamentosCommand.Execute(null);

            // Assert
            Assert.Equal(2, _viewModel.SugestoesBusca.Count);
        }

        [Fact]
        public async Task BuscarMedicamentos_QuandoTextoInformado_DeveFiltrarPorNomeComercial()
        {
            // Arrange
            var listaCompleta = new List<Medicamento> { new Medicamento { NomeComercial = "Paracetamol" }, new Medicamento { NomeComercial = "Ibuprofeno" } };
            _agendamentoServiceMock.Setup(s => s.ObterTodosMedicamentosAsync()).ReturnsAsync(listaCompleta);
            await _viewModel.InicializarAsync(10, 0);

            _viewModel.TextoBusca = "para";

            // Act
            _viewModel.BuscarMedicamentosCommand.Execute(null);

            // Assert
            Assert.Single(_viewModel.SugestoesBusca);
            Assert.Equal("Paracetamol", _viewModel.SugestoesBusca.First().NomeComercial);
        }

        #endregion

        #region Método: SelecionarMedicamento

        [Fact]
        public void SelecionarMedicamento_QuandoAcionado_DeveAtualizarEstadoESepararSelecao()
        {
            // Arrange
            var med = new Medicamento { Id = 8, NomeComercial = "Amoxicilina" };

            // Act
            _viewModel.SelecionarMedicamentoCommand.Execute(med);

            // Assert
            Assert.Equal(med, _viewModel.MedicamentoSelecionado);
            Assert.True(_viewModel.TemMedicamentoSelecionado);
            Assert.Equal("Amoxicilina", _viewModel.TextoBusca);
            Assert.Empty(_viewModel.SugestoesBusca);
        }

        #endregion

        #region Método: GerarDoses

        [Fact]
        public void GerarDoses_QuandoAcionado_DeveMapearIntervalosCorretamenteDentroDoPeriodo()
        {
            // Arrange
            _viewModel.DataInicio = DateTime.Today;
            _viewModel.HoraInicio = new TimeSpan(8, 0, 0); // Hoje 08:00
            _viewModel.DataFim = DateTime.Today;
            _viewModel.HoraFim = new TimeSpan(21, 0, 0);  // Hoje 21:00
            _viewModel.IntervaloHoras = 6;

            // Act
            _viewModel.GerarDosesCommand.Execute(null);

            // Assert
            // Esperado: 08:00, 14:00, 20:00 (3 Doses)
            Assert.Equal(3, _viewModel.DosesGeradas.Count);
            Assert.Equal(DateTime.Today.AddHours(8), _viewModel.DosesGeradas[0].HorarioOriginal);
            Assert.Equal(DateTime.Today.AddHours(14), _viewModel.DosesGeradas[1].HorarioOriginal);
            Assert.Equal(DateTime.Today.AddHours(20), _viewModel.DosesGeradas[2].HorarioOriginal);
            Assert.All(_viewModel.DosesGeradas, d => Assert.Equal("Pendente", d.Status));
        }

        #endregion

        #region Método: DeletarDosesAsync

        [Fact]
        public async Task DeletarDosesAsync_QuandoPodeDeletarDosesFalso_NaoDeveChamarServico()
        {
            // Arrange
            _viewModel.PossuiDosesGeradas = true;
            _viewModel.DosesGeradas.Add(new Agendamento { Status = "Tomado" }); // Bloqueia deleção

            // Act
            await _viewModel.DeletarDosesCommand.ExecuteAsync(null);

            // Assert
            _agendamentoServiceMock.Verify(s => s.DeletarDosesEAlarmeAsync(It.IsAny<List<Agendamento>>()), Times.Never);
        }

        [Fact]
        public async Task DeletarDosesAsync_QuandoSucessoNoServico_DeveLimparEstadoLocal()
        {
            // Arrange
            _viewModel.PossuiDosesGeradas = true;
            _viewModel.DosesGeradas.Add(new Agendamento { Status = "Pendente" });
            _agendamentoServiceMock.Setup(s => s.DeletarDosesEAlarmeAsync(It.IsAny<List<Agendamento>>())).ReturnsAsync(true);

            // Act
            await _viewModel.DeletarDosesCommand.ExecuteAsync(null);

            // Assert
            Assert.Empty(_viewModel.DosesGeradas);
            Assert.False(_viewModel.PossuiDosesGeradas);
            Assert.False(_viewModel.EstaEditando);
        }

        #endregion

        #region Método: SalvarAgendamentoAsync

        [Fact]
        public async Task SalvarAgendamentoAsync_QuandoNaoPossuiMedicamentoSelecionado_DeveAbortarOperacao()
        {
            // Arrange
            _viewModel.MedicamentoSelecionado = null;

            // Act
            await _viewModel.SalvarAgendamentoCommand.ExecuteAsync(null);

            // Assert
            _agendamentoServiceMock.Verify(s => s.ObterTratamentoPorIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task SalvarAgendamentoAsync_QuandoTratamentoNaoEncontrado_DeveExibirAlerta()
        {
            // Arrange
            _viewModel.MedicamentoSelecionado = new Medicamento { Id = 1 };
            _viewModel.TratamentoId = 99;
            _agendamentoServiceMock.Setup(s => s.ObterTratamentoPorIdAsync(99)).ReturnsAsync((Tratamento)null);

            // Act
            await _viewModel.SalvarAgendamentoCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Erro", "Tratamento de origem não encontrado.", "OK"), Times.Once);
        }

        [Fact]
        public async Task SalvarAgendamentoAsync_QuandoDetectadaDuplicidade_DeveExibirAlertaEBloquear()
        {
            // Arrange
            _viewModel.MedicamentoSelecionado = new Medicamento { Id = 2, NomeComercial = "Gliclazida" };
            _viewModel.TratamentoId = 10;
            var tratamentoFake = new Tratamento { PacienteId = 5 };

            _agendamentoServiceMock.Setup(s => s.ObterTratamentoPorIdAsync(10)).ReturnsAsync(tratamentoFake);
            _conflitoServiceMock.Setup(c => c.VerificarDuplicidadeMedicamentoEmAndamentoAsync(5, 2, 0)).ReturnsAsync(true);

            // Act
            await _viewModel.SalvarAgendamentoCommand.ExecuteAsync(null);

            // Assert
            _dialogServiceMock.Verify(d => d.DisplayAlert("Medicamento em Uso", It.Is<string>(s => s.Contains("já possui um agendamento ativo")), "OK"), Times.Once);
            _agendamentoServiceMock.Verify(s => s.SalvarNovoAgendamentoAsync(It.IsAny<int>(), It.IsAny<Medicamento>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<Agendamento>>()), Times.Never);
        }
        [Fact]
        public async Task SalvarAgendamentoAsync_QuandoHaConflitosEUsuarioNegaConfirmacao_DeveAbortarSalvar()
        {
            // Arrange
            _viewModel.MedicamentoSelecionado = new Medicamento { Id = 3 };
            _viewModel.MedicamentoTratamentoId = 0; // Novo agendamento
            var tratamentoFake = new Tratamento { PacienteId = 5 };

            _agendamentoServiceMock.Setup(s => s.ObterTratamentoPorIdAsync(It.IsAny<int>())).ReturnsAsync(tratamentoFake);
            _conflitoServiceMock.Setup(c => c.VerificarDuplicidadeMedicamentoEmAndamentoAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);
            _conflitoServiceMock.Setup(c => c.VerificarConflitosAsync(It.IsAny<Tratamento>(), It.IsAny<List<DateTime>>())).ReturnsAsync(new List<Agendamento> { new Agendamento() });

            _dialogServiceMock.Setup(d => d.DisplayConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), "Sim", "Não")).ReturnsAsync(false); // Clicou em "Não"

            // Act
            await _viewModel.SalvarAgendamentoCommand.ExecuteAsync(null);

            // Assert
            _agendamentoServiceMock.Verify(s => s.SalvarNovoAgendamentoAsync(It.IsAny<int>(), It.IsAny<Medicamento>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<Agendamento>>()), Times.Never);
        }

        [Fact]
        public async Task SalvarAgendamentoAsync_QuandoNovoEValido_DeveSalvarNovoAgendamentoComSucesso()
        {
            // Arrange
            _viewModel.MedicamentoSelecionado = new Medicamento { Id = 3 };
            _viewModel.MedicamentoTratamentoId = 0;
            _viewModel.Dose = "2mg";
            _viewModel.IntervaloHoras = 8;
            _viewModel.Ativado = true;

            var tratamentoFake = new Tratamento { PacienteId = 5 };

            _agendamentoServiceMock.Setup(s => s.ObterTratamentoPorIdAsync(It.IsAny<int>())).ReturnsAsync(tratamentoFake);
            _conflitoServiceMock.Setup(c => c.VerificarConflitosAsync(It.IsAny<Tratamento>(), It.IsAny<List<DateTime>>())).ReturnsAsync(new List<Agendamento>());
            _agendamentoServiceMock.Setup(s => s.SalvarNovoAgendamentoAsync(It.IsAny<int>(), _viewModel.MedicamentoSelecionado, "2mg", 8, true, It.IsAny<List<Agendamento>>())).ReturnsAsync(true);

            // Act
            await _viewModel.SalvarAgendamentoCommand.ExecuteAsync(null);

            // Assert
            Assert.True(_viewModel.PossuiDosesGeradas);
            Assert.True(_viewModel.EstaEditando);
            _agendamentoServiceMock.Verify(s => s.SalvarNovoAgendamentoAsync(It.IsAny<int>(), It.IsAny<Medicamento>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<Agendamento>>()), Times.Once);
        }

        [Fact]
        public async Task SalvarAgendamentoAsync_QuandoEdicaoEValida_DeveAtualizarAgendamentoExistenteComSucesso()
        {
            // Arrange
            _viewModel.MedicamentoSelecionado = new Medicamento { Id = 3 };
            _viewModel.MedicamentoTratamentoId = 123; // Indicativo de Edição
            _viewModel.Dose = "5ml";
            _viewModel.IntervaloHoras = 6;
            _viewModel.Ativado = false;

            var tratamentoFake = new Tratamento { PacienteId = 5 };

            _agendamentoServiceMock.Setup(s => s.ObterTratamentoPorIdAsync(It.IsAny<int>())).ReturnsAsync(tratamentoFake);
            _agendamentoServiceMock.Setup(s => s.AtualizarAgendamentoExistenteAsync(123, "5ml", 6, false)).ReturnsAsync(true);

            // Act
            await _viewModel.SalvarAgendamentoCommand.ExecuteAsync(null);

            // Assert
            Assert.True(_viewModel.PossuiDosesGeradas);
            Assert.True(_viewModel.EstaEditando);
            _agendamentoServiceMock.Verify(s => s.AtualizarAgendamentoExistenteAsync(123, "5ml", 6, false), Times.Once);
            _agendamentoServiceMock.Verify(s => s.SalvarNovoAgendamentoAsync(It.IsAny<int>(), It.IsAny<Medicamento>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<Agendamento>>()), Times.Never);
        }

        #endregion

        #region Método: VoltarAsync

        [Fact]
        public async Task VoltarAsync_QuandoExecutado_DeveNavegarParaTelaAnterior()
        {
            // Act
            await _viewModel.VoltarCommand.ExecuteAsync(null);

            // Assert
            _navigationMock.Verify(n => n.GoToAsync(".."), Times.Once);
        }

        #endregion
    }
}

