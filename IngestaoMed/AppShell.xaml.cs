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

            // --- ADICIONE OS NOVOS REGISTROS ---
            Routing.RegisterRoute("WelcomePage", typeof(WelcomePage));
            // Se você criou a RegisterCuidadorPage, registre-a também:
            // Routing.RegisterRoute("RegisterCuidadorPage", typeof(Views.Auth.RegisterCuidadorPage));
        }
    }
}