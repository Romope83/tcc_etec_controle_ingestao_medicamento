using IngestaoMed;
using IngestaoMed.Core.Interfaces;

namespace IngestaoMed;
using Plugin.LocalNotification;

public partial class App : Application
{
    private readonly IAuthService _authService;
    private readonly AppShell _shell;
    private readonly IConfigService _configService;
    private readonly IEmailOutboxProcessor _processor;
    private CancellationTokenSource? _cts;

    public App(IAuthService authService, AppShell shell, IConfigService configService, IEmailOutboxProcessor processor)
    {
        InitializeComponent();
        _authService = authService;
        _shell = shell;
        _configService = configService;
        _processor = processor;

#if WINDOWS
            LocalNotificationCenter.Current.RegisterCategoryList(new HashSet<NotificationCategory>
            {
                new NotificationCategory(NotificationCategoryType.Alarm)
            });
#endif
    }


    protected override async void OnStart()
    {
        base.OnStart();
        IniciarTarefaBackground();
        await WindowsInicialization();
    }

    private async Task WindowsInicialization()
    {
        if (_configService.EhPrimeiroAcesso)
        {
            await Shell.Current.GoToAsync("//WelcomePage");
            return;
        }

        var config = _configService.ConfiguracaoCuidador;
        bool existeUsuario = await _authService.ExisteCuidadorCadastrado();

        if (config != null)
        {
            await Shell.Current.GoToAsync("//ListaPacientesPage");
        }
        else if (existeUsuario)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
        else
        {
            await Shell.Current.GoToAsync("//WelcomePage");
        }
    }



    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_shell);
    }
    private void IniciarTarefaBackground()
    {
        if (_cts != null && !_cts.IsCancellationRequested) return;

        _cts = new CancellationTokenSource();

        Task.Run(async () => await _processor.IniciarProcessamentoAsync(_cts.Token), _cts.Token);
    }
    private void PararTarefaBackground()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

}