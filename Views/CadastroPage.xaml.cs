using IngestaoMed.ViewModels;

namespace IngestaoMed.Views;

public partial class CadastroPage : ContentPage
{
    public CadastroPage(CadastroViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}