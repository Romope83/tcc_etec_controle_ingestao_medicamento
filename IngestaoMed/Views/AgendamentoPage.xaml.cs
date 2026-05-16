using IngestaoMed.Core.ViewModels;

namespace IngestaoMed.Views;

[QueryProperty(nameof(TratamentoIdStr), "tratamentoId")]
[QueryProperty(nameof(MedicamentoTratamentoIdStr), "medicamentoTratamentoId")]
public partial class AgendamentoPage : ContentPage
{
    private readonly AgendamentoViewModel _viewModel;

    public string? TratamentoIdStr { get; set; }
    public string? MedicamentoTratamentoIdStr { get; set; }

    public AgendamentoPage(AgendamentoViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        int.TryParse(TratamentoIdStr, out var tId);
        int.TryParse(MedicamentoTratamentoIdStr, out var mtId);

        await _viewModel.InicializarAsync(tId, mtId);
    }
}