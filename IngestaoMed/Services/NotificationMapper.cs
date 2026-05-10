using IngestaoMed.Core.Models;
using IngestaoMed.Interfaces; // Referência para a nova pasta
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace IngestaoMed.Services.Notifications
{
    public class NotificationMapper : INotificationMapper
    {
        public NotificationRequest MapToRequest(Agendamento agendamento)
        {
            if (agendamento == null) return null;

            return new NotificationRequest
            {
                NotificationId = agendamento.Id,
                Title = "💊 Hora do seu Medicamento",
                Description = $"Está na hora de tomar: {agendamento.Tratamento?.NomeMedicamento}",
                ReturningData = agendamento.Id.ToString(),
                CategoryType = NotificationCategoryType.Status,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = agendamento.HorarioProgramado,
                    Android = new AndroidScheduleOptions { AlarmType = AndroidAlarmType.RtcWakeup}
                }
            };
        }
    }
}