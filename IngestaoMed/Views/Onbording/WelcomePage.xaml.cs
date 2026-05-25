using IngestaoMed.Core.ViewModels;

namespace IngestaoMed.Views.Onboarding;

public partial class WelcomePage : ContentPage
{
    public WelcomePage(WelcomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}