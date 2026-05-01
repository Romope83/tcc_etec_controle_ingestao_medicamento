using IngestaoMed.Core.ViewModels;

namespace IngestaoMed.Views
{
    public partial class ListaPacientesPage : ContentPage
    {
        private readonly ListaPacientesViewModel _viewModel;

        // Injeção de Dependência: O MAUI resolve o ViewModel automaticamente
        public ListaPacientesPage(ListaPacientesViewModel viewModel)
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

            // Damos um pequeno respiro para o sistema de UI
            await Task.Delay(100);

            if (_viewModel.CarregarPacientesCommand.CanExecute(null))
            {
                await _viewModel.CarregarPacientesCommand.ExecuteAsync(null);
            }
        }
    }
}