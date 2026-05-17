using IngestaoMed.Views;
using IngestaoMed.Views.Onboarding; // Namespace da sua nova pasta

namespace IngestaoMed
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(Views.MedicamentoPage), typeof(Views.MedicamentoPage));
            Routing.RegisterRoute(nameof(Views.ListaMedicamentosPage), typeof(Views.ListaMedicamentosPage));
            Routing.RegisterRoute(nameof(PacientePage), typeof(PacientePage));
            Routing.RegisterRoute(nameof(PacienteDetalhesPage), typeof(PacienteDetalhesPage));
            Routing.RegisterRoute(nameof(TratamentoPage), typeof(TratamentoPage));
            Routing.RegisterRoute(nameof(AgendamentoPage), typeof(AgendamentoPage));
            Routing.RegisterRoute(nameof(PacientePage), typeof(PacientePage));
            Routing.RegisterRoute(nameof(ListaTratamentosPage), typeof(ListaTratamentosPage));

        }
    }
}