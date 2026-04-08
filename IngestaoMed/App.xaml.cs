using IngestaoMed;
using IngestaoMed.Core.Services;

namespace IngestaoMed;

public partial class App : Application
{
    private readonly IAuthService _authService;
    private readonly AppShell _shell;

    public App(IAuthService authService, AppShell shell)
    {
        InitializeComponent();
        _authService = authService;
        _shell = shell;
    }


    protected override async void OnStart()
    {
        base.OnStart();
        await WindowsInicialization();
    }

    private async Task WindowsInicialization()
    {
        // Verifica se já existe algum cuidador no SQLite
        bool existeUsuario = await _authService.ExisteCuidadorCadastrado();

        if (existeUsuario)
        {
            // Se existe, mandamos para o Login (ou Home, se preferir sem senha)
            await Shell.Current.GoToAsync("//LoginPage");
        }
        else
        {
            // Se não existe, mantém no Cadastro
            await Shell.Current.GoToAsync("//CadastroPage");
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_shell);
    }
}