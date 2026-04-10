using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;

namespace IngestaoMed.Core.ViewModels
{
    public partial class CadastroMedicamentoViewModel : ObservableObject
    {
        private readonly IMedicamentoService _medicamentoService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _nomeComercial = string.Empty;

        [ObservableProperty]
        private string? _dosagem;

        [ObservableProperty]
        private string? _formaIngestao;

        // Lista para preencher o Picker na tela
        public List<string> FormasIngestaoDisponiveis { get; } = new()
        {
            "Comprimido", "Cápsula", "Gotas", "Xarope", "Injeção", "Pomada"
        };

        public CadastroMedicamentoViewModel(
            IMedicamentoService medicamentoService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _medicamentoService = medicamentoService;
            _dialogService = dialogService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task SalvarAsync()
        {
            // 1. Validação simples de UI (SRP)
            if (string.IsNullOrWhiteSpace(NomeComercial))
            {
                await _dialogService.DisplayAlert("Aviso", "O Nome Comercial é obrigatório.", "OK");
                return;
            }

            // 2. Construção do Modelo (SetsRequiredMembers garantido)
            var novoMedicamento = new Medicamento
            {
                NomeComercial = NomeComercial,
                Dosagem = Dosagem,
                FormaIngestao = FormaIngestao
                // FotoPath pode ser adicionado futuramente através de um MediaPicker
            };

            // 3. Persistência via Serviço Abstraído
            bool sucesso = await _medicamentoService.AdicionarMedicamentoAsync(novoMedicamento);

            if (sucesso)
            {
                await _dialogService.DisplayAlert("Sucesso", "Medicamento cadastrado com sucesso!", "OK");
                // Retorna para a tela anterior (Lista de medicamentos ou Home)
                await _navigationService.GoToAsync("..");
            }
            else
            {
                await _dialogService.DisplayAlert("Erro", "Não foi possível salvar o medicamento.", "OK");
            }
        }
    }
}