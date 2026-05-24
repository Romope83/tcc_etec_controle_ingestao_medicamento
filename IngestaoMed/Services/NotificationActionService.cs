using IngestaoMed.Core.Constants;
using IngestaoMed.Core.Enums;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.ViewModels;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;
using System.Collections.Generic;
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

        public void RegistrarAcoes()
        {
            var acaoConfirmar = new NotificationAction(NotificationConstants.ActionTomeiId)
            {
                Title = "Tomei",
                Android = { IconName = { ResourceName = "check_icon" } }
            };

            var acaoSoneca = new NotificationAction(NotificationConstants.ActionSonecaId)
            {
                Title = "Soneca (10 min)",
                Android = { IconName = { ResourceName = "snooze_icon" } }
            };

            var categoria = new NotificationCategory(NotificationCategoryType.Status)
            {
                ActionList = new HashSet<NotificationAction> { acaoConfirmar, acaoSoneca }
            };

            LocalNotificationCenter.Current.RegisterCategoryList(new HashSet<NotificationCategory> { categoria });
        }

        private async void OnNotificationActionTapped(NotificationActionEventArgs e)
        {
            int actionId = e.ActionId;

            if (int.TryParse(e.Request.ReturningData, out int agendamentoId))
            {
                await ProcessarAcaoAsync(actionId, agendamentoId);
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
                    await _snoozeService.AgendarSonecaAsync(agendamentoId, 10);
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