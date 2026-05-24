using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Services;
using IngestaoMed.Views;
using IngestaoMed.Views.Onboarding;
using System.Windows.Input;

namespace IngestaoMed
{
    public partial class AppShell : Shell
    {

        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        public ICommand LogoutCommand { get; }
        public AppShell(IAuthService authService, INavigationService navigationService)
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MedicamentoPage), typeof(MedicamentoPage));
            Routing.RegisterRoute(nameof(ListaMedicamentosPage), typeof(ListaMedicamentosPage));
            Routing.RegisterRoute(nameof(PacientePage), typeof(PacientePage));
            Routing.RegisterRoute(nameof(PacienteDetalhesPage), typeof(PacienteDetalhesPage));
            Routing.RegisterRoute(nameof(TratamentoPage), typeof(TratamentoPage));
            Routing.RegisterRoute(nameof(AgendamentoPage), typeof(AgendamentoPage));
            Routing.RegisterRoute(nameof(ListaPacientesPage), typeof(ListaPacientesPage));
            Routing.RegisterRoute(nameof(CuidadorPage), typeof(CuidadorPage));
            Routing.RegisterRoute(nameof(ListaTratamentosPage), typeof(ListaTratamentosPage));

            _authService = authService;
            _navigationService = navigationService;

            LogoutCommand = new Command(async () => await ExecutarLogout());
            BindingContext = this;

        }
        private async Task ExecutarLogout()
        {
            var aceitou = await DisplayAlert("Sair", "Deseja realmente sair do aplicativo?", "Sim", "Não");
            if (aceitou)
            {
                await _authService.FazerLogout();
                await _navigationService.GoToAsync("//LoginPage");
            }
        }
    }
}