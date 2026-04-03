namespace IngestaoMed
{
    public partial class App : Application
    {
        private readonly AppShell _shell;

        // O MAUI injeta o AppShell aqui automaticamente
        public App(AppShell shell)
        {
            InitializeComponent();
            _shell = shell;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Retorna a janela usando o Shell injetado
            return new Window(_shell);
        }
    }
}