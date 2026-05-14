using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Collections.ObjectModel;

namespace IngestaoMed.Core.ViewModels
{
    public partial class TratamentoViewModel : ObservableObject
    {
        private readonly IDatabaseContext _db;
        private readonly IDialogService _dialog;
        private readonly INavigationService _navigation;
        private readonly IAgendamentoConflitoService _conflitoService;

        [ObservableProperty]
        private ObservableCollection<Paciente> pacientes = new();

        [ObservableProperty]
        private ObservableCollection<Medicamento> medicamentos = new();

        [ObservableProperty]
        private Paciente? pacienteSelecionado;

        [ObservableProperty]
        private Medicamento? medicamentoSelecionado;

        [ObservableProperty]
        private string nome = string.Empty;

        [ObservableProperty]
        private string? descricao;

        [ObservableProperty]
        private string dosagem = string.Empty;

        [ObservableProperty]
        private int intervaloHoras;

        [ObservableProperty]
        private DateTime dataInicio = DateTime.Now;

        [ObservableProperty]
        private DateTime? dataFim;

        public TratamentoViewModel(IDatabaseContext db, IDialogService dialog, INavigationService navigation, IAgendamentoConflitoService conflitoService)
        {
            _db = db;
            _dialog = dialog;
            _navigation = navigation;
            _conflitoService = conflitoService;
        }

        public async Task CarregarDadosIniciais()
        {
            var listaPacientes = await _db.BuscarTodosAsync<Paciente>();
            var listaMedicamentos = await _db.BuscarTodosAsync<Medicamento>();

            Pacientes = new ObservableCollection<Paciente>(listaPacientes);
            Medicamentos = new ObservableCollection<Medicamento>(listaMedicamentos);
        }

        [RelayCommand]
        private async Task Salvar()
        {
            // 1. Validação agora foca apenas no "Grupo" de tratamento e no Paciente
            if (PacienteSelecionado == null || string.IsNullOrWhiteSpace(Nome))
            {
                await _dialog.DisplayAlert("Erro", "Selecione o paciente e dê um nome ao tratamento.", "OK");
                return;
            }

            // 2. Validação de datas
            if (DataFim.HasValue && DataFim.Value < DataInicio)
            {
                await _dialog.DisplayAlert("Erro", "A data de término não pode ser anterior ao início.", "OK");
                return;
            }

            // 3. Criação do "Mestre" (Tratamento)
            var novoTratamento = new Tratamento
            {
                Nome = Nome,
                Descricao = Descricao,
                PacienteId = PacienteSelecionado.Id,
                DataInicio = DataInicio,
                DataFim = DataFim,
                Ativo = true
            };

            bool sucesso = await _db.InserirAsync(novoTratamento);

            if (sucesso)
            {
                await _navigation.GoToAsync($"AdicionarRemediosPage?tratamentoId={novoTratamento.Id}");
            }
        }

        private async Task GerarAgenda(Tratamento tratamento)
        {
            DateTime dataProjetada = tratamento.DataInicio;

            // Define um limite de segurança (30 dias) para uso contínuo ou utiliza a data de término
            DateTime dataLimite = tratamento.DataFim ?? DateTime.Now.AddDays(30);

            while (dataProjetada <= dataLimite)
            {
                var agendamento = new Agendamento
                {
                    TratamentoId = tratamento.Id,
                    HorarioProgramado = dataProjetada,
                    Status = "Pendente",
                    HorarioRealizado = null
                };

                await _db.InserirAsync(agendamento);

                // Incrementa a data com base no intervalo definido no tratamento
                dataProjetada = dataProjetada.AddHours(tratamento.IntervaloHoras);
            }
        }
    }
}