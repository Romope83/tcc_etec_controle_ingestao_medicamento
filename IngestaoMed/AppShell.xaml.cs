using IngestaoMed.Views;
using IngestaoMed.Views.Onboarding; // Namespace da sua nova pasta

namespace IngestaoMed
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // --- MANTENHA SEUS REGISTROS ORIGINAIS ---
            Routing.RegisterRoute(nameof(Views.CadastroMedicamentoPage), typeof(Views.CadastroMedicamentoPage));
            Routing.RegisterRoute(nameof(Views.ListaMedicamentosPage), typeof(Views.ListaMedicamentosPage));
            Routing.RegisterRoute(nameof(ListaPacientesPage), typeof(ListaPacientesPage));
            Routing.RegisterRoute(nameof(CadastroPacientePage), typeof(CadastroPacientePage));
            Routing.RegisterRoute(nameof(PacienteDetalhesPage), typeof(PacienteDetalhesPage));
            // --- ADICIONE OS NOVOS REGISTROS ---
            Routing.RegisterRoute("WelcomePage", typeof(WelcomePage));
            Routing.RegisterRoute("RegisterCuidadorPage", typeof(Views.Onboarding.RegisterCuidadorPage));
        }
    }
}