/*using Moq;
using Xunit;
using IngestaoMed.Core.ViewModels;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Data;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace IngestaoMed.Tests.ViewModels
{
    public class AgendamentoViewModelTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly Mock<INavigationService> _navigationMock;
        private readonly Mock<IDialogService> _dialogMock;
        private readonly Mock<IAgendamentoConflitoService> _conflitoMock;
        private readonly AgendamentoViewModel _viewModel;

        public AgendamentoViewModelTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _navigationMock = new Mock<INavigationService>();
            _dialogMock = new Mock<IDialogService>();
            _conflitoMock = new Mock<IAgendamentoConflitoService>();

            _viewModel = new AgendamentoViewModel(
                _dbMock.Object,
                _navigationMock.Object,
                _dialogMock.Object,
                _conflitoMock.Object);
        }

        #region Testes: InicializarAsync e Carregamento

        [Fact]
        public async Task InicializarAsync_ModoCadastro_DeveCarregarMedicamentosEConfigurarEstadoLimpo()
        {
            // Arrange
            var medicamentos = new List<Medicamento> { new Medicamento { Id = 1, NomeComercial = "Dipirona" } };
            _dbMock.Setup(d => d.BuscarTodosAsync<Medicamento>()).ReturnsAsync(medicamentos);

            // Act
            await _viewModel.InicializarAsync(tratamentoId: 5, medicamentoTratamentoId: 0);

            // Assert
            Assert.Equal(5, _viewModel.TratamentoId);
            Assert.Equal(0, _viewModel.MedicamentoTratamentoId);
            Assert.False(_viewModel.TemMedicamentoSelecionado);
            Assert.False(_viewModel.PossuiDosesGeradas);
            Assert.Equal(1, _viewModel.SugestoesBusca.Count);
        }

        [Fact]
        public async Task InicializarAsync_ModoEdicaoSucesso_DevePopularCamposEDosesExistentes()
        {
            // Arrange
            int mtId = 50;
            var vinculo = new MedicamentoTratamento { Id = mtId, MedicamentoId = 1, Dosagem = "20 gotas", IntervaloHoras = 6, Ativo = true };
            var med = new Medicamento { Id = 1, NomeComercial = "Paracetamol" };
            var doses = new List<Agendamento>
            {
                new Agendamento { Id = 10, MedicamentoTratamentoId = mtId, HorarioOriginal = DateTime.Now, Status = "Pendente" }
            };

            _dbMock.Setup(d => d.BuscarPrimeiroAsync<MedicamentoTratamento>(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>())).ReturnsAsync(vinculo);
            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Medicamento>(It.IsAny<Expression<Func<Medicamento, bool>>>())).ReturnsAsync(med);
            _dbMock.Setup(d => d.BuscarOndeAsync<Agendamento>(It.IsAny<Expression<Func<Agendamento, bool>>>())).ReturnsAsync(doses);

            // Act
            await _viewModel.InicializarAsync(tratamentoId: 5, medicamentoTratamentoId: mtId);

            // Assert
            Assert.Equal("20 gotas", _viewModel.Dose);
            Assert.Equal(6, _viewModel.IntervaloHoras);
            Assert.True(_viewModel.TemMedicamentoSelecionado);
            Assert.True(_viewModel.PossuiDosesGeradas);
            Assert.Single(_viewModel.DosesGeradas);
        }

        #endregion

        #region Testes: Comandos de Busca e Seleção

        [Fact]
        public async Task BuscarMedicamentos_TextoPreenchido_DeveFiltrarListaOriginal()
        {
            // Arrange
            var medicamentos = new List<Medicamento>
            {
                new Medicamento { Id = 1, NomeComercial = "Dipirona" },
                new Medicamento { Id = 2, NomeComercial = "Losartana" }
            };
            _dbMock.Setup(d => d.BuscarTodosAsync<Medicamento>()).ReturnsAsync(medicamentos);
            await _viewModel.InicializarAsync(5, 0);

            // Act
            _viewModel.TextoBusca = "Dipi";
            // Invocação direta do método gerado pelo RelayCommand (BuscarMedicamentosCommand)
            _viewModel.BuscarMedicamentosCommand.Execute(null);

            // Assert
            Assert.Single(_viewModel.SugestoesBusca);
            Assert.Equal("Dipirona", _viewModel.SugestoesBusca.First().NomeComercial);
        }

        [Fact]
        public void SelecionarMedicamento_DeveAtualizarEstadoETextoBusca()
        {
            // Arrange
            var med = new Medicamento { Id = 1, NomeComercial = "Amoxicilina" };

            // Act
            _viewModel.SelecionarMedicamentoCommand.Execute(med);

            // Assert
            Assert.Equal(med, _viewModel.MedicamentoSelecionado);
            Assert.True(_viewModel.TemMedicamentoSelecionado);
            Assert.Equal("Amoxicilina", _viewModel.TextoBusca);
            Assert.Empty(_viewModel.SugestoesBusca);
        }

        [Fact]
        public void AlterarMedicamento_ComDosesSalvas_DeveBloquearEPrevenirLimpeza()
        {
            // Arrange
            _viewModel.PossuiDosesGeradas = true;
            _viewModel.MedicamentoSelecionado = new Medicamento { Id = 1, NomeComercial = "Dipirona" };

            // Act
            _viewModel.AlterarMedicamentoCommand.Execute(null);

            // Assert
            Assert.NotNull(_viewModel.MedicamentoSelecionado); // Não limpou por segurança
        }

        #endregion

        #region Testes: Regras de Tolerância e Geração de Doses

        [Fact]
        public void OnToleranciaChanged_DeveArredondarParaMultiploDeDez()
        {
            // Act
            _viewModel.Tolerancia = 24;

            // Assert
            Assert.Equal(20, _viewModel.Tolerancia);
        }

        [Fact]
        public void GerarDoses_DeveCalcularEPreencherGradeHorariaCorretamente()
        {
            // Arrange
            _viewModel.DataInicio = new DateTime(2026, 5, 20);
            _viewModel.HoraInicio = new TimeSpan(8, 0, 0); // 20/05/2026 08:00
            _viewModel.DataFim = new DateTime(2026, 5, 20);
            _viewModel.HoraFim = new TimeSpan(20, 0, 0);   // 20/05/2026 20:00
            _viewModel.IntervaloHoras = 6;                 // Deve gerar em: 08:00, 14:00, 20:00

            // Act
            _viewModel.GerarDosesCommand.Execute(null);

            // Assert
            Assert.Equal(3, _viewModel.DosesGeradas.Count);
            Assert.Equal("Pendente", _viewModel.DosesGeradas.First().Status);
        }

        #endregion

        #region Testes: DeletarDosesAsync

        [Fact]
        public async Task DeletarDosesAsync_DoseTomada_DeveBloquearExclusao()
        {
            // Arrange
            _viewModel.PossuiDosesGeradas = true;
            _viewModel.DosesGeradas.Add(new Agendamento { Id = 1, Status = "Tomado" });

            // Act
            await _viewModel.DeletarDosesCommand.ExecuteAsync(null);

            // Assert
            Assert.False(_viewModel.PodeDeletarDoses);
            _dbMock.Verify(d => d.ExcluirAsync(It.IsAny<Agendamento>()), Times.Never);
        }

        [Fact]
        public async Task DeletarDosesAsync_ApenasPendentes_DeveRemoverDoBancoEDaTela()
        {
            // Arrange
            _viewModel.PossuiDosesGeradas = true;
            var dose = new Agendamento { Id = 99, Status = "Pendente" };
            _viewModel.DosesGeradas.Add(dose);

            // Act
            await _viewModel.DeletarDosesCommand.ExecuteAsync(null);

            // Assert
            _dbMock.Verify(d => d.ExcluirAsync(It.Is<Agendamento>(a => a.Id == 99)), Times.Once);
            Assert.Empty(_viewModel.DosesGeradas);
            Assert.False(_viewModel.PossuiDosesGeradas);
            Assert.False(_viewModel.EstaEditando);
        }

        #endregion

        #region Testes: SalvarAgendamentoAsync (Conflitos e Duplicidades)

        [Fact]
        public async Task SalvarAgendamentoAsync_MedicamentoDuplicadoEmAndamento_DeveExibirAlertaEInterromper()
        {
            // Arrange
            _viewModel.TratamentoId = 5;
            _viewModel.MedicamentoSelecionado = new Medicamento { Id = 1, NomeComercial = "Dipirona" };

            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(new Tratamento { Id = 5, PacienteId = 2 });

            // Configura o serviço de conflito para apontar que já existe este remédio ativo no paciente
            _conflitoMock.Setup(c => c.VerificarDuplicidadeMedicamentoEmAndamentoAsync(2, 1, 0))
                         .ReturnsAsync(true);

            // Act
            await _viewModel.SalvarAgendamentoCommand.ExecuteAsync(null);

            // Assert
            _dialogMock.Verify(d => d.DisplayAlert("Medicamento em Uso", It.IsAny<string>(), "OK"), Times.Once);
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<MedicamentoTratamento>()), Times.Never); // Interrompeu o save
        }

        [Fact]
        public async Task SalvarAgendamentoAsync_ConflitoHorarioRecusadoPeloUsuario_DeveInterromperSalvamento()
        {
            // Arrange
            _viewModel.TratamentoId = 5;
            _viewModel.MedicamentoTratamentoId = 0; // Novo
            _viewModel.MedicamentoSelecionado = new Medicamento { Id = 1, NomeComercial = "Dipirona" };

            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(new Tratamento { Id = 5, PacienteId = 2 });

            _conflitoMock.Setup(c => c.VerificarDuplicidadeMedicamentoEmAndamentoAsync(2, 1, 0)).ReturnsAsync(false);

            // Simula que existem conflitos de 30 minutos na grade
            _conflitoMock.Setup(c => c.VerificarConflitosAsync(It.IsAny<Tratamento>(), It.IsAny<List<DateTime>>()))
                         .ReturnsAsync(new List<Agendamento> { new Agendamento { Id = 88 } });

            // Usuário escolhe "Não" (recusa o conflito de horários)
            _dialogMock.Setup(d => d.DisplayConfirmationAsync("Aviso de Conflito", It.IsAny<string>(), "Sim", "Não"))
                       .ReturnsAsync(false);

            // Act
            await _viewModel.SalvarAgendamentoCommand.ExecuteAsync(null);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<MedicamentoTratamento>()), Times.Never);
        }

        [Fact]
        public async Task SalvarAgendamentoAsync_NovoSemConflitos_DeveInserirVinculoEDoses()
        {
            // Arrange
            _viewModel.TratamentoId = 5;
            _viewModel.MedicamentoTratamentoId = 0;
            _viewModel.Dose = "1 comprimido";
            _viewModel.MedicamentoSelecionado = new Medicamento { Id = 1, NomeComercial = "Dipirona" };

            _viewModel.DataInicio = DateTime.Now;
            _viewModel.HoraInicio = TimeSpan.FromHours(8);
            _viewModel.DataFim = DateTime.Now;
            _viewModel.HoraFim = TimeSpan.FromHours(9); // Força gerar apenas 1 dose para simplificar o mock
            _viewModel.IntervaloHoras = 8;

            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(new Tratamento { Id = 5, PacienteId = 2 });

            _conflitoMock.Setup(c => c.VerificarDuplicidadeMedicamentoEmAndamentoAsync(2, 1, 0)).ReturnsAsync(false);
            _conflitoMock.Setup(c => c.VerificarConflitosAsync(It.IsAny<Tratamento>(), It.IsAny<List<DateTime>>())).ReturnsAsync(new List<Agendamento>());

            // Act
            await _viewModel.SalvarAgendamentoCommand.ExecuteAsync(null);

            // Assert
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<MedicamentoTratamento>()), Times.Once);
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<Agendamento>()), Times.AtLeastOnce);
            Assert.True(_viewModel.PossuiDosesGeradas);
            Assert.True(_viewModel.EstaEditando);
        }

        [Fact]
        public async Task SalvarAgendamentoAsync_ModoEdicao_DeveApenasAtualizarVinculoSemRegerarDoses()
        {
            // Arrange
            _viewModel.TratamentoId = 5;
            _viewModel.MedicamentoTratamentoId = 50; // Edição
            _viewModel.Dose = "Nova Dose 5ml";
            _viewModel.MedicamentoSelecionado = new Medicamento { Id = 1, NomeComercial = "Dipirona" };

            _dbMock.Setup(d => d.BuscarPrimeiroAsync<Tratamento>(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(new Tratamento { Id = 5, PacienteId = 2 });

            _dbMock.Setup(d => d.BuscarPrimeiroAsync<MedicamentoTratamento>(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(new MedicamentoTratamento { Id = 50 });

            _conflitoMock.Setup(c => c.VerificarDuplicidadeMedicamentoEmAndamentoAsync(2, 1, 50)).ReturnsAsync(false);

            // Act
            await _viewModel.SalvarAgendamentoCommand.ExecuteAsync(null);

            // Assert
            _dbMock.Verify(d => d.AtualizarAsync(It.IsAny<MedicamentoTratamento>()), Times.Once);
            _dbMock.Verify(d => d.InserirAsync(It.IsAny<Agendamento>()), Times.Never); // Edição simples não sobrescreve histórico de doses
        }

        #endregion

        #region Testes: Navegação

        [Fact]
        public async Task VoltarAsync_DeveChamarNavegacaoDeRetorno()
        {
            // Act
            await _viewModel.VoltarCommand.ExecuteAsync(null);

            // Assert
            _navigationMock.Verify(n => n.GoToAsync(".."), Times.Once);
        }

        #endregion
    }
}*/