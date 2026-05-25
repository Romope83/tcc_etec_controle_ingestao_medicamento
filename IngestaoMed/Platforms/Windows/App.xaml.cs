using Microsoft.UI.Xaml;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IngestaoMed.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();


protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        // Obtém a ativação atual
        var activatedEventArgs = AppInstance.GetCurrent().GetActivatedEventArgs();

        // Verifica se já existe uma instância do app rodando
        var firstInstance = AppInstance.FindOrRegisterForKey("IngestaoMed_UniqueKey");

        if (!firstInstance.IsCurrent)
        {
            // Se já existe outra aberta, redireciona os argumentos (o clique do alerta) para ela
            firstInstance.RedirectActivationToAsync(activatedEventArgs).AsTask().Wait();

            // Fecha esta nova instância que tentou abrir por engano
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            return;
        }

        // Se for a primeira instância, segue o jogo normalmente
        base.OnLaunched(args);
    }

}

}
