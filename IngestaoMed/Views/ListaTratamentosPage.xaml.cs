using IngestaoMed.Core.ViewModels;

namespace IngestaoMed.Views;

public partial class ListaTratamentosPage : ContentPage
{
    private readonly ListaTratamentosViewModel _viewModel;

    public ListaTratamentosPage(ListaTratamentosViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CarregarTratamentosAsync();
    }
}