using IngestaoMed.Core.ViewModels;

namespace IngestaoMed.Views
{
    [QueryProperty(nameof(IdRecebido), "id")]
    public partial class MedicamentoPage : ContentPage
    {
        private readonly MedicamentoViewModel _viewModel;

        public MedicamentoPage(MedicamentoViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public int IdRecebido
        {
            set
            {
                int id = value;
                _ = _viewModel.InicializarAsync(id);
            }
        }
    }
}