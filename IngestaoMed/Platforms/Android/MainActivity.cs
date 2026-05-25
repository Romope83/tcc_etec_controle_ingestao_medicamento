using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace IngestaoMed
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ActivityStateManager.Default.Init(this, savedInstanceState);

            var listaCanais = new List<NotificationChannelRequest>
            {
                new NotificationChannelRequest
                {
                    Id = "ingestaomed_alarmes",
                    Name = "Lembretes de Medicamentos",
                    Importance = AndroidImportance.High,
                    Description = "Canal de notificações para os alarmes do IngestaoMed"
                }
            };

            LocalNotificationCenter.CreateNotificationChannels(listaCanais);
        }
        // ADICIONE ESTE MÉTODO ABAIXO DO ONCREATE:
        protected override async void OnResume()
        {
            base.OnResume();

            try
            {
                // Só verifica a permissão quando a Activity já está carregada e segura em primeiro plano
                bool temPermissao = await LocalNotificationCenter.Current.AreNotificationsEnabled();

                if (!temPermissao)
                {
                    await LocalNotificationCenter.Current.RequestNotificationPermission();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Android] Erro ao pedir permissão: {ex.Message}");
            }
        }
    }
}
