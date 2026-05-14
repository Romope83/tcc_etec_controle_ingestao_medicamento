using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;

namespace IngestaoMed.Services
{
    public static class NotificationHelper
    {
        public static void Initialize()
        {
            LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;
        }

        private static void OnNotificationActionTapped(NotificationActionEventArgs e)
        {
            if (e.IsTapped)
            {
                // Lógica para abrir uma tela específica ou marcar como lido
            }

            if (e.ActionId == 100) // Exemplo de ID para botão "Tomei"
            {
                // Lógica para registrar ingestão rápida
            }
        }
    }
}