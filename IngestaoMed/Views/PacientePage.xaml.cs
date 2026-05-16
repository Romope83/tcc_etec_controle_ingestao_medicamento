using IngestaoMed.Core.ViewModels;

namespace IngestaoMed.Views
{
    [QueryProperty(nameof(IdRecebido), "id")]
    public partial class PacientePage : ContentPage
    {
        private readonly PacienteViewModel _viewModel;

        public PacientePage(PacienteViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        public int IdRecebido
        {
            set => _ = _viewModel.InicializarAsync(value);
        }
    }
}