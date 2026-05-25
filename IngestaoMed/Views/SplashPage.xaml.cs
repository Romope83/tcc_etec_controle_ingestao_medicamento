using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Services;

namespace IngestaoMed.Views.Onboarding
{
    public partial class SplashPage : ContentPage
    {
        private readonly IAuthService _authService;
        private readonly IConfigService _configService;
        private readonly INavigationService _navigationService;

        public SplashPage(IAuthService authService, IConfigService configService, INavigationService navigationService)
        {
            InitializeComponent();
            _authService = authService;
            _configService = configService;
            _navigationService = navigationService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ExecutarVerificacoesIniciaisAsync();
        }

        private async Task ExecutarVerificacoesIniciaisAsync()
        {
            await Task.Delay(500);

            if (_configService.EhPrimeiroAcesso)
            {
                await _navigationService.GoToAsync("//WelcomePage");
                return;
            }

            var config = _configService.ConfiguracaoCuidador;
            bool existeUsuario = await _authService.ExisteCuidadorCadastrado();

            if (config != null)
            {
                await _navigationService.GoToAsync("//ListaPacientesPage");
            }
            else if (existeUsuario)
            {
                await _navigationService.GoToAsync("//LoginPage");
            }
            else
            {
                await _navigationService.GoToAsync("//WelcomePage");
            }
        }
    }
}