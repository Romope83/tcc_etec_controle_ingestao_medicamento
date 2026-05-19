using IngestaoMed;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Services;

namespace IngestaoMed;
using Plugin.LocalNotification;

public partial class App : Application
{
    private readonly IAuthService _authService;
    private readonly AppShell _shell;
    private readonly IConfigService _configService;

    public App(IAuthService authService, AppShell shell, IConfigService configService)
    {
        InitializeComponent();
        _authService = authService;
        _shell = shell;
        _configService = configService;

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
        await WindowsInicialization();
    }

    private async Task WindowsInicialization()
    {
        if (_configService.EhPrimeiroAcesso)
        {            
            await Shell.Current.GoToAsync("//WelcomePage");
        }
        else
        {
            bool existeUsuario = await _authService.ExisteCuidadorCadastrado();

            if (existeUsuario)
            {
                await Shell.Current.GoToAsync("//LoginPage");
            }
            else
            {
                await Shell.Current.GoToAsync("CuidadorPage");
            }
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_shell);
    }

}