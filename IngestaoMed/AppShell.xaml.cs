using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Services;
using IngestaoMed.UI.Views;
using IngestaoMed.Views;
using IngestaoMed.Views.Onboarding;
using System.Windows.Input;

namespace IngestaoMed
{
    public partial class AppShell : Shell
    {

        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly IAgendamentoService _agendamentoService;
        private readonly IEmailOutboxService _emailOutboxService;

        public ICommand LogoutCommand { get; }
        public AppShell(IAuthService authService, INavigationService navigationService, IAgendamentoService agendamentoService, IEmailOutboxService emailOutboxService)
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
            Routing.RegisterRoute(nameof(AlarmPage), typeof(AlarmPage));
            Routing.RegisterRoute(nameof(SplashPage), typeof(SplashPage));

            _authService = authService;
            _navigationService = navigationService;
            _agendamentoService = agendamentoService;
            _emailOutboxService = emailOutboxService;

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
        protected override async void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            if (Handler is not null)
            {
                await _agendamentoService.SincronizarFilaDeAlarmesAsync();
                await _emailOutboxService.LimparFilaAntigaAsync();

            }
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            await _agendamentoService.SincronizarFilaDeAlarmesAsync();
        }



        //protected override void OnNavigating(ShellNavigatingEventArgs args)
        //{
        //    base.OnNavigating(args);
        //    if (args.Source == ShellNavigationSource.ShellSectionChanged)
        //    {
        //        var navigationStack = Current.Navigation.NavigationStack.ToList();

        //        for (int i = navigationStack.Count - 2; i >= 0; i--)
        //        {
        //            Current.Navigation.RemovePage(navigationStack[i]);
        //        }
        //    }
        //}
    }
}