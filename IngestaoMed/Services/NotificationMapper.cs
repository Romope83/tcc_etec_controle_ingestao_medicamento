using IngestaoMed.Core.Models;
using IngestaoMed.Core.Interfaces; // Ajustado para seu namespace de interfaces
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using IngestaoMed.Interfaces;

namespace IngestaoMed.Services.Notifications
{
    public class NotificationMapper : INotificationMapper
    {
        public NotificationRequest MapToRequest(Agendamento agendamento)
        {
            if (agendamento == null) return null;

            string nomeMedicamento = !string.IsNullOrWhiteSpace(agendamento.NomeRemedioEspecifico)
                ? agendamento.NomeRemedioEspecifico
                : "Medicamento";

            return new NotificationRequest
            {
                NotificationId = agendamento.Id,
                Title = "💊 Hora do seu Medicamento",
                // Agora exibe o remédio exato vinculado a este horário
                Description = $"Está na hora de tomar: {nomeMedicamento}",
                ReturningData = agendamento.Id.ToString(),
                CategoryType = NotificationCategoryType.Status,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = agendamento.ProximoAlarme,
                    Android = new AndroidScheduleOptions
                    {
                        AlarmType = AndroidAlarmType.RtcWakeup
                    }
                }
            };
        }
    }
}