using IngestaoMed.Core.Models;
using IngestaoMed.Core.Interfaces;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using IngestaoMed.Interfaces;

namespace IngestaoMed.Services.Notifications
{
    public class NotificationMapper : INotificationMapper
    {
        public NotificationRequest MapToRequest(Agendamento agendamento)
        {
            if (agendamento == null)
                return null;

            return new NotificationRequest
            {
                NotificationId = agendamento.Id,
                Title = "💊 Hora do Medicamento",
                Description = "Toque para abrir os detalhes e confirmar a ingestão.",
                ReturningData = agendamento.Id.ToString(),
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