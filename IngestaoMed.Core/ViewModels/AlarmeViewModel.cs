using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Data;
using IngestaoMed.Core.Enums;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Services;
using static System.Net.Mime.MediaTypeNames;

namespace IngestaoMed.Core.ViewModels
{
    public partial class AlarmeViewModel : ObservableObject
    {
        private readonly IDatabaseContext _db;
        private readonly IAlarmService _alarmService;
        private readonly IAgendamentoService _agendamentoService;
        private readonly IPacienteService _pacienteService;
        private readonly ISnoozeService _snoozeService;
        private readonly ISnoozeScheduler _snoozeScheduler;
        private readonly ILogService _logService;
        private readonly IMonitorFalhaService _monitorFalhaService;
        private readonly ITimerService _timerService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private Agendamento? agendamentoAtual;

        [ObservableProperty]
        private Paciente? pacienteAtual;

        [ObservableProperty]
        private Medicamento? medicamentoAtual;

        [ObservableProperty]
        private MedicamentoTratamento? vinculoAtual;

        [ObservableProperty]
        private string _tempoContador;

        [ObservableProperty]
        private bool _estaAtrasado;


        private int _agendamentoId;
        public int AgendamentoId
        {
            get => _agendamentoId;
            set
            {
                if (SetProperty(ref _agendamentoId, value))
                {
                    _ = CarregarDadosAlarmeAsync(value);
                }
            }
        }

        public AlarmeViewModel(
            IDatabaseContext db,
            IAlarmService alarmService,
            IAgendamentoService agendamentoService,
            IPacienteService pacienteService,
            ISnoozeService snoozeService,
            ISnoozeScheduler snoozeScheduler,
            ILogService logService,
            IMonitorFalhaService monitorFalhaService,
            ITimerService timerService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _db = db;
            _alarmService = alarmService;
            _agendamentoService = agendamentoService;
            _pacienteService = pacienteService;
            _snoozeService = snoozeService;
            _snoozeScheduler = snoozeScheduler;
            _logService = logService;
            _monitorFalhaService = monitorFalhaService;
            _timerService = timerService;
            _dialogService = dialogService;
            _navigationService = navigationService;
        }

        public async Task CarregarDadosAlarmeAsync(int agendamentoId)
        {
            try
            {
                var agendamento = await _db.BuscarPrimeiroAsync<Agendamento>(a => a.Id == agendamentoId);
                if (agendamento == null) return;

                var vinculo = await _agendamentoService.ObterVinculoPorIdAsync(agendamento.MedicamentoTratamentoId);
                if (vinculo == null) return;

                var medicamento = await _agendamentoService.ObterMedicamentoPorIdAsync(vinculo.MedicamentoId);

                var tratamento = await _agendamentoService.ObterTratamentoPorIdAsync(vinculo.TratamentoId);

                if (tratamento != null)
                {
                    PacienteAtual = await _pacienteService.BuscarPacientePorIdAsync(tratamento.PacienteId);
                }

                VinculoAtual = vinculo;
                MedicamentoAtual = medicamento;
                AgendamentoAtual = agendamento;
                IniciarContador();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar dados do alarme: {ex.Message}");
            }
        }

        private void IniciarContador()
        {
            _timerService.Iniciar(TimeSpan.FromSeconds(1), AtualizarContador);
        }

        private void AtualizarContador()
        {
            if (AgendamentoAtual == null) return;

            var agora = DateTime.Now;
            var horarioAlarme = AgendamentoAtual.ProximoAlarme;

            EstaAtrasado = agora >= horarioAlarme;

            if (EstaAtrasado)
            {
                var dif = agora - horarioAlarme;
                TempoContador = $"-{dif:hh\\:mm\\:ss}";
            }
            else
            {
                var dif = horarioAlarme - agora;
                TempoContador = $"{dif:mm\\:ss}";
            }
        }

        [RelayCommand]
        private async Task ConfirmarIngestao()
        {
            if (AgendamentoAtual == null) return;

            AgendamentoAtual.Status = "Tomado";
            AgendamentoAtual.HorarioConfirmacao = DateTime.Now;

            await _db.AtualizarAsync(AgendamentoAtual);
            _snoozeScheduler.LimparHistorico(AgendamentoAtual.Id);
            await _logService.RegistrarAsync(AgendamentoAtual.Id, TipoEventoLog.ConfirmacaoDireta);
            await _alarmService.CancelarAlarmeAsync(AgendamentoAtual.Id);
            await _dialogService.DisplayAlert("Atenção", "Muito bem, continue assim!", "OK");
            await _agendamentoService.SincronizarFilaDeAlarmesAsync();
            await _navigationService.GoToAsync("//ListaPacientesPage");

        }

        [RelayCommand]
        private async Task AdiarSoneca()
        {
            if (AgendamentoAtual == null) return;
            if (_snoozeScheduler.PodeAdiar(AgendamentoAtual.Id))
            {
                await _snoozeService.AgendarSonecaAsync(AgendamentoAtual.Id, 1);
                await _logService.RegistrarAsync(AgendamentoAtual.Id, TipoEventoLog.SonecaDisparada);
                await CarregarDadosAlarmeAsync(AgendamentoAtual.Id);

            }
            else
            {
                AgendamentoAtual.Status = "Perdido";
                await _db.AtualizarAsync(AgendamentoAtual);
                int totalSonecas = _snoozeScheduler.ObterTentativas(AgendamentoAtual.Id);
                await _logService.RegistrarAsync(AgendamentoAtual.Id, TipoEventoLog.AtrasoCritico);
                await _monitorFalhaService.VerificarELoggerFalhaAsync(AgendamentoAtual.Id, totalSonecas);
                await _dialogService.DisplayAlert("Atenção", "Limite de adiamentos atingido. O cuidador foi notificado.", "OK");
                await _agendamentoService.SincronizarFilaDeAlarmesAsync();
                await _navigationService.GoToAsync("//ListaPacientesPage");
            }
        }
    }
}