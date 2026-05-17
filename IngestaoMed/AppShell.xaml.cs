using IngestaoMed.Views;
using IngestaoMed.Views.Onboarding;

namespace IngestaoMed
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MedicamentoPage), typeof(MedicamentoPage));
            Routing.RegisterRoute(nameof(ListaMedicamentosPage), typeof(ListaMedicamentosPage));
            Routing.RegisterRoute(nameof(PacientePage), typeof(PacientePage));
            Routing.RegisterRoute(nameof(PacienteDetalhesPage), typeof(PacienteDetalhesPage));
            Routing.RegisterRoute(nameof(TratamentoPage), typeof(TratamentoPage));
            Routing.RegisterRoute(nameof(AgendamentoPage), typeof(AgendamentoPage));
            Routing.RegisterRoute(nameof(PacientePage), typeof(PacientePage));
            Routing.RegisterRoute(nameof(CuidadorPage), typeof(CuidadorPage));
            Routing.RegisterRoute(nameof(ListaTratamentosPage), typeof(ListaTratamentosPage));

        }
    }
}