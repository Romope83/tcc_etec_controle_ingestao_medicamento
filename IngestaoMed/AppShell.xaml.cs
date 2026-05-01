using IngestaoMed.Views;

namespace IngestaoMed
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registramos as rotas das páginas que não estão fixas no ShellContent do XAML
            // ou que queremos garantir o acesso via INavigationService.

            Routing.RegisterRoute(nameof(Views.CadastroMedicamentoPage), typeof(Views.CadastroMedicamentoPage));
            Routing.RegisterRoute(nameof(Views.ListaMedicamentosPage), typeof(Views.ListaMedicamentosPage));
            Routing.RegisterRoute(nameof(ListaPacientesPage), typeof(ListaPacientesPage));
            Routing.RegisterRoute(nameof(CadastroPacientePage), typeof(CadastroPacientePage));
            // Dica: Se CadastroPage e LoginPage já estão no XAML com Route="...", 
            // o Shell já as reconhece, mas registrar aqui com nameof evita erros de digitação.
        }
    }
}