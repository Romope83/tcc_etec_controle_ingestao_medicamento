using IngestaoMed.Core.ViewModels;

namespace IngestaoMed.Views.Onboarding
{
    public partial class CuidadorPage : ContentPage
    {
        private readonly CuidadorViewModel _viewModel;

        public CuidadorPage(CuidadorViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
            _viewModel.InicializarAsync();
        }
    }
}