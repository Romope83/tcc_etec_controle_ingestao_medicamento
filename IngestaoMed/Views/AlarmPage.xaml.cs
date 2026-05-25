using IngestaoMed.Core.ViewModels;

namespace IngestaoMed.UI.Views
{
    [QueryProperty(nameof(AgendamentoIdStr), "agendamentoId")]
    public partial class AlarmPage : ContentPage
    {
        private readonly AlarmeViewModel _viewModel;

        public string AgendamentoIdStr
        {
            set
            {
                if (int.TryParse(value, out int id) && _viewModel != null)
                {
                    _viewModel.AgendamentoId = id;
                }
            }
        }

        public AlarmPage(AlarmeViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
    }
}