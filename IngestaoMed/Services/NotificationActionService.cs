using IngestaoMed.Core.Constants;
using IngestaoMed.Core.Enums;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.ViewModels;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;
using System.Threading.Tasks;

namespace IngestaoMed.Services
{
    public class NotificationActionService : INotificationActionService
    {
        private readonly AlarmeViewModel _viewModel;
        private readonly ILogService _logService;
        private readonly ISnoozeScheduler _snoozeScheduler;
        private readonly ISnoozeService _snoozeService;
        private readonly IMonitorFalhaService _monitorFalhaService;

        public NotificationActionService(
            AlarmeViewModel viewModel,
            ILogService logService,
            ISnoozeScheduler snoozeScheduler,
            ISnoozeService snoozeService,
            IMonitorFalhaService monitorFalhaService)
        {
            _viewModel = viewModel;
            _logService = logService;
            _snoozeScheduler = snoozeScheduler;
            _snoozeService = snoozeService;
            _monitorFalhaService = monitorFalhaService;

            LocalNotificationCenter.Current.NotificationActionTapped -= OnNotificationActionTapped;
            LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;
        }

        // Método mantido limpo, já que não há mais botões nativos no card de notificação
        public void RegistrarAcoes()
        {
            // Categoria vazia ou apenas registro básico se o plugin exigir, sem ActionList com botões.
        }

        private async void OnNotificationActionTapped(NotificationActionEventArgs e)
        {
            if (int.TryParse(e.Request.ReturningData, out int agendamentoId))
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync($"AlarmPage?agendamentoId={agendamentoId}");
                });
            }
        }

        public async Task ProcessarAcaoAsync(int actionId, int agendamentoId)
        {
            _viewModel.AgendamentoAtual = new Agendamento { Id = agendamentoId };

            if (actionId == NotificationConstants.ActionTomeiId)
            {
                _snoozeScheduler.LimparHistorico(agendamentoId);
                await _logService.RegistrarAsync(agendamentoId, TipoEventoLog.ConfirmacaoDireta);
                await _viewModel.ConfirmarIngestaoCommand.ExecuteAsync(null);
            }
            else if (actionId == NotificationConstants.ActionSonecaId)
            {
                if (_snoozeScheduler.PodeAdiar(agendamentoId))
                {
                    await _snoozeService.AgendarSonecaAsync(agendamentoId, 1);
                    await _logService.RegistrarAsync(agendamentoId, TipoEventoLog.SonecaDisparada);
                    await _viewModel.AdiarSonecaCommand.ExecuteAsync(null);
                }
                else
                {
                    int totalSonecas = _snoozeScheduler.ObterTentativas(agendamentoId);
                    await _logService.RegistrarAsync(agendamentoId, TipoEventoLog.AtrasoCritico);
                    await _monitorFalhaService.VerificarELoggerFalhaAsync(agendamentoId, totalSonecas);
                }
            }
        }
    }
}