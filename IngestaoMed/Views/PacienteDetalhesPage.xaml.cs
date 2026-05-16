using IngestaoMed.Core.ViewModels;

namespace IngestaoMed.Views;

[QueryProperty(nameof(IdRecebido), "id")]
public partial class PacienteDetalhesPage : ContentPage
{
    private readonly PacienteDetalhesViewModel _viewModel;

    public PacienteDetalhesPage(PacienteDetalhesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    public int IdRecebido
    {
        set => _ = _viewModel.InicializarAsync(value);
    }
}