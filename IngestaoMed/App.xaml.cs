using IngestaoMed;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Services;

namespace IngestaoMed;

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
    }


    protected override async void OnStart()
    {
        base.OnStart();
        await WindowsInicialization();
    }

    private async Task WindowsInicialization()
    {
        _configService.EhPrimeiroAcesso = true;
        if (_configService.EhPrimeiroAcesso)
        {
            // Estado 1: Usuário instalou agora. Mostra Slides.
            await Shell.Current.GoToAsync("//WelcomePage");
        }
        else
        {
            bool existeUsuario = await _authService.ExisteCuidadorCadastrado();

            if (existeUsuario)
            {
                // Estado 3: Usuário recorrente.
                await Shell.Current.GoToAsync("//LoginPage");
            }
            else
            {
                // Estado 2: Já viu os slides, mas não terminou o cadastro.
                await Shell.Current.GoToAsync("//RegisterCuidadorPage");
            }
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_shell);
    }
}