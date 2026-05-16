using IngestaoMed.Core.ViewModels;

namespace IngestaoMed.Views;

[QueryProperty(nameof(PacienteId), "pacienteId")]
[QueryProperty(nameof(TratamentoId), "tratamentoId")]
public partial class TratamentoPage : ContentPage
{
    private readonly TratamentoViewModel _viewModel;

    public string? PacienteId { get; set; }
    public string? TratamentoId { get; set; }

    public TratamentoPage(TratamentoViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        int.TryParse(PacienteId, out var pId);
        int.TryParse(TratamentoId, out var tId);

        await _viewModel.InicializarAsync(pId, tId);
    }
}