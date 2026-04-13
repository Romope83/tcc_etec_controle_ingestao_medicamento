using IngestaoMed.Core.ViewModels;

namespace IngestaoMed.Views
{
    public partial class ListaMedicamentosPage : ContentPage
    {
        private readonly ListaMedicamentosViewModel _viewModel;

        // Injeção de Dependência: O MAUI resolve o ViewModel automaticamente
        public ListaMedicamentosPage(ListaMedicamentosViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        /// <summary>
        /// Sobrescrevemos o OnAppearing para garantir que a lista seja 
        /// atualizada toda vez que o usuário navegar para esta tela.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Executa o comando de carregamento da ViewModel de forma assíncrona
            if (_viewModel.CarregarMedicamentosCommand.CanExecute(null))
            {
                await _viewModel.CarregarMedicamentosCommand.ExecuteAsync(null);
            }
        }
    }
}