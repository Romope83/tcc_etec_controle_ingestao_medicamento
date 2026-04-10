using IngestaoMed.Core.ViewModels;

namespace IngestaoMed.Views
{
    public partial class CadastroMedicamentoPage : ContentPage
    {
        public CadastroMedicamentoPage(CadastroMedicamentoViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}