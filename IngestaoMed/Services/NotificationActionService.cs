using IngestaoMed.Core.Constants;
using IngestaoMed.Core.Enums;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.ViewModels;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;

namespace IngestaoMed.Services
{
    public class NotificationActionService : INotificationActionService
    {
        private readonly AlarmeViewModel _viewModel;
        private readonly ILogService _logService;

        public NotificationActionService(AlarmeViewModel viewModel, ILogService logService)
        {
            _viewModel = viewModel;
            _logService = logService;
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

            // O plugin costuma usar NotificationCategoryType.Status ou o ID da string
            var categoria = new NotificationCategory(NotificationCategoryType.Status)
            {
                ActionList = new HashSet<NotificationAction> { acaoConfirmar, acaoSoneca }
            };

            LocalNotificationCenter.Current.RegisterCategoryList(new HashSet<NotificationCategory> { categoria });
        }

        private async void OnNotificationActionTapped(NotificationActionEventArgs e)
        {
            // O plugin retorna o ActionId como int, exatamente como suas novas constantes
            int actionId = e.ActionId;

            if (int.TryParse(e.Request.ReturningData, out int agendamentoId))
            {
                await ProcessarAcaoAsync(actionId, agendamentoId);
            }
        }

        public async Task ProcessarAcaoAsync(int actionId, int agendamentoId)
        {
            // Vincula o ID à VM para que os comandos saibam qual registro alterar no banco
            _viewModel.AgendamentoAtual = new Agendamento { Id = agendamentoId };

            if (actionId == NotificationConstants.ActionTomeiId)
            {
                await _logService.RegistrarAsync(agendamentoId, TipoEventoLog.ConfirmacaoDireta);
                await _viewModel.ConfirmarIngestaoCommand.ExecuteAsync(null);
            }
            else if (actionId == NotificationConstants.ActionSonecaId)
            {
                await _logService.RegistrarAsync(agendamentoId, TipoEventoLog.SonecaDisparada);
                await _viewModel.AdiarSonecaCommand.ExecuteAsync(null);
            }
        }
    }
}