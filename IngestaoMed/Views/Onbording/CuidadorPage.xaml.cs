using IngestaoMed.Core.ViewModels;

namespace IngestaoMed.Views.Onboarding;

public partial class CuidadorPage : ContentPage
{

    public CuidadorPage(CuidadorViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}