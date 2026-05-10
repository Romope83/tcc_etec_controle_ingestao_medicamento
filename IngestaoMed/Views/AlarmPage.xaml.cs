using IngestaoMed.Core.ViewModels;

namespace IngestaoMed.UI.Views;

public partial class AlarmPage : ContentPage
{
    public AlarmPage(AlarmeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}