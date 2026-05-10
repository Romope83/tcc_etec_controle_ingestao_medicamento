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
            // 1. Validações de preenchimento
            if (PacienteSelecionado == null || MedicamentoSelecionado == null || string.IsNullOrWhiteSpace(Nome))
            {
                await _dialog.DisplayAlert("Erro", "Selecione o paciente, o medicamento e dê um nome ao tratamento.", "OK");
                return;
            }

            // 2. Validação de intervalo mínimo
            if (IntervaloHoras <= 0)
            {
                await _dialog.DisplayAlert("Erro", "O intervalo entre as doses deve ser maior que zero.", "OK");
                return;
            }

            // 3. Validação de consistência de datas
            if (DataFim.HasValue && DataFim.Value < DataInicio)
            {
                await _dialog.DisplayAlert("Erro", "A data de término não pode ser anterior ao início.", "OK");
                return;
            }

            // 4. Projeção de horários para verificação de conflitos
            var horariosPretendidos = new List<DateTime>();
            DateTime dataProjetada = DataInicio;
            DateTime dataLimite = DataFim ?? DateTime.Now.AddDays(30);

            while (dataProjetada <= dataLimite)
            {
                horariosPretendidos.Add(dataProjetada);
                dataProjetada = dataProjetada.AddHours(IntervaloHoras);
            }

            // 5. Verificação de conflitos com agendamentos existentes
            var tratamentoTemp = new Tratamento { PacienteId = PacienteSelecionado.Id };
            var conflitos = await _conflitoService.VerificarConflitosAsync(tratamentoTemp, horariosPretendidos);

            if (conflitos.Any())
            {
                bool prosseguir = await _dialog.DisplayAlert("Atenção",
                    $"Existem {conflitos.Count} agendamentos em horários próximos (janela de 30min). Deseja continuar?", "Sim", "Não");

                if (!prosseguir) return;
            }

            // 6. Criação do objeto de tratamento
            var novoTratamento = new Tratamento
            {
                Nome = Nome,
                Descricao = Descricao,
                PacienteId = PacienteSelecionado.Id,
                MedicamentoId = MedicamentoSelecionado.Id,
                Dosagem = Dosagem,
                IntervaloHoras = IntervaloHoras,
                DataInicio = DataInicio,
                DataFim = DataFim,
                Ativo = true
            };

            // 7. Persistência e Geração da Agenda
            bool sucesso = await _db.InserirAsync(novoTratamento);

            if (sucesso)
            {
                await GerarAgenda(novoTratamento);
                await _navigation.GoToAsync("..");
            }
            else
            {
                await _dialog.DisplayAlert("Erro", "Não foi possível salvar o tratamento.", "OK");
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