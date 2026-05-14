using IngestaoMed.Core.Models;
using Plugin.LocalNotification;

namespace IngestaoMed.Interfaces
{
    public interface INotificationMapper
    {
        NotificationRequest MapToRequest(Agendamento agendamento);
    }
}