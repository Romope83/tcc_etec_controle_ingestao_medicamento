using IngestaoMed.Core.ViewModels;

namespace IngestaoMed.Views
{
    public partial class CadastroPacientePage : ContentPage
    {
        public CadastroPacientePage(CadastroPacienteViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}