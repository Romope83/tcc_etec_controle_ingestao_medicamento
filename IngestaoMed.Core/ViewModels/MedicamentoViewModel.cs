using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace IngestaoMed.Core.ViewModels
{
    public partial class MedicamentoViewModel : ObservableObject
    {
        private readonly IMedicamentoService _medicamentoService;
        private readonly ITratamentoService _tratamentoService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IMediaPickerService _mediaPickerService;
        private readonly IMediaManagerService _mediaManagerService;

        private Medicamento? _medicamentoOriginal;

        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private string _nomeComercial = string.Empty;

        [ObservableProperty]
        private string _dosagem = string.Empty;

        [ObservableProperty]
        private string? _unidadeDosagem;

        [ObservableProperty]
        private string? _formaIngestao;

        [ObservableProperty]
        private string? _fotoPath;

        public MedicamentoViewModel(
            IMedicamentoService medicamentoService,
            ITratamentoService tratamentoService,
            IDialogService dialogService,
            INavigationService navigationService,
            IMediaPickerService mediaPickerService,
            IMediaManagerService mediaManagerService)
        {
            _medicamentoService = medicamentoService;
            _tratamentoService = tratamentoService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _mediaPickerService = mediaPickerService;
            _mediaManagerService = mediaManagerService;
        }

        public async Task InicializarAsync(int medicamentoId)
        {
            Id = medicamentoId;

            if (medicamentoId > 0)
            {
                _medicamentoOriginal = await _medicamentoService.BuscarPrimeiroMedicamentoAsync(m => m.Id == medicamentoId);

                if (_medicamentoOriginal != null)
                {
                    NomeComercial = _medicamentoOriginal.NomeComercial ?? string.Empty;
                    UnidadeDosagem = _medicamentoOriginal.UnidadeDosagem;
                    FormaIngestao = _medicamentoOriginal.FormaIngestao;
                    FotoPath = _medicamentoOriginal.FotoPath;
                }
            }
            else
            {
                _medicamentoOriginal = null;
                NomeComercial = string.Empty;
                Dosagem = string.Empty;
                UnidadeDosagem = null;
                FormaIngestao = null;
                FotoPath = null;
            }
        }

        [RelayCommand]
        private async Task SelecionarFotoAsync()
        {
            string acao = await _dialogService.DisplayActionSheet(
                "Selecione uma opção",
                "Cancelar",
                null,
                "Tirar Foto",
                "Escolher da Galeria");

            if (acao == "Cancelar" || string.IsNullOrEmpty(acao)) return;

            try
            {
                string? caminhoTemporario = null;

                if (acao == "Tirar Foto")
                {
                    caminhoTemporario = await _mediaPickerService.CapturarFotoAsync();
                }
                else if (acao == "Escolher da Galeria")
                {
                    caminhoTemporario = await _mediaPickerService.SelecionarFotoGaleriaAsync();
                }

                if (caminhoTemporario != null)
                {
                    if (Id > 0)
                    {
                        string? caminhoSalvo = await _mediaManagerService.RegistrarFotoPacienteAsync(Id, caminhoTemporario);
                        if (caminhoSalvo != null)
                        {
                            FotoPath = caminhoSalvo;
                        }
                    }
                    else
                    {
                        FotoPath = caminhoTemporario;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao processar imagem do medicamento: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SalvarAsync()
        {
            if (string.IsNullOrWhiteSpace(NomeComercial))
            {
                await _dialogService.DisplayAlert("Aviso", "O Nome Comercial é obrigatório.", "OK");
                return;
            }


            if (string.IsNullOrWhiteSpace(UnidadeDosagem))
            {
                await _dialogService.DisplayAlert("Aviso", "Selecione a Unidade de Dosagem.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(FormaIngestao))
            {
                await _dialogService.DisplayAlert("Aviso", "Selecione a Forma de Ingestão.", "OK");
                return;
            }

            if (Id == 0)
            {
                bool jaExiste = await _medicamentoService.ExisteMedicamentoAsync(NomeComercial, FormaIngestao);
                if (jaExiste)
                {
                    await _dialogService.DisplayAlert("Erro", "Este medicamento já está cadastrado com esta forma de ingestão.", "OK");
                    return;
                }

                var novoMedicamento = new Medicamento
                {
                    NomeComercial = NomeComercial,
                    UnidadeDosagem = UnidadeDosagem,
                    FormaIngestao = FormaIngestao,
                    FotoPath = FotoPath
                };

                bool sucesso = await _medicamentoService.AdicionarOuAtualizarMedicamentoAsync(novoMedicamento);

                if (sucesso)
                {
                    await _dialogService.DisplayAlert("Sucesso", "Medicamento cadastrado com sucesso!", "OK");
                    await _navigationService.GoToAsync("..");
                }
                else
                {
                    await _dialogService.DisplayAlert("Erro", "Não foi possível salvar o medicamento.", "OK");
                }
            }
            else
            {
                if (_medicamentoOriginal != null)
                {
                    _medicamentoOriginal.NomeComercial = NomeComercial;
                    _medicamentoOriginal.UnidadeDosagem = UnidadeDosagem;
                    _medicamentoOriginal.FormaIngestao = FormaIngestao;
                    _medicamentoOriginal.FotoPath = FotoPath;

                    bool sucesso = await _medicamentoService.AdicionarOuAtualizarMedicamentoAsync(_medicamentoOriginal);

                    if (sucesso)
                    {
                        await _dialogService.DisplayAlert("Sucesso", "Medicamento atualizado com sucesso!", "OK");
                        await _navigationService.GoToAsync("..");
                    }
                    else
                    {
                        await _dialogService.DisplayAlert("Erro", "Não foi possível atualizar o medicamento.", "OK");
                    }
                }
            }
        }

        [RelayCommand]
        private async Task ExcluirAsync()
        {
            if (Id <= 0 || _medicamentoOriginal == null) return;

            // 1. Validação de Dependência: Impede a exclusão se o remédio estiver em uso por algum tratamento ativo ou histórico
            var vinculos = await _tratamentoService.ObterOndeMedicamentoVinculadoAsync(m => m.MedicamentoId == Id);
            if (vinculos != null && vinculos.Count > 0)
            {
                await _dialogService.DisplayAlert(
                    "Não Permitido",
                    "Este medicamento não pode ser excluído pois está vinculado a um ou mais tratamentos.",
                    "OK");
                return;
            }

            // 2. Confirmação do Usuário caso esteja livre de vínculos
            bool confirmar = await _dialogService.DisplayConfirmationAsync(
                "Excluir Medicamento",
                $"Tem certeza que deseja remover {NomeComercial} definitivamente do catálogo?",
                "Sim",
                "Não");

            if (!confirmar) return;

            bool sucesso = await _medicamentoService.RemoverMedicamentoAsync(_medicamentoOriginal);

            if (sucesso)
            {
                await _dialogService.DisplayAlert("Sucesso", "Medicamento removido com sucesso!", "OK");
                await _navigationService.GoToAsync("..");
            }
            else
            {
                await _dialogService.DisplayAlert("Erro", "Não foi possível remover o medicamento.", "OK");
            }
        }
    }
}