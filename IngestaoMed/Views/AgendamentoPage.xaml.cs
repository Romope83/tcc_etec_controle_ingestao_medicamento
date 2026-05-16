using IngestaoMed.Core.ViewModels;


namespace IngestaoMed.Views;

[QueryProperty(nameof(TratamentoIdStr), "tratamentoId")]
public partial class AgendamentoPage : ContentPage
{
    private readonly AgendamentoViewModel _viewModel;

    public string? TratamentoIdStr { get; set; }

    public AgendamentoPage(AgendamentoViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (int.TryParse(TratamentoIdStr, out var tId))
        {
            await _viewModel.InicializarAsync(tId);
        }
    }
}